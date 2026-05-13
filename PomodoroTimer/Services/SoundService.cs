namespace PomodoroTimer.Services;

public class SoundService
{
    private readonly bool _enabled;

    public SoundService(bool enabled)
    {
        _enabled = enabled;
    }

    public void PlayWorkComplete()
    {
        if (!_enabled) return;
        PlayBeep(800, 300);
    }

    public void PlayBreakComplete()
    {
        if (!_enabled) return;
        PlayBeep(600, 200);
    }

    private static void PlayBeep(int frequency, int durationMs)
    {
        try
        {
            Console.Beep(frequency, durationMs);
        }
        catch
        {
        }
    }
}
