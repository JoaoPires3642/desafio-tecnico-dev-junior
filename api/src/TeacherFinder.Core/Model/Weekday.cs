using System.Globalization;

namespace TeacherFinder.Core.Model;

/// <summary>Dia da semana.</summary>
public enum Weekday
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday,
}

/// <summary>
/// Converte entre os textos em português usados na API/dados ("segunda".."domingo")
/// e o enum <see cref="Weekday"/>. A comparação ignora maiúsculas/minúsculas e,
/// por conveniência, também aceita as formas acentuadas ("terça", "sábado").
/// </summary>
public static class Weekdays
{
    private static readonly Dictionary<string, Weekday> ByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["segunda"] = Weekday.Monday,
        ["terca"] = Weekday.Tuesday,
        ["terça"] = Weekday.Tuesday,
        ["quarta"] = Weekday.Wednesday,
        ["quinta"] = Weekday.Thursday,
        ["sexta"] = Weekday.Friday,
        ["sabado"] = Weekday.Saturday,
        ["sábado"] = Weekday.Saturday,
        ["domingo"] = Weekday.Sunday,
    };

    public static bool TryParse(string? value, out Weekday weekday)
    {
        weekday = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return ByName.TryGetValue(value.Trim(), out weekday);
    }

    /// <summary>Forma canônica usada na API (sem acento), útil em mensagens e testes.</summary>
    public static string ToWireValue(Weekday weekday) => weekday switch
    {
        Weekday.Monday => "segunda",
        Weekday.Tuesday => "terca",
        Weekday.Wednesday => "quarta",
        Weekday.Thursday => "quinta",
        Weekday.Friday => "sexta",
        Weekday.Saturday => "sabado",
        _ => "domingo",
    };
}
