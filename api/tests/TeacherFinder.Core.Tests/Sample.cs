using TeacherFinder.Core.Model;
using TeacherFinder.Core.Search;

namespace TeacherFinder.Core.Tests;

/// <summary>Atalhos para montar dados de teste de forma legível.</summary>
internal static class Sample
{
    public static TimeOfDay At(string hhmm) =>
        TimeOfDay.TryParse(hhmm, out var value)
            ? value
            : throw new ArgumentException($"Hora inválida no teste: {hhmm}");

    public static Weekday Day(string name) =>
        Weekdays.TryParse(name, out var value)
            ? value
            : throw new ArgumentException($"Dia inválido no teste: {name}");

    public static AvailabilityWindow Window(string day, string start, string end) =>
        new(Day(day), At(start), At(end));

    public static ScheduledClass Class(string day, string start, int durationMinutes) =>
        new(Day(day), At(start), durationMinutes);

    public static Teacher Teacher(
        int id,
        string name,
        string[] languages,
        bool scheduleBlocked = false,
        AvailabilityWindow[]? availability = null,
        ScheduledClass[]? scheduledClasses = null) =>
        new(
            id,
            name,
            languages,
            scheduleBlocked,
            availability ?? Array.Empty<AvailabilityWindow>(),
            scheduledClasses ?? Array.Empty<ScheduledClass>());

    public static SearchRequest Request(string language, string[] days, string start, string end, int durationMinutes) =>
        new(language, days.Select(Day).ToList(), At(start), At(end), durationMinutes);
}
