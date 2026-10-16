using System.Net;
using System.Net.Http.Json;

namespace TeacherFinder.Api.Tests;

public sealed class SearchEndpointTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public SearchEndpointTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object Pedido(string idioma, string[] dias, string inicio, string fim, int duracao) =>
        new { idioma, dias, inicioFaixa = inicio, fimFaixa = fim, duracaoMinutos = duracao };

    private async Task<BuscaResposta> BuscarOk(object pedido)
    {
        var response = await _client.PostAsJsonAsync("/api/professores/busca", pedido);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<BuscaResposta>(Json.Options))!;
    }

    [Fact]
    public void Arquivo_de_professores_esta_disponivel_para_o_teste()
    {
        Assert.True(File.Exists(ApiFactory.ProfessoresPath), $"Não encontrei {ApiFactory.ProfessoresPath}");
    }

    [Fact]
    public async Task Exemplo_do_enunciado_devolve_ana_e_diego_nesta_ordem()
    {
        var resposta = await BuscarOk(Pedido("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(2, resposta.Professores.Count);

        Assert.Equal(1, resposta.Professores[0].Id);
        Assert.Equal("Ana", resposta.Professores[0].Nome);
        Assert.Equal(1, resposta.Professores[0].AulasNaSemana);
        Assert.Equal(new[] { "18:00" }, resposta.Professores[0].Horarios);

        Assert.Equal(4, resposta.Professores[1].Id);
        Assert.Equal("Diego", resposta.Professores[1].Nome);
        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, resposta.Professores[1].Horarios);
    }

    [Fact]
    public async Task Nao_elegiveis_trazem_motivo_e_ordem_por_aulas_depois_nome()
    {
        var resposta = await BuscarOk(Pedido("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(
            new[] { "Carla", "Elisa", "Fábio", "Bruno", "Gabriela", "Heitor" },
            resposta.NaoElegiveis.Select(ineligivel => ineligivel.Nome));

        Assert.All(resposta.NaoElegiveis, ineligivel => Assert.Equal("Não ensina o idioma", ineligivel.Motivo));
    }

    [Fact]
    public async Task Mistura_de_motivos_mantem_a_ordem_esperada()
    {
        // Só Fábio tem disponibilidade na sexta de manhã.
        var resposta = await BuscarOk(Pedido("Inglês", new[] { "sexta" }, "09:00", "12:00", 60));

        var fabio = Assert.Single(resposta.Professores);
        Assert.Equal("Fábio", fabio.Nome);
        // 09:30 e 10:00 batem na aula da sexta; 11:00 encosta no fim dela.
        Assert.Equal(new[] { "09:00", "11:00" }, fabio.Horarios);

        Assert.Equal(
            new[] { "Carla", "Elisa", "Ana", "Diego", "Bruno", "Gabriela", "Heitor" },
            resposta.NaoElegiveis.Select(ineligivel => ineligivel.Nome));

        Assert.Equal("Agenda bloqueada", resposta.NaoElegiveis.Single(i => i.Nome == "Carla").Motivo);
        Assert.Equal("Não ensina o idioma", resposta.NaoElegiveis.Single(i => i.Nome == "Diego").Motivo);
        Assert.Equal("Sem horário em comum nos dias pedidos", resposta.NaoElegiveis.Single(i => i.Nome == "Elisa").Motivo);
    }

    [Fact]
    public async Task Idioma_sem_professor_devolve_lista_vazia_mas_com_motivos()
    {
        var resposta = await BuscarOk(Pedido("Francês", new[] { "segunda", "quarta" }, "08:00", "22:00", 60));

        Assert.Empty(resposta.Professores);
        Assert.Equal(8, resposta.NaoElegiveis.Count);
        Assert.All(resposta.NaoElegiveis, ineligivel => Assert.Equal("Não ensina o idioma", ineligivel.Motivo));
    }

    [Fact]
    public async Task Exige_o_mesmo_horario_em_todos_os_dias_pedidos()
    {
        var resposta = await BuscarOk(Pedido("Inglês", new[] { "segunda", "quarta" }, "18:00", "20:00", 60));

        Assert.Equal(
            new[] { "Ana", "Fábio", "Bruno", "Gabriela" },
            resposta.Professores.Select(professor => professor.Nome));

        // Gabriela tem aula na segunda das 18:00 às 18:30, então 18:00 não serve.
        var gabriela = resposta.Professores.Single(professor => professor.Nome == "Gabriela");
        Assert.Equal(new[] { "18:30", "19:00" }, gabriela.Horarios);

        // Heitor tem a quarta inteira ocupada entre 18:00 e 20:00.
        Assert.Equal("Sem horário em comum nos dias pedidos", resposta.NaoElegiveis.Single(i => i.Nome == "Heitor").Motivo);
    }

    [Fact]
    public async Task Pedido_invalido_devolve_400_com_as_mensagens()
    {
        var pedido = Pedido("  ", new[] { "feriado" }, "25:00", "20h", 45);

        var response = await _client.PostAsJsonAsync("/api/professores/busca", pedido);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var erro = await response.Content.ReadFromJsonAsync<ErroResposta>(Json.Options);

        Assert.NotNull(erro);
        Assert.Equal("Dados inválidos.", erro!.Mensagem);
        Assert.Contains("O idioma é obrigatório.", erro.Erros);
        Assert.Contains("Dia inválido: 'feriado'.", erro.Erros);
        Assert.Contains("Horário inválido: '25:00'. Use o formato HH:mm.", erro.Erros);
        Assert.Contains("A duração da aula deve ser 30, 60 ou 90 minutos.", erro.Erros);
    }

    [Fact]
    public async Task Fim_da_faixa_antes_do_inicio_devolve_400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/professores/busca",
            Pedido("Inglês", new[] { "segunda" }, "20:00", "18:00", 60));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var erro = await response.Content.ReadFromJsonAsync<ErroResposta>(Json.Options);
        Assert.Contains("O fim da faixa de horário deve ser depois do início.", erro!.Erros);
    }

    [Fact]
    public async Task Json_malformado_devolve_400_no_mesmo_formato()
    {
        using var content = new StringContent("{ \"idioma\": ", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/professores/busca", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var erro = await response.Content.ReadFromJsonAsync<ErroResposta>(Json.Options);
        Assert.Equal("Dados inválidos.", erro!.Mensagem);
        Assert.NotEmpty(erro.Erros);
    }

    [Fact]
    public async Task Health_responde_ok()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
