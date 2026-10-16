using System.Globalization;

namespace TeacherFinder.Core.Model;

/// <summary>
/// Um horário dentro do dia, guardado como minutos desde a meia-noite (00:00 = 0).
/// Os dados garantem que nada atravessa a meia-noite, então um <see cref="int"/> basta
/// e evita as armadilhas de aritmética com <see cref="TimeOnly"/> em bordas de faixa.
/// </summary>
public readonly record struct TimeOfDay : IComparable<TimeOfDay>
{
    private const int MinutesInDay = 24 * 60;

    public TimeOfDay(int minutes) => Minutes = minutes;

    /// <summary>Minutos desde a meia-noite.</summary>
    public int Minutes { get; }

    /// <summary>Faz o parse de um horário no formato exato HH:mm.</summary>
    public static bool TryParse(string? text, out TimeOfDay value)
    {
        value = default;
        if (text is null || text.Length != 5 || text[2] != ':')
        {
            return false;
        }

        if (!char.IsAsciiDigit(text[0]) || !char.IsAsciiDigit(text[1]) ||
            !char.IsAsciiDigit(text[3]) || !char.IsAsciiDigit(text[4]))
        {
            return false;
        }

        var hours = ((text[0] - '0') * 10) + (text[1] - '0');
        var minutes = ((text[3] - '0') * 10) + (text[4] - '0');

        if (hours > 23 || minutes > 59)
        {
            return false;
        }

        value = new TimeOfDay((hours * 60) + minutes);
        return true;
    }

    public int CompareTo(TimeOfDay other) => Minutes.CompareTo(other.Minutes);

    public override string ToString() =>
        (Minutes / 60).ToString("D2", CultureInfo.InvariantCulture) + ":" +
        (Minutes % 60).ToString("D2", CultureInfo.InvariantCulture);
}
