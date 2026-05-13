using System.IO;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Tests;

public class MainViewModelTests : IDisposable
{
    private readonly string _testDir;

    public MainViewModelTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"pomodoro_vm_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        Directory.Delete(_testDir, true);
    }

    [Fact]
    public void InitialState_ShowsWorkTime()
    {
        var settings = new PomodoroSettings { WorkMinutes = 25 };
        var timer = new TimerService(settings);
        var history = new HistoryService(Path.Combine(_testDir, "history.json"));
        var vm = new MainViewModel(timer, history, settings);

        Assert.Equal("25:00", vm.TimeDisplay);
        Assert.Equal("就绪", vm.StateDisplay);
    }

    [Fact]
    public void StartCommand_ChangesStateDisplay()
    {
        var settings = new PomodoroSettings();
        var timer = new TimerService(settings);
        var history = new HistoryService(Path.Combine(_testDir, "history.json"));
        var vm = new MainViewModel(timer, history, settings);

        vm.StartCommand.Execute(null);

        Assert.True(vm.IsRunning);
    }

    [Fact]
    public void ResetCommand_ResetsTimeDisplay()
    {
        var settings = new PomodoroSettings { WorkMinutes = 30 };
        var timer = new TimerService(settings);
        var history = new HistoryService(Path.Combine(_testDir, "history.json"));
        var vm = new MainViewModel(timer, history, settings);

        vm.StartCommand.Execute(null);
        vm.ResetCommand.Execute(null);

        Assert.Equal("30:00", vm.TimeDisplay);
        Assert.False(vm.IsRunning);
    }

    [Fact]
    public void TimeDisplay_UpdatesOnTick()
    {
        var settings = new PomodoroSettings { WorkMinutes = 25 };
        var timer = new TimerService(settings);
        var history = new HistoryService(Path.Combine(_testDir, "history.json"));
        var vm = new MainViewModel(timer, history, settings);

        vm.StartCommand.Execute(null);
        timer.TickOnce();

        Assert.Equal("24:59", vm.TimeDisplay);
    }
}
