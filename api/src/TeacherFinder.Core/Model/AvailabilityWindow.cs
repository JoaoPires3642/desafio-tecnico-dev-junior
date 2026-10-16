namespace TeacherFinder.Core.Model;

/// <summary>
/// Bloco semanal em que o professor pode dar aula. Os dados garantem que blocos
/// do mesmo dia não se sobrepõem nem se encostam.
/// </summary>
public sealed record AvailabilityWindow(Weekday Day, TimeOfDay Start, TimeOfDay End)
{
    /// <summary>Diz se uma aula de <paramref name="durationMinutes"/> começando em <paramref name="start"/> cabe inteira no bloco.</summary>
    public bool FitsWithin(TimeOfDay candidateStart, int durationMinutes) =>
        Start.Minutes <= candidateStart.Minutes &&
        candidateStart.Minutes + durationMinutes <= End.Minutes;
}
