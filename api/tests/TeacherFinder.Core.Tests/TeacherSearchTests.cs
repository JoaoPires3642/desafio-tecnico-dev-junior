using TeacherFinder.Core.Model;
using TeacherFinder.Core.Search;

namespace TeacherFinder.Core.Tests;

public sealed class TeacherSearchTests
{
    private readonly TeacherSearch _search = new();

    private static string[] Slots(TeacherMatch match) => match.AvailableSlots.Select(slot => slot.ToString()).ToArray();

    [Fact]
    public void Permite_aula_que_encosta_no_fim_de_uma_aula_marcada()
    {
        // Aula marcada das 19:00 às 20:00. Começar às 18:00 (60 min) encosta, então vale.
        var teacher = Sample.Teacher(1, "Ana", new[] { "Espanhol" },
            availability: new[] { Sample.Window("segunda", "18:00", "22:00") },
            scheduledClasses: new[] { Sample.Class("segunda", "19:00", 60) });

        var result = _search.Search(new[] { teacher }, Sample.Request("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        var match = Assert.Single(result.Teachers);
        Assert.Equal(new[] { "18:00" }, Slots(match));
        Assert.Empty(result.Ineligible);
    }

    [Fact]
    public void Nao_oferece_horario_que_bate_em_aula_marcada()
    {
        var teacher = Sample.Teacher(1, "Ana", new[] { "Espanhol" },
            availability: new[] { Sample.Window("segunda", "18:00", "22:00") },
            scheduledClasses: new[] { Sample.Class("segunda", "19:00", 60) });

        var result = _search.Search(new[] { teacher }, Sample.Request("Espanhol", new[] { "segunda" }, "18:00", "22:00", 60));

        // 18:30 (18:30-19:30), 19:00 e 19:30 colidem com a aula marcada.
        var match = Assert.Single(result.Teachers);
        Assert.DoesNotContain("18:30", Slots(match));
        Assert.DoesNotContain("19:00", Slots(match));
        Assert.DoesNotContain("19:30", Slots(match));
        Assert.Equal(new[] { "18:00", "20:00", "20:30", "21:00" }, Slots(match));
    }

    [Fact]
    public void Lista_todos_os_horarios_livres_em_ordem_crescente()
    {
        var teacher = Sample.Teacher(4, "Diego", new[] { "Espanhol" },
            availability: new[] { Sample.Window("segunda", "18:00", "22:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Professor_que_nao_ensina_o_idioma_fica_de_fora()
    {
        var teacher = Sample.Teacher(2, "Bruno", new[] { "Inglês" },
            availability: new[] { Sample.Window("segunda", "18:00", "21:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Empty(result.Teachers);
        var ineligible = Assert.Single(result.Ineligible);
        Assert.Equal(IneligibilityReasons.DoesNotTeachLanguage, ineligible.Reason);
    }

    [Fact]
    public void Professor_com_agenda_bloqueada_fica_de_fora()
    {
        var teacher = Sample.Teacher(3, "Carla", new[] { "Inglês" }, scheduleBlocked: true,
            availability: new[] { Sample.Window("segunda", "08:00", "22:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Empty(result.Teachers);
        var ineligible = Assert.Single(result.Ineligible);
        Assert.Equal(IneligibilityReasons.ScheduleBlocked, ineligible.Reason);
    }

    [Fact]
    public void Motivo_do_idioma_tem_prioridade_sobre_agenda_bloqueada()
    {
        var teacher = Sample.Teacher(3, "Carla", new[] { "Inglês" }, scheduleBlocked: true);

        var result = _search.Search(new[] { teacher }, Sample.Request("Espanhol", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(IneligibilityReasons.DoesNotTeachLanguage, Assert.Single(result.Ineligible).Reason);
    }

    [Fact]
    public void Sem_disponibilidade_no_dia_pedido_fica_sem_horario_em_comum()
    {
        var teacher = Sample.Teacher(9, "Helena", new[] { "Inglês" },
            availability: new[] { Sample.Window("terca", "18:00", "20:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Empty(result.Teachers);
        Assert.Equal(IneligibilityReasons.NoCommonSlot, Assert.Single(result.Ineligible).Reason);
    }

    [Fact]
    public void Exige_o_mesmo_horario_de_inicio_em_todos_os_dias_pedidos()
    {
        // Segunda 18-22 e terça 19-21. Só 19:00, 19:30 e 20:00 servem para os dois dias.
        var teacher = Sample.Teacher(1, "Ana", new[] { "Inglês" },
            availability: new[]
            {
                Sample.Window("segunda", "18:00", "22:00"),
                Sample.Window("terca", "19:00", "21:00"),
            });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda", "terca" }, "18:00", "22:00", 60));

        Assert.Equal(new[] { "19:00", "19:30", "20:00" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Aula_precisa_caber_inteira_no_bloco_de_disponibilidade()
    {
        // Bloco começa 18:15: 18:00 não cabe inteiro; 18:30 e 19:00 sim.
        var teacher = Sample.Teacher(6, "Fábio", new[] { "Inglês" },
            availability: new[] { Sample.Window("segunda", "18:15", "20:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(new[] { "18:30", "19:00" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Aula_precisa_terminar_dentro_da_faixa_pedida()
    {
        var teacher = Sample.Teacher(1, "Ana", new[] { "Inglês" },
            availability: new[] { Sample.Window("segunda", "08:00", "22:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "19:00", 60));

        // 18:30 terminaria 19:30, fora da faixa.
        Assert.Equal(new[] { "18:00" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Suporta_duracao_de_90_minutos()
    {
        var teacher = Sample.Teacher(1, "Ana", new[] { "Inglês" },
            availability: new[] { Sample.Window("segunda", "18:00", "22:00") });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "22:00", 90));

        Assert.Equal(new[] { "18:00", "18:30", "19:00", "19:30", "20:00", "20:30" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Aulas_marcadas_em_outro_dia_nao_afetam_o_dia_pedido()
    {
        var teacher = Sample.Teacher(1, "Ana", new[] { "Inglês" },
            availability: new[]
            {
                Sample.Window("segunda", "18:00", "22:00"),
                Sample.Window("quarta", "18:00", "22:00"),
            },
            scheduledClasses: new[] { Sample.Class("quarta", "18:00", 60) });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Equal(new[] { "18:00", "18:30", "19:00" }, Slots(Assert.Single(result.Teachers)));
    }

    [Fact]
    public void Ordena_elegiveis_por_menos_aulas_na_semana_e_depois_por_nome()
    {
        var availability = new[] { Sample.Window("segunda", "08:00", "22:00") };
        var teachers = new[]
        {
            Sample.Teacher(30, "Bruno", new[] { "Inglês" }, availability: availability,
                scheduledClasses: new[] { Sample.Class("segunda", "08:00", 60), Sample.Class("terca", "08:00", 60) }),
            Sample.Teacher(31, "Fábio", new[] { "Inglês" }, availability: availability,
                scheduledClasses: new[] { Sample.Class("segunda", "08:00", 60) }),
            Sample.Teacher(32, "Ana", new[] { "Inglês" }, availability: availability,
                scheduledClasses: new[] { Sample.Class("segunda", "08:00", 60) }),
            Sample.Teacher(33, "Felipe", new[] { "Inglês" }, availability: availability,
                scheduledClasses: new[] { Sample.Class("segunda", "08:00", 60) }),
        };

        var result = _search.Search(teachers, Sample.Request("Inglês", new[] { "segunda" }, "18:00", "19:00", 60));

        // Fábio(1) antes de Felipe(1): "á" ordena como "a" em pt-BR.
        Assert.Equal(new[] { "Ana", "Fábio", "Felipe", "Bruno" }, result.Teachers.Select(t => t.Name));
    }

    [Fact]
    public void Aulas_na_semana_conta_todas_as_aulas_do_professor()
    {
        var teacher = Sample.Teacher(2, "Bruno", new[] { "Inglês" },
            availability: new[] { Sample.Window("quinta", "18:00", "20:00") },
            scheduledClasses: new[]
            {
                Sample.Class("terca", "18:00", 60),
                Sample.Class("quinta", "18:00", 60),
            });

        var result = _search.Search(new[] { teacher }, Sample.Request("Inglês", new[] { "quinta" }, "18:00", "20:00", 60));

        Assert.Equal(2, Assert.Single(result.Teachers).ClassesPerWeek);
    }

    [Fact]
    public void Resposta_sem_professor_elegivel_vem_vazia_sem_quebrar()
    {
        var result = _search.Search(Array.Empty<Teacher>(), Sample.Request("Francês", new[] { "segunda" }, "18:00", "20:00", 60));

        Assert.Empty(result.Teachers);
        Assert.Empty(result.Ineligible);
    }
}
