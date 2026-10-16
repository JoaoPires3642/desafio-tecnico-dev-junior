using System.Text.Json;
using System.Text.Json.Serialization;
using TeacherFinder.Core.Model;

namespace TeacherFinder.Api.Infrastructure;

/// <summary>
/// Lê o JSON de professores (o formato do professores.json) e converte para o domínio.
/// Os garantidos pelo enunciado são conferidos aqui: se o arquivo vier torto, falha
/// com uma mensagem que diz qual professor e qual campo estão errados.
/// </summary>
public static class TeachersJsonReader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static IReadOnlyList<Teacher> Read(string json)
    {
        var root = JsonSerializer.Deserialize<FileRoot>(json, Options)
            ?? throw new InvalidDataException("O arquivo de professores está vazio.");

        var teachers = root.Professors
            ?? throw new InvalidDataException("O arquivo de professores não tem a lista 'professores'.");

        return teachers.Select(ToDomain).ToList();
    }

    private static Teacher ToDomain(FileTeacher teacher)
    {
        var availability = (teacher.Availability ?? new List<FileWindow>())
            .Select(window => new AvailabilityWindow(
                ParseWeekday(window.Day, teacher),
                ParseTime(window.Start, teacher),
                ParseTime(window.End, teacher)))
            .ToList();

        var scheduledClasses = (teacher.ScheduledClasses ?? new List<FileClass>())
            .Select(scheduled => new ScheduledClass(
                ParseWeekday(scheduled.Day, teacher),
                ParseTime(scheduled.Start, teacher),
                scheduled.DurationMinutes))
            .ToList();

        return new Teacher(
            teacher.Id,
            teacher.Name ?? string.Empty,
            teacher.Languages ?? new List<string>(),
            teacher.ScheduleBlocked,
            availability,
            scheduledClasses);
    }

    private static Weekday ParseWeekday(string? value, FileTeacher teacher) =>
        Weekdays.TryParse(value, out var weekday)
            ? weekday
            : throw new InvalidDataException($"Professor {teacher.Id}: dia inválido '{value}'.");

    private static TimeOfDay ParseTime(string? value, FileTeacher teacher) =>
        TimeOfDay.TryParse(value, out var time)
            ? time
            : throw new InvalidDataException($"Professor {teacher.Id}: horário inválido '{value}' (esperado HH:mm).");

    private sealed record FileRoot(
        [property: JsonPropertyName("professores")] List<FileTeacher>? Professors);

    private sealed record FileTeacher(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("nome")] string? Name,
        [property: JsonPropertyName("idiomas")] List<string>? Languages,
        [property: JsonPropertyName("agendaBloqueada")] bool ScheduleBlocked,
        [property: JsonPropertyName("disponibilidade")] List<FileWindow>? Availability,
        [property: JsonPropertyName("aulasMarcadas")] List<FileClass>? ScheduledClasses);

    private sealed record FileWindow(
        [property: JsonPropertyName("dia")] string? Day,
        [property: JsonPropertyName("inicio")] string? Start,
        [property: JsonPropertyName("fim")] string? End);

    private sealed record FileClass(
        [property: JsonPropertyName("dia")] string? Day,
        [property: JsonPropertyName("inicio")] string? Start,
        [property: JsonPropertyName("duracaoMinutos")] int DurationMinutes);
}
