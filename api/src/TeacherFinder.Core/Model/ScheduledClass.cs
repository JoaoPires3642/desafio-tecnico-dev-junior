namespace TeacherFinder.Core.Model;

/// <summary>Aula já marcada na semana do professor.</summary>
public sealed record ScheduledClass(Weekday Day, TimeOfDay Start, int DurationMinutes)
{
    /// <summary>Horário em que a aula termina.</summary>
    public TimeOfDay End => new(Start.Minutes + DurationMinutes);

    /// <summary>
    /// Diz se esta aula conflita com uma aula que começa em <paramref name="candidateStart"/>.
    /// Encostar não conflita: se a aula marcada termina às 19:00, pode começar outra às 19:00.
    /// </summary>
    public bool OverlapsWith(TimeOfDay candidateStart, int durationMinutes)
    {
        var candidateEnd = candidateStart.Minutes + durationMinutes;
        return Start.Minutes < candidateEnd && candidateStart.Minutes < End.Minutes;
    }
}
