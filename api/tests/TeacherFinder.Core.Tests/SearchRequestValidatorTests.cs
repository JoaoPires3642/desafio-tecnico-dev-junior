using TeacherFinder.Core.Model;
using TeacherFinder.Core.Search;

namespace TeacherFinder.Core.Tests;

public sealed class SearchRequestValidatorTests
{
    private readonly SearchRequestValidator _validator = new();

    private static SearchRequestInput Valid() =>
        new("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60);

    [Fact]
    public void Aceita_um_pedido_completo()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);

        var request = Assert.IsType<SearchRequest>(result.Request);
        Assert.Equal("Espanhol", request.Language);
        Assert.Equal(new[] { Weekday.Monday }, request.Days);
        Assert.Equal(Sample.At("18:00"), request.StartWindow);
        Assert.Equal(Sample.At("20:00"), request.EndWindow);
        Assert.Equal(60, request.DurationMinutes);
    }

    [Fact]
    public void Rejeita_idioma_em_branco()
    {
        var result = _validator.Validate(Valid() with { Language = "   " });

        Assert.False(result.IsValid);
        Assert.Contains("O idioma é obrigatório.", result.Errors);
    }

    [Fact]
    public void Rejeita_pedido_sem_dias()
    {
        var result = _validator.Validate(Valid() with { Days = Array.Empty<string>() });

        Assert.False(result.IsValid);
        Assert.Contains("Informe pelo menos um dia da semana.", result.Errors);
    }

    [Fact]
    public void Rejeita_dia_invalido_mostrando_o_valor()
    {
        var result = _validator.Validate(Valid() with { Days = new[] { "feriado" } });

        Assert.False(result.IsValid);
        Assert.Contains("Dia inválido: 'feriado'.", result.Errors);
    }

    [Fact]
    public void Rejeita_horario_fora_do_formato()
    {
        var result = _validator.Validate(Valid() with { StartWindow = "25:00", EndWindow = "20h" });

        Assert.False(result.IsValid);
        Assert.Contains("Horário inválido: '25:00'. Use o formato HH:mm.", result.Errors);
        Assert.Contains("Horário inválido: '20h'. Use o formato HH:mm.", result.Errors);
    }

    [Fact]
    public void Rejeita_fim_antes_ou_igual_ao_inicio()
    {
        var result = _validator.Validate(Valid() with { StartWindow = "20:00", EndWindow = "18:00" });

        Assert.False(result.IsValid);
        Assert.Contains("O fim da faixa de horário deve ser depois do início.", result.Errors);
    }

    [Theory]
    [InlineData(45)]
    [InlineData(15)]
    [InlineData(0)]
    public void Rejeita_duracao_nao_suportada(int duration)
    {
        var result = _validator.Validate(Valid() with { DurationMinutes = duration });

        Assert.False(result.IsValid);
        Assert.Contains("A duração da aula deve ser 30, 60 ou 90 minutos.", result.Errors);
    }

    [Fact]
    public void Rejeita_duracao_ausente()
    {
        var result = _validator.Validate(Valid() with { DurationMinutes = null });

        Assert.False(result.IsValid);
        Assert.Contains("A duração da aula deve ser 30, 60 ou 90 minutos.", result.Errors);
    }

    [Fact]
    public void Devolve_todos_os_erros_de_uma_vez()
    {
        var result = _validator.Validate(new SearchRequestInput(null, null, "abc", "20:00", 45));

        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
    }

    [Fact]
    public void Deduplica_dias_repetidos()
    {
        var result = _validator.Validate(Valid() with { Days = new[] { "segunda", "segunda", "terca" } });

        Assert.True(result.IsValid);
        Assert.Equal(new[] { Weekday.Monday, Weekday.Tuesday }, result.Request!.Days);
    }

    [Fact]
    public void Aceita_dia_com_acento_e_caixa_diferente()
    {
        var result = _validator.Validate(Valid() with { Days = new[] { "Terça", "SÁBADO" } });

        Assert.True(result.IsValid);
        Assert.Equal(new[] { Weekday.Tuesday, Weekday.Saturday }, result.Request!.Days);
    }
}
