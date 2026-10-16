using System.Text.Json.Serialization;

namespace TeacherFinder.Api.Contracts;

/// <summary>
/// Corpo de POST /api/professores/busca. Os identificadores são em inglês, mas os
/// nomes no JSON seguem o contrato em português (via <see cref="JsonPropertyNameAttribute"/>).
/// Tudo é opcional aqui de propósito: quem decide o que é erro é o validador do Core.
/// </summary>
public sealed record SearchRequestDto(
    [property: JsonPropertyName("idioma")] string? Language,
    [property: JsonPropertyName("dias")] List<string>? Days,
    [property: JsonPropertyName("inicioFaixa")] string? StartWindow,
    [property: JsonPropertyName("fimFaixa")] string? EndWindow,
    [property: JsonPropertyName("duracaoMinutos")] int? DurationMinutes);
