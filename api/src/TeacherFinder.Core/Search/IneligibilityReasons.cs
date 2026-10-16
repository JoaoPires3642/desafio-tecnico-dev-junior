namespace TeacherFinder.Core.Search;

/// <summary>
/// Motivos devolvidos na resposta para quem ficou de fora. São texto exibido ao
/// usuário, por isso ficam em português (fazem parte do contrato da API).
/// </summary>
public static class IneligibilityReasons
{
    public const string DoesNotTeachLanguage = "Não ensina o idioma";
    public const string ScheduleBlocked = "Agenda bloqueada";
    public const string NoCommonSlot = "Sem horário em comum nos dias pedidos";
}
