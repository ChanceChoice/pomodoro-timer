using PomodoroTimer.Models;

namespace PomodoroTimer.Services;

public enum TimerState
{
    Idle,
    Working,
    ShortBreak,
    LongBreak
}

public class TimerService
{
    private readonly PomodoroSettings _settings;
    private int _sessionPomodoroCount;

    public TimerState State { get; private set; } = TimerState.Idle;
    public int RemainingSeconds { get; private set; }
    public int SessionPomodoroCount => _sessionPomodoroCount;

    public event Action? Tick;
    public event Action? WorkCompleted;
    public event Action? BreakCompleted;
    public event Action? StateChanged;

    public TimerService(PomodoroSettings settings)
    {
        _settings = settings;
        RemainingSeconds = settings.WorkMinutes * 60;
    }

    public void Start()
    {
        if (State == TimerState.Idle)
        {
            State = TimerState.Working;
            StateChanged?.Invoke();
        }
    }

    public void Pause()
    {
        State = TimerState.Idle;
        StateChanged?.Invoke();
    }

    public void Reset()
    {
        State = TimerState.Idle;
        RemainingSeconds = _settings.WorkMinutes * 60;
        StateChanged?.Invoke();
    }

    public void Skip()
    {
        if (State == TimerState.Working)
            CompleteWork();
        else if (State is TimerState.ShortBreak or TimerState.LongBreak)
            CompleteBreak();
    }

    public void TickOnce()
    {
        if (State == TimerState.Idle) return;

        RemainingSeconds--;
        Tick?.Invoke();

        if (RemainingSeconds <= 0)
        {
            if (State == TimerState.Working)
                CompleteWork();
            else
                CompleteBreak();
        }
    }

    private void CompleteWork()
    {
        _sessionPomodoroCount++;
        WorkCompleted?.Invoke();

        bool isLongBreak = _sessionPomodoroCount % _settings.PomodorosBeforeLongBreak == 0;
        State = isLongBreak ? TimerState.LongBreak : TimerState.ShortBreak;
        RemainingSeconds = (isLongBreak ? _settings.LongBreakMinutes : _settings.ShortBreakMinutes) * 60;
        StateChanged?.Invoke();
    }

    private void CompleteBreak()
    {
        BreakCompleted?.Invoke();
        State = TimerState.Working;
        RemainingSeconds = _settings.WorkMinutes * 60;
        StateChanged?.Invoke();
    }
}
