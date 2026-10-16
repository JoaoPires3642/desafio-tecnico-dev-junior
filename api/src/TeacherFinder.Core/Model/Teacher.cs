namespace TeacherFinder.Core.Model;

/// <summary>Um professor e a sua agenda semanal.</summary>
public sealed record Teacher(
    int Id,
    string Name,
    IReadOnlyList<string> Languages,
    bool ScheduleBlocked,
    IReadOnlyList<AvailabilityWindow> Availability,
    IReadOnlyList<ScheduledClass> ScheduledClasses)
{
    /// <summary>Quantidade de aulas já marcadas na semana (todas, não só nos dias buscados).</summary>
    public int ClassesPerWeek => ScheduledClasses.Count;
}
