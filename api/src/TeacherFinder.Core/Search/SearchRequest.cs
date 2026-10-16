using TeacherFinder.Core.Model;

namespace TeacherFinder.Core.Search;

/// <summary>Pedido de busca já validado e convertido para tipos do domínio.</summary>
public sealed record SearchRequest(
    string Language,
    IReadOnlyList<Weekday> Days,
    TimeOfDay StartWindow,
    TimeOfDay EndWindow,
    int DurationMinutes);
