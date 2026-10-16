using System.Globalization;
using TeacherFinder.Core.Model;

namespace TeacherFinder.Core.Search;

/// <summary>
/// Motor da busca: decide quem atende ao pedido e quais horários cada um tem.
/// É puro (não conhece HTTP nem arquivos), então dá para testar direto.
/// </summary>
public sealed class TeacherSearch
{
    /// <summary>Aula só pode começar em hora cheia ou meia hora.</summary>
    private const int SlotStepMinutes = 30;

    /// <summary>Desempate alfabético por nome, em pt-BR e ignorando acentos na comparação.</summary>
    private static readonly StringComparer NameComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), CompareOptions.None);

    public SearchResult Search(IReadOnlyList<Teacher> teachers, SearchRequest request)
    {
        var matches = new List<TeacherMatch>();
        // Guarda o professor junto do motivo para poder ordenar por aulas na semana.
        var ineligible = new List<(Teacher Teacher, string Reason)>();

        foreach (var teacher in teachers)
        {
            // A ordem dos motivos importa: vale o primeiro que se aplica.
            if (!TeachesLanguage(teacher, request.Language))
            {
                ineligible.Add((teacher, IneligibilityReasons.DoesNotTeachLanguage));
                continue;
            }

            if (teacher.ScheduleBlocked)
            {
                ineligible.Add((teacher, IneligibilityReasons.ScheduleBlocked));
                continue;
            }

            var slots = FindSlots(teacher, request);
            if (slots.Count == 0)
            {
                ineligible.Add((teacher, IneligibilityReasons.NoCommonSlot));
                continue;
            }

            matches.Add(new TeacherMatch(teacher.Id, teacher.Name, teacher.ClassesPerWeek, slots));
        }

        var orderedMatches = matches
            .OrderBy(match => match.ClassesPerWeek)
            .ThenBy(match => match.Name, NameComparer)
            .ThenBy(match => match.Id)
            .ToList();

        // Mesma regra dos elegíveis: menos aulas na semana primeiro, depois nome.
        var orderedIneligible = ineligible
            .OrderBy(entry => entry.Teacher.ClassesPerWeek)
            .ThenBy(entry => entry.Teacher.Name, NameComparer)
            .ThenBy(entry => entry.Teacher.Id)
            .Select(entry => new IneligibleTeacher(entry.Teacher.Id, entry.Teacher.Name, entry.Reason))
            .ToList();

        return new SearchResult(orderedMatches, orderedIneligible);
    }

    private static bool TeachesLanguage(Teacher teacher, string language) =>
        teacher.Languages.Any(t =>
            string.Equals(t?.Trim(), language.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Horários de início possíveis, em ordem crescente. Um horário só vale se a aula
    /// couber inteira na faixa pedida, em todos os dias pedidos, dentro de algum bloco
    /// de disponibilidade e sem bater em aula já marcada.
    /// </summary>
    private static List<TimeOfDay> FindSlots(Teacher teacher, SearchRequest request)
    {
        var slots = new List<TimeOfDay>();

        // Alinha no primeiro múltiplo de 30 a partir do início da faixa (ex.: 18:15 -> 18:30).
        var firstStart = RoundUpToStep(request.StartWindow.Minutes);
        var lastStart = request.EndWindow.Minutes - request.DurationMinutes;

        for (var start = firstStart; start <= lastStart; start += SlotStepMinutes)
        {
            if (request.Days.All(day => IsFree(teacher, day, start, request.DurationMinutes)))
            {
                slots.Add(new TimeOfDay(start));
            }
        }

        return slots;
    }

    private static bool IsFree(Teacher teacher, Weekday day, int startMinutes, int durationMinutes)
    {
        var start = new TimeOfDay(startMinutes);

        var fitsInSomeWindow = teacher.Availability.Any(window =>
            window.Day == day && window.FitsWithin(start, durationMinutes));

        if (!fitsInSomeWindow)
        {
            return false;
        }

        var clashesWithClass = teacher.ScheduledClasses.Any(scheduled =>
            scheduled.Day == day && scheduled.OverlapsWith(start, durationMinutes));

        return !clashesWithClass;
    }

    private static int RoundUpToStep(int minutes) =>
        (minutes + SlotStepMinutes - 1) / SlotStepMinutes * SlotStepMinutes;
}
