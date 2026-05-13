namespace PomodoroTimer.Models;

public class PomodoroSettings
{
    public int WorkMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int PomodorosBeforeLongBreak { get; set; } = 4;
    public bool SoundEnabled { get; set; } = true;

    public void Clamp()
    {
        WorkMinutes = Math.Clamp(WorkMinutes, 1, 120);
        ShortBreakMinutes = Math.Clamp(ShortBreakMinutes, 1, 60);
        LongBreakMinutes = Math.Clamp(LongBreakMinutes, 1, 120);
        PomodorosBeforeLongBreak = Math.Clamp(PomodorosBeforeLongBreak, 1, 10);
    }
}
