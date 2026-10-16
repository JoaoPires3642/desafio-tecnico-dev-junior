using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeacherFinder.Api.Tests;

/// <summary>
/// Espelha o contrato JSON da API do lado do teste: se alguém renomear uma chave
/// na resposta, estes testes quebram. Por isso não reutilizamos os DTOs da API.
/// </summary>
internal sealed record BuscaResposta(
    [property: JsonPropertyName("professores")] List<ProfessorElegivel> Professores,
    [property: JsonPropertyName("naoElegiveis")] List<ProfessorInelegivel> NaoElegiveis);

internal sealed record ProfessorElegivel(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("aulasNaSemana")] int AulasNaSemana,
    [property: JsonPropertyName("horarios")] List<string> Horarios);

internal sealed record ProfessorInelegivel(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nome")] string Nome,
    [property: JsonPropertyName("motivo")] string Motivo);

internal sealed record ErroResposta(
    [property: JsonPropertyName("mensagem")] string Mensagem,
    [property: JsonPropertyName("erros")] List<string> Erros);

internal static class Json
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}
