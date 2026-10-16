namespace TeacherFinder.Core.Search;

/// <summary>Resultado da validação: ou um pedido pronto, ou a lista de erros.</summary>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors, SearchRequest? Request)
{
    public static ValidationResult Invalid(IReadOnlyList<string> errors) => new(false, errors, null);

    public static ValidationResult Valid(SearchRequest request) =>
        new(true, Array.Empty<string>(), request);
}
