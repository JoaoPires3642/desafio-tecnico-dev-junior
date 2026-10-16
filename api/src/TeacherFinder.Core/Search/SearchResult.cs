using TeacherFinder.Core.Model;

namespace TeacherFinder.Core.Search;

/// <summary>Professor que atende ao pedido, com os horários de início possíveis.</summary>
public sealed record TeacherMatch(
    int Id,
    string Name,
    int ClassesPerWeek,
    IReadOnlyList<TimeOfDay> AvailableSlots);

/// <summary>Professor que ficou de fora, com o motivo.</summary>
public sealed record IneligibleTeacher(int Id, string Name, string Reason);

/// <summary>Resultado completo da busca.</summary>
public sealed record SearchResult(
    IReadOnlyList<TeacherMatch> Teachers,
    IReadOnlyList<IneligibleTeacher> Ineligible);
