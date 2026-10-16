using TeacherFinder.Core.Model;

namespace TeacherFinder.Core.Search;

/// <summary>
/// Valida o pedido e devolve todos os problemas de uma vez, com mensagens claras.
/// Fica no Core (e não no controller) para ser testado sem subir a API.
/// </summary>
public sealed class SearchRequestValidator
{
    private static readonly int[] AllowedDurations = { 30, 60, 90 };

    public ValidationResult Validate(SearchRequestInput input)
    {
        var errors = new List<string>();

        var language = input.Language?.Trim();
        if (string.IsNullOrEmpty(language))
        {
            errors.Add("O idioma é obrigatório.");
        }

        var days = new List<Weekday>();
        if (input.Days is null || input.Days.Count == 0)
        {
            errors.Add("Informe pelo menos um dia da semana.");
        }
        else
        {
            foreach (var raw in input.Days)
            {
                if (!Weekdays.TryParse(raw, out var day))
                {
                    errors.Add($"Dia inválido: '{raw}'.");
                }
                else if (!days.Contains(day))
                {
                    days.Add(day);
                }
            }
        }

        var hasStart = TimeOfDay.TryParse(input.StartWindow, out var start);
        if (!hasStart)
        {
            errors.Add($"Horário inválido: '{input.StartWindow}'. Use o formato HH:mm.");
        }

        var hasEnd = TimeOfDay.TryParse(input.EndWindow, out var end);
        if (!hasEnd)
        {
            errors.Add($"Horário inválido: '{input.EndWindow}'. Use o formato HH:mm.");
        }

        if (hasStart && hasEnd && end.Minutes <= start.Minutes)
        {
            errors.Add("O fim da faixa de horário deve ser depois do início.");
        }

        var duration = input.DurationMinutes ?? 0;
        if (input.DurationMinutes is null || Array.IndexOf(AllowedDurations, duration) < 0)
        {
            errors.Add("A duração da aula deve ser 30, 60 ou 90 minutos.");
        }

        if (errors.Count > 0)
        {
            return ValidationResult.Invalid(errors);
        }

        var request = new SearchRequest(language!, days, start, end, duration);
        return ValidationResult.Valid(request);
    }
}
