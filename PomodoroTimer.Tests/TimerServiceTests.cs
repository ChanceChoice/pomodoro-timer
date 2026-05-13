using PomodoroTimer.Models;
using PomodoroTimer.Services;

namespace PomodoroTimer.Tests;

public class TimerServiceTests
{
    [Fact]
    public void InitialState_IsIdle()
    {
        var settings = new PomodoroSettings();
        var service = new TimerService(settings);
        Assert.Equal(TimerState.Idle, service.State);
        Assert.Equal(25 * 60, service.RemainingSeconds);
    }

    [Fact]
    public void Start_SetsStateToWorking()
    {
        var settings = new PomodoroSettings();
        var service = new TimerService(settings);
        service.Start();
        Assert.Equal(TimerState.Working, service.State);
    }

    [Fact]
    public void Pause_SetsStateToIdle()
    {
        var settings = new PomodoroSettings();
        var service = new TimerService(settings);
        service.Start();
        service.Pause();
        Assert.Equal(TimerState.Idle, service.State);
    }

    [Fact]
    public void Reset_ResetsToWorkDuration()
    {
        var settings = new PomodoroSettings { WorkMinutes = 30 };
        var service = new TimerService(settings);
        service.Start();
        service.Reset();
        Assert.Equal(TimerState.Idle, service.State);
        Assert.Equal(30 * 60, service.RemainingSeconds);
    }

    [Fact]
    public void TickOnce_WhenWorking_DecrementsRemaining()
    {
        var settings = new PomodoroSettings();
        var service = new TimerService(settings);
        service.Start();
        service.TickOnce();
        Assert.Equal(25 * 60 - 1, service.RemainingSeconds);
    }

    [Fact]
    public void SessionPomodoroCount_IncrementsOnWorkComplete()
    {
        var settings = new PomodoroSettings { WorkMinutes = 0 };
        var service = new TimerService(settings);
        var completedCount = 0;
        service.WorkCompleted += () => completedCount++;
        service.Start();
        service.TickOnce();
        Assert.Equal(1, completedCount);
    }
}
