using TeacherFinder.Api.Infrastructure;
using TeacherFinder.Core.Model;

namespace TeacherFinder.Api.Tests;

/// <summary>Testes de carga usando o professores.json real do repositório.</summary>
public sealed class TeachersJsonReaderTests
{
    private static IReadOnlyList<Teacher> LerArquivoReal() =>
        TeachersJsonReader.Read(File.ReadAllText(ApiFactory.ProfessoresPath));

    [Fact]
    public void Le_o_arquivo_real_com_os_oito_professores()
    {
        var professores = LerArquivoReal();

        Assert.Equal(8, professores.Count);
        Assert.Equal(Enumerable.Range(1, 8), professores.Select(professor => professor.Id));
    }

    [Fact]
    public void Mapeia_disponibilidade_e_aulas_do_arquivo_real()
    {
        var fabio = LerArquivoReal().Single(professor => professor.Id == 6);

        Assert.Equal("Fábio", fabio.Name);
        Assert.Equal(new[] { "Inglês" }, fabio.Languages);
        Assert.False(fabio.ScheduleBlocked);

        Assert.Equal(3, fabio.Availability.Count);
        var segunda = fabio.Availability.Single(bloco => bloco.Day == Weekday.Monday);
        Assert.Equal("18:15", segunda.Start.ToString());
        Assert.Equal("20:00", segunda.End.ToString());

        var aula = Assert.Single(fabio.ScheduledClasses);
        Assert.Equal(Weekday.Friday, aula.Day);
        Assert.Equal("10:00", aula.Start.ToString());
        Assert.Equal("11:00", aula.End.ToString());
        Assert.Equal(1, fabio.ClassesPerWeek);
    }

    [Fact]
    public void Le_a_agenda_bloqueada_da_carla()
    {
        var carla = LerArquivoReal().Single(professor => professor.Id == 3);

        Assert.True(carla.ScheduleBlocked);
        Assert.Empty(carla.ScheduledClasses);
    }

    [Fact]
    public void Falha_com_mensagem_clara_quando_o_dia_e_invalido()
    {
        const string json = """
            {"professores":[{"id":99,"nome":"X","idiomas":["Inglês"],"agendaBloqueada":false,
            "disponibilidade":[{"dia":"feriado","inicio":"18:00","fim":"19:00"}],"aulasMarcadas":[]}]}
            """;

        var erro = Assert.Throws<InvalidDataException>(() => TeachersJsonReader.Read(json));

        Assert.Contains("Professor 99", erro.Message);
        Assert.Contains("feriado", erro.Message);
    }

    [Fact]
    public void Falha_com_mensagem_clara_quando_o_horario_e_invalido()
    {
        const string json = """
            {"professores":[{"id":7,"nome":"Y","idiomas":["Inglês"],"agendaBloqueada":false,
            "disponibilidade":[],"aulasMarcadas":[{"dia":"segunda","inicio":"25:00","duracaoMinutos":60}]}]}
            """;

        var erro = Assert.Throws<InvalidDataException>(() => TeachersJsonReader.Read(json));

        Assert.Contains("Professor 7", erro.Message);
        Assert.Contains("25:00", erro.Message);
    }

    [Fact]
    public void Falha_quando_o_arquivo_nao_tem_a_lista_de_professores()
    {
        Assert.Throws<InvalidDataException>(() => TeachersJsonReader.Read("""{"outraCoisa":[]}"""));
    }
}
