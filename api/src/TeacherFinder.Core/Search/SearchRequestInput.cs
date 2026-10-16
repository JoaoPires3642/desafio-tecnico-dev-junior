namespace TeacherFinder.Core.Search;

/// <summary>
/// O que chega "cru" do mundo externo (strings, campos possivelmente ausentes).
/// O validador transforma isto num <see cref="SearchRequest"/> ou numa lista de erros.
/// </summary>
public sealed record SearchRequestInput(
    string? Language,
    IReadOnlyList<string>? Days,
    string? StartWindow,
    string? EndWindow,
    int? DurationMinutes);
