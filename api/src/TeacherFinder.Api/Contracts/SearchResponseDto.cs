using System.Text.Json.Serialization;

namespace TeacherFinder.Api.Contracts;

/// <summary>Professor elegível, com os horários de início que ele tem livres.</summary>
public sealed record TeacherMatchDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("aulasNaSemana")] int ClassesPerWeek,
    [property: JsonPropertyName("horarios")] IReadOnlyList<string> AvailableSlots);

/// <summary>Professor que ficou de fora, com o motivo.</summary>
public sealed record IneligibleTeacherDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("motivo")] string Reason);

/// <summary>Corpo de resposta de sucesso da busca.</summary>
public sealed record SearchResponseDto(
    [property: JsonPropertyName("professores")] IReadOnlyList<TeacherMatchDto> Teachers,
    [property: JsonPropertyName("naoElegiveis")] IReadOnlyList<IneligibleTeacherDto> Ineligible);

/// <summary>Corpo de resposta para pedidos inválidos (HTTP 400).</summary>
public sealed record ValidationErrorResponse(
    [property: JsonPropertyName("mensagem")] string Message,
    [property: JsonPropertyName("erros")] IReadOnlyList<string> Errors);
