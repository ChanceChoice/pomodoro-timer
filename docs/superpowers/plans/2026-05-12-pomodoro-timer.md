# Pomodoro Timer Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a minimal Pomodoro timer Windows desktop app with WPF, featuring customizable durations, sound/visual notifications, system tray, and daily stats.

**Architecture:** Single-window WPF app using MVVM pattern. Services (Timer, History, Sound, Notification) are injected into MainViewModel. Settings and history persisted as local JSON files.

**Tech Stack:** .NET 8, WPF, Hardcodet.NotifyIcon.Wpf, System.Text.Json, xUnit (tests)

---

## File Structure

```
PomodoroTimer/
├── PomodoroTimer.sln
├── PomodoroTimer/
│   ├── PomodoroTimer.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   └── RelayCommand.cs
│   ├── Models/
│   │   ├── PomodoroSettings.cs
│   │   └── PomodoroRecord.cs
│   ├── Services/
│   │   ├── TimerService.cs
│   │   ├── HistoryService.cs
│   │   ├── SoundService.cs
│   │   └── NotificationService.cs
│   └── Converters/
│       └── StateToColorConverter.cs
├── PomodoroTimer.Tests/
│   ├── PomodoroTimer.Tests.csproj
│   ├── TimerServiceTests.cs
│   ├── HistoryServiceTests.cs
│   └── MainViewModelTests.cs
```

---

### Task 1: Project Scaffolding

**Files:**
- Create: `PomodoroTimer.sln`
- Create: `PomodoroTimer/PomodoroTimer.csproj`
- Create: `PomodoroTimer/App.xaml`
- Create: `PomodoroTimer/App.xaml.cs`
- Create: `PomodoroTimer/MainWindow.xaml`
- Create: `PomodoroTimer/MainWindow.xaml.cs`
- Create: `PomodoroTimer.Tests/PomodoroTimer.Tests.csproj`

- [ ] **Step 1: Create the solution and projects**

```bash
cd D:/Chance/coding/01
dotnet new sln -n PomodoroTimer
dotnet new wpf -n PomodoroTimer -o PomodoroTimer
dotnet new xunit -n PomodoroTimer.Tests -o PomodoroTimer.Tests
dotnet sln add PomodoroTimer/PomodoroTimer.csproj
dotnet sln add PomodoroTimer.Tests/PomodoroTimer.Tests.csproj
dotnet add PomodoroTimer.Tests/PomodoroTimer.Tests.csproj reference PomodoroTimer/PomodoroTimer.csproj
```

- [ ] **Step 2: Add NuGet packages**

```bash
cd D:/Chance/coding/01
dotnet add PomodoroTimer/PomodoroTimer.csproj package Hardcodet.NotifyIcon.Wpf
dotnet add PomodoroTimer.Tests/PomodoroTimer.Tests.csproj package Moq
```

- [ ] **Step 3: Verify build**

```bash
cd D:/Chance/coding/01
dotnet build
```

Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Commit**

```bash
cd D:/Chance/coding/01
git init
git add -A
git commit -m "chore: scaffold WPF project with test project"
```

---

### Task 2: Models

**Files:**
- Create: `PomodoroTimer/Models/PomodoroSettings.cs`
- Create: `PomodoroTimer/Models/PomodoroRecord.cs`

- [ ] **Step 1: Create PomodoroSettings model**

```csharp
// PomodoroTimer/Models/PomodoroSettings.cs
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
```

- [ ] **Step 2: Create PomodoroRecord model**

```csharp
// PomodoroTimer/Models/PomodoroRecord.cs
namespace PomodoroTimer.Models;

public class PomodoroRecord
{
    public DateOnly Date { get; set; }
    public int CompletedCount { get; set; }
}
```

- [ ] **Step 3: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/Models/
git commit -m "feat: add PomodoroSettings and PomodoroRecord models"
```

---

### Task 3: TimerService + Tests

**Files:**
- Create: `PomodoroTimer/Services/TimerService.cs`
- Create: `PomodoroTimer.Tests/TimerServiceTests.cs`

- [ ] **Step 1: Write failing tests for TimerService**

```csharp
// PomodoroTimer.Tests/TimerServiceTests.cs
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
    public void Tick_WhenWorking_DecrementsRemaining()
    {
        var settings = new PomodoroSettings();
        var service = new TimerService(settings);
        service.Start();
        service.Tick(); // simulate 1 second
        Assert.Equal(25 * 60 - 1, service.RemainingSeconds);
    }

    [Fact]
    public void SessionPomodoroCount_IncrementsOnWorkComplete()
    {
        var settings = new PomodoroSettings { WorkMinutes = 0 }; // instant complete
        var service = new TimerService(settings);
        var completedCount = 0;
        service.WorkCompleted += () => completedCount++;
        service.Start();
        service.Tick(); // triggers completion
        Assert.Equal(1, completedCount);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "TimerServiceTests" -v
```

Expected: FAIL — `TimerService` and `TimerState` do not exist.

- [ ] **Step 3: Implement TimerService**

```csharp
// PomodoroTimer/Services/TimerService.cs
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

    /// <summary>
    /// Call this every second to advance the timer.
    /// In production, wired to DispatcherTimer.Tick.
    /// In tests, call manually.
    /// </summary>
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
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "TimerServiceTests" -v
```

Expected: All 6 tests PASS.

- [ ] **Step 5: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/Services/TimerService.cs PomodoroTimer.Tests/TimerServiceTests.cs
git commit -m "feat: add TimerService with state machine and tests"
```

---

### Task 4: HistoryService + Tests

**Files:**
- Create: `PomodoroTimer/Services/HistoryService.cs`
- Create: `PomodoroTimer.Tests/HistoryServiceTests.cs`

- [ ] **Step 1: Write failing tests for HistoryService**

```csharp
// PomodoroTimer.Tests/HistoryServiceTests.cs
using PomodoroTimer.Services;

namespace PomodoroTimer.Tests;

public class HistoryServiceTests : IDisposable
{
    private readonly string _testDir;
    private readonly string _testFile;

    public HistoryServiceTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"pomodoro_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
        _testFile = Path.Combine(_testDir, "history.json");
    }

    public void Dispose()
    {
        Directory.Delete(_testDir, true);
    }

    [Fact]
    public void NewService_ReturnsZeroForToday()
    {
        var service = new HistoryService(_testFile);
        Assert.Equal(0, service.GetTodayCount());
    }

    [Fact]
    public void AddCompletedPomodoro_IncrementsTodayCount()
    {
        var service = new HistoryService(_testFile);
        service.AddCompletedPomodoro();
        service.AddCompletedPomodoro();
        Assert.Equal(2, service.GetTodayCount());
    }

    [Fact]
    public void Data_PersistsAcrossInstances()
    {
        var service1 = new HistoryService(_testFile);
        service1.AddCompletedPomodoro();

        var service2 = new HistoryService(_testFile);
        Assert.Equal(1, service2.GetTodayCount());
    }

    [Fact]
    public void GetStreak_ReturnsConsecutiveDays()
    {
        // Write test data with consecutive days
        var today = DateOnly.FromDateTime(DateTime.Today);
        var records = new List<object>
        {
            new { Date = today.ToString("yyyy-MM-dd"), CompletedCount = 3 },
            new { Date = today.AddDays(-1).ToString("yyyy-MM-dd"), CompletedCount = 2 },
            new { Date = today.AddDays(-2).ToString("yyyy-MM-dd"), CompletedCount = 1 },
        };
        File.WriteAllText(_testFile, System.Text.Json.JsonSerializer.Serialize(records));

        var service = new HistoryService(_testFile);
        Assert.Equal(3, service.GetStreak());
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "HistoryServiceTests" -v
```

Expected: FAIL — `HistoryService` does not exist.

- [ ] **Step 3: Implement HistoryService**

```csharp
// PomodoroTimer/Services/HistoryService.cs
using System.Text.Json;
using PomodoroTimer.Models;

namespace PomodoroTimer.Services;

public class HistoryService
{
    private readonly string _filePath;
    private List<PomodoroRecord> _records;

    public HistoryService(string filePath)
    {
        _filePath = filePath;
        _records = Load();
    }

    public void AddCompletedPomodoro()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var record = _records.FirstOrDefault(r => r.Date == today);
        if (record == null)
        {
            record = new PomodoroRecord { Date = today, CompletedCount = 0 };
            _records.Add(record);
        }
        record.CompletedCount++;
        Save();
    }

    public int GetTodayCount()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _records.FirstOrDefault(r => r.Date == today)?.CompletedCount ?? 0;
    }

    public int GetStreak()
    {
        var sorted = _records
            .OrderByDescending(r => r.Date)
            .Where(r => r.CompletedCount > 0)
            .ToList();

        if (sorted.Count == 0) return 0;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var streak = 0;
        var expected = today;

        foreach (var record in sorted)
        {
            if (record.Date == expected)
            {
                streak++;
                expected = expected.AddDays(-1);
            }
            else if (record.Date < expected)
            {
                break;
            }
        }

        return streak;
    }

    private List<PomodoroRecord> Load()
    {
        if (!File.Exists(_filePath)) return new List<PomodoroRecord>();
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<PomodoroRecord>>(json) ?? new List<PomodoroRecord>();
        }
        catch
        {
            return new List<PomodoroRecord>();
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_records, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "HistoryServiceTests" -v
```

Expected: All 4 tests PASS.

- [ ] **Step 5: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/Services/HistoryService.cs PomodoroTimer.Tests/HistoryServiceTests.cs
git commit -m "feat: add HistoryService with persistence and streak calculation"
```

---

### Task 5: MainViewModel

**Files:**
- Create: `PomodoroTimer/ViewModels/RelayCommand.cs`
- Create: `PomodoroTimer/ViewModels/MainViewModel.cs`
- Create: `PomodoroTimer.Tests/MainViewModelTests.cs`

- [ ] **Step 1: Create RelayCommand (MVVM infrastructure)**

```csharp
// PomodoroTimer/ViewModels/RelayCommand.cs
using System.Windows.Input;

namespace PomodoroTimer.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
```

- [ ] **Step 2: Write failing tests for MainViewModel**

```csharp
// PomodoroTimer.Tests/MainViewModelTests.cs
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
        Assert.Equal("工作中", vm.StateDisplay);
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
        timer.TickOnce(); // simulate 1 second

        Assert.Equal("24:59", vm.TimeDisplay);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "MainViewModelTests" -v
```

Expected: FAIL — `MainViewModel` does not exist.

- [ ] **Step 4: Implement MainViewModel**

```csharp
// PomodoroTimer/ViewModels/MainViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using PomodoroTimer.Models;
using PomodoroTimer.Services;

namespace PomodoroTimer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly TimerService _timer;
    private readonly HistoryService _history;
    private readonly PomodoroSettings _settings;
    private readonly DispatcherTimer _dispatcherTimer;

    private string _timeDisplay = "";
    private string _stateDisplay = "";
    private bool _isRunning;
    private string _todayCount = "";
    private string _streak = "";

    public string TimeDisplay
    {
        get => _timeDisplay;
        set { _timeDisplay = value; OnPropertyChanged(); }
    }

    public string StateDisplay
    {
        get => _stateDisplay;
        set { _stateDisplay = value; OnPropertyChanged(); }
    }

    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public string TodayCount
    {
        get => _todayCount;
        set { _todayCount = value; OnPropertyChanged(); }
    }

    public string Streak
    {
        get => _streak;
        set { _streak = value; OnPropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SkipCommand { get; }

    public event Action? WorkCompleted;

    public MainViewModel(TimerService timer, HistoryService history, PomodoroSettings settings)
    {
        _timer = timer;
        _history = history;
        _settings = settings;

        StartCommand = new RelayCommand(ToggleStartPause);
        ResetCommand = new RelayCommand(Reset);
        SkipCommand = new RelayCommand(Skip);

        _timer.Tick += OnTick;
        _timer.StateChanged += OnStateChanged;
        _timer.WorkCompleted += OnWorkCompleted;

        _dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _dispatcherTimer.Tick += (s, e) => _timer.TickOnce();

        UpdateDisplay();
        UpdateStats();
    }

    private void ToggleStartPause()
    {
        if (_timer.State == TimerState.Idle)
        {
            _timer.Start();
            _dispatcherTimer.Start();
            IsRunning = true;
        }
        else
        {
            _timer.Pause();
            _dispatcherTimer.Stop();
            IsRunning = false;
        }
    }

    private void Reset()
    {
        _timer.Reset();
        _dispatcherTimer.Stop();
        IsRunning = false;
        UpdateDisplay();
    }

    private void Skip()
    {
        _timer.Skip();
    }

    private void OnTick()
    {
        UpdateDisplay();
    }

    private void OnStateChanged()
    {
        UpdateDisplay();
        IsRunning = _timer.State != TimerState.Idle;

        if (_timer.State == TimerState.Idle)
            _dispatcherTimer.Stop();
    }

    private void OnWorkCompleted()
    {
        _history.AddCompletedPomodoro();
        UpdateStats();
        WorkCompleted?.Invoke();
    }

    private void UpdateDisplay()
    {
        var minutes = _timer.RemainingSeconds / 60;
        var seconds = _timer.RemainingSeconds % 60;
        TimeDisplay = $"{minutes:D2}:{seconds:D2}";

        StateDisplay = _timer.State switch
        {
            TimerState.Idle => "就绪",
            TimerState.Working => "工作中",
            TimerState.ShortBreak => "短休息",
            TimerState.LongBreak => "长休息",
            _ => ""
        };
    }

    private void UpdateStats()
    {
        TodayCount = $"今日: {_history.GetTodayCount()} 个";
        Streak = $"连续: {_history.GetStreak()} 天";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
cd D:/Chance/coding/01
dotnet test PomodoroTimer.Tests/ --filter "MainViewModelTests" -v
```

Expected: All 4 tests PASS.

- [ ] **Step 6: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/ViewModels/ PomodoroTimer.Tests/MainViewModelTests.cs
git commit -m "feat: add MainViewModel with MVVM commands and display logic"
```

---

### Task 6: MainWindow UI

**Files:**
- Modify: `PomodoroTimer/MainWindow.xaml`
- Modify: `PomodoroTimer/MainWindow.xaml.cs`
- Create: `PomodoroTimer/Converters/StateToColorConverter.cs`

- [ ] **Step 1: Create StateToColorConverter**

```csharp
// PomodoroTimer/Converters/StateToColorConverter.cs
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using PomodoroTimer.Services;

namespace PomodoroTimer.Converters;

public class StateToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TimerState state)
        {
            return state switch
            {
                TimerState.Working => new SolidColorBrush(Color.FromRgb(233, 69, 96)),  // red
                TimerState.ShortBreak => new SolidColorBrush(Color.FromRgb(46, 213, 115)), // green
                TimerState.LongBreak => new SolidColorBrush(Color.FromRgb(46, 213, 115)),  // green
                _ => new SolidColorBrush(Color.FromRgb(238, 238, 238)), // white
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

- [ ] **Step 2: Write MainWindow XAML**

```xml
<!-- PomodoroTimer/MainWindow.xaml -->
<Window x:Class="PomodoroTimer.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:conv="clr-namespace:PomodoroTimer.Converters"
        Title="番茄钟"
        Width="360" Height="440"
        ResizeMode="NoResize"
        WindowStartupLocation="CenterScreen"
        Background="#1a1a2e">
    <Window.Resources>
        <conv:StateToColorConverter x:Key="StateToColor"/>
        <Style x:Key="TimerButton" TargetType="Button">
            <Setter Property="Background" Value="#16213e"/>
            <Setter Property="Foreground" Value="#eee"/>
            <Setter Property="FontSize" Value="14"/>
            <Setter Property="Padding" Value="20,8"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                                CornerRadius="8"
                                Padding="{TemplateBinding Padding}">
                            <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#1a3a5c"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Window.Resources>

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Title -->
        <TextBlock Grid.Row="0" Text="番茄钟"
                   FontSize="18" Foreground="#888"
                   HorizontalAlignment="Center" Margin="0,10,0,20"/>

        <!-- Timer Display -->
        <StackPanel Grid.Row="1" VerticalAlignment="Center">
            <TextBlock Text="{Binding TimeDisplay}"
                       FontSize="72" FontFamily="Consolas"
                       Foreground="{Binding TimerStateForColor, Converter={StaticResource StateToColor}}"
                       HorizontalAlignment="Center"/>
            <TextBlock Text="{Binding StateDisplay}"
                       FontSize="16" Foreground="#888"
                       HorizontalAlignment="Center" Margin="0,5,0,0"/>
        </StackPanel>

        <!-- Controls -->
        <StackPanel Grid.Row="2" Orientation="Horizontal"
                    HorizontalAlignment="Center" Margin="0,20,0,0">
            <Button Content="开始" Command="{Binding StartCommand}"
                    Style="{StaticResource TimerButton}" Margin="5"/>
            <Button Content="重置" Command="{Binding ResetCommand}"
                    Style="{StaticResource TimerButton}" Margin="5"/>
            <Button Content="跳过" Command="{Binding SkipCommand}"
                    Style="{StaticResource TimerButton}" Margin="5"/>
        </StackPanel>

        <!-- Stats -->
        <StackPanel Grid.Row="3" Orientation="Horizontal"
                    HorizontalAlignment="Center" Margin="0,20,0,0">
            <TextBlock Text="{Binding TodayCount}" Foreground="#666" FontSize="13" Margin="0,0,20,0"/>
            <TextBlock Text="{Binding Streak}" Foreground="#666" FontSize="13"/>
        </StackPanel>

        <!-- Settings Toggle -->
        <TextBlock Grid.Row="4" Text="⚙ 设置"
                   Foreground="#555" FontSize="12"
                   HorizontalAlignment="Center" Margin="0,15,0,0"
                   Cursor="Hand" MouseLeftButtonDown="OnSettingsClick"/>
    </Grid>
</Window>
```

- [ ] **Step 3: Update MainViewModel to expose TimerState for binding**

Add to `MainViewModel.cs` — a new property that the converter can bind to:

```csharp
// Add this property to MainViewModel.cs
public TimerState TimerStateForColor => _timer.State;
```

Update `OnStateChanged()` to notify this property:

```csharp
private void OnStateChanged()
{
    UpdateDisplay();
    IsRunning = _timer.State != TimerState.Idle;
    OnPropertyChanged(nameof(TimerStateForColor));

    if (_timer.State == TimerState.Idle)
        _dispatcherTimer.Stop();
}
```

- [ ] **Step 4: Write MainWindow code-behind**

```csharp
// PomodoroTimer/MainWindow.xaml.cs
using System.IO;
using System.Windows;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        var appDir = AppDomain.CurrentDomain.BaseDirectory;
        var settingsPath = Path.Combine(appDir, "settings.json");
        var historyPath = Path.Combine(appDir, "history.json");

        var settings = LoadSettings(settingsPath);
        var timer = new TimerService(settings);
        var history = new HistoryService(historyPath);

        _viewModel = new MainViewModel(timer, history, settings);
        DataContext = _viewModel;

        _viewModel.WorkCompleted += () =>
        {
            // Notification handled in Task 8
        };

        InitializeComponent();
    }

    private static PomodoroSettings LoadSettings(string path)
    {
        if (!File.Exists(path)) return new PomodoroSettings();
        try
        {
            var json = File.ReadAllText(path);
            var settings = System.Text.Json.JsonSerializer.Deserialize<PomodoroSettings>(json) ?? new PomodoroSettings();
            settings.Clamp();
            return settings;
        }
        catch
        {
            return new PomodoroSettings();
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        // Settings panel handled in Task 9
        MessageBox.Show("设置功能即将推出", "番茄钟");
    }
}
```

- [ ] **Step 5: Write App.xaml with dark theme**

```xml
<!-- PomodoroTimer/App.xaml -->
<Application x:Class="PomodoroTimer.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <Style TargetType="Window">
            <Setter Property="Foreground" Value="#eeeeee"/>
        </Style>
    </Application.Resources>
</Application>
```

- [ ] **Step 6: Build and verify**

```bash
cd D:/Chance/coding/01
dotnet build PomodoroTimer/
```

Expected: Build succeeded.

- [ ] **Step 7: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/MainWindow.xaml PomodoroTimer/MainWindow.xaml.cs PomodoroTimer/Converters/ PomodoroTimer/App.xaml PomodoroTimer/ViewModels/MainViewModel.cs
git commit -m "feat: add main window UI with dark theme and timer display"
```

---

### Task 7: SoundService

**Files:**
- Create: `PomodoroTimer/Services/SoundService.cs`

- [ ] **Step 1: Implement SoundService**

```csharp
// PomodoroTimer/Services/SoundService.cs
using System.Media;

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
            // Some systems don't support Console.Beep
        }
    }
}
```

- [ ] **Step 2: Wire into MainWindow code-behind**

In `MainWindow.xaml.cs`, update the `WorkCompleted` handler:

```csharp
// Replace the WorkCompleted handler in MainWindow constructor
var soundService = new SoundService(settings.SoundEnabled);

_viewModel.WorkCompleted += () =>
{
    soundService.PlayWorkComplete();
};
```

Add a `BreakCompleted` event to `MainViewModel`:

```csharp
// Add to MainViewModel.cs
public event Action? BreakCompleted;
```

And fire it from `OnBreakCompleted`:

```csharp
// Add to MainViewModel.cs - subscribe in constructor
_timer.BreakCompleted += OnBreakCompleted;

private void OnBreakCompleted()
{
    BreakCompleted?.Invoke();
}
```

Wire in MainWindow:

```csharp
_viewModel.BreakCompleted += () =>
{
    soundService.PlayBreakComplete();
};
```

- [ ] **Step 3: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/Services/SoundService.cs PomodoroTimer/MainWindow.xaml.cs PomodoroTimer/ViewModels/MainViewModel.cs
git commit -m "feat: add sound alerts for work and break completion"
```

---

### Task 8: NotificationService + System Tray

**Files:**
- Create: `PomodoroTimer/Services/NotificationService.cs`
- Modify: `PomodoroTimer/MainWindow.xaml`
- Modify: `PomodoroTimer/MainWindow.xaml.cs`

- [ ] **Step 1: Implement NotificationService**

```csharp
// PomodoroTimer/Services/NotificationService.cs
using System.Windows;

namespace PomodoroTimer.Services;

public class NotificationService
{
    public void ShowWorkComplete(int pomodoroCount)
    {
        Show("番茄钟", $"工作完成！今日第 {pomodoroCount} 个番茄 🍅");
    }

    public void ShowBreakComplete()
    {
        Show("番茄钟", "休息结束，继续加油！");
    }

    private static void Show(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
```

- [ ] **Step 2: Add system tray icon to XAML**

Add namespace to `MainWindow.xaml`:

```xml
xmlns:tb="http://www.hardcodet.net/taskbar"
```

Add before the closing `</Grid>`:

```xml
<!-- System Tray -->
<tb:TaskbarIcon x:Name="TrayIcon"
                ToolTipText="番茄钟"
                DoubleClickCommand="{Binding ShowWindowCommand}"
                Visibility="Visible"/>
```

- [ ] **Step 3: Add tray commands to MainViewModel**

```csharp
// Add to MainViewModel.cs
public ICommand ShowWindowCommand { get; }
public ICommand ExitCommand { get; }
public ICommand MinimizeToTrayCommand { get; }

// In constructor:
ShowWindowCommand = new RelayCommand(ShowWindow);
ExitCommand = new RelayCommand(() => System.Windows.Application.Current.Shutdown());
MinimizeToTrayCommand = new RelayCommand(MinimizeToTray);
```

Add methods:

```csharp
private void ShowWindow()
{
    var window = Application.Current.MainWindow;
    window.Show();
    window.WindowState = WindowState.Normal;
    window.Activate();
}

private void MinimizeToTray()
{
    Application.Current.MainWindow.Hide();
}
```

- [ ] **Step 4: Wire notifications in MainWindow**

Update `MainWindow.xaml.cs` constructor:

```csharp
var notificationService = new NotificationService();
var soundService = new SoundService(settings.SoundEnabled);

_viewModel.WorkCompleted += () =>
{
    soundService.PlayWorkComplete();
    notificationService.ShowWorkComplete(_viewModel.SessionPomodoroCount);
};

_viewModel.BreakCompleted += () =>
{
    soundService.PlayBreakComplete();
    notificationService.ShowBreakComplete();
};
```

Expose `SessionPomodoroCount` in MainViewModel:

```csharp
// Add to MainViewModel.cs
public int SessionPomodoroCount => _timer.SessionPomodoroCount;
```

- [ ] **Step 5: Override OnClosing to minimize to tray**

```csharp
// Add to MainWindow.xaml.cs
protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
{
    e.Cancel = true;
    _viewModel.MinimizeToTrayCommand.Execute(null);
}
```

- [ ] **Step 6: Add tray context menu in XAML**

```xml
<tb:TaskbarIcon x:Name="TrayIcon"
                ToolTipText="番茄钟"
                DoubleClickCommand="{Binding ShowWindowCommand}"
                Visibility="Visible">
    <tb:TaskbarIcon.ContextMenu>
        <ContextMenu>
            <MenuItem Header="显示主窗口" Command="{Binding ShowWindowCommand}"/>
            <Separator/>
            <MenuItem Header="退出" Command="{Binding ExitCommand}"/>
        </ContextMenu>
    </tb:TaskbarIcon.ContextMenu>
</tb:TaskbarIcon>
```

- [ ] **Step 7: Build and verify**

```bash
cd D:/Chance/coding/01
dotnet build PomodoroTimer/
```

Expected: Build succeeded.

- [ ] **Step 8: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/
git commit -m "feat: add notifications and system tray integration"
```

---

### Task 9: Settings Panel

**Files:**
- Modify: `PomodoroTimer/MainWindow.xaml`
- Modify: `PomodoroTimer/MainWindow.xaml.cs`

- [ ] **Step 1: Add settings overlay to MainWindow XAML**

Add after the main Grid inside the Window:

```xml
<!-- Settings Overlay -->
<Grid x:Name="SettingsOverlay" Background="#1a1a2e" Visibility="Collapsed" Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0" Text="设置" FontSize="18" Foreground="#888"
               HorizontalAlignment="Center" Margin="0,10,0,20"/>

    <!-- Work Duration -->
    <StackPanel Grid.Row="1" Margin="0,10">
        <TextBlock Text="工作时长（分钟）" Foreground="#aaa" FontSize="13"/>
        <TextBlock x:Name="WorkDurationLabel" Text="25" Foreground="#eee" FontSize="14" Margin="0,5"/>
        <Slider x:Name="WorkSlider" Minimum="1" Maximum="120" Value="25"
                TickFrequency="1" IsSnapToTickEnabled="True"
                ValueChanged="OnWorkSliderChanged"/>
    </StackPanel>

    <!-- Short Break Duration -->
    <StackPanel Grid.Row="2" Margin="0,10">
        <TextBlock Text="短休息时长（分钟）" Foreground="#aaa" FontSize="13"/>
        <TextBlock x:Name="ShortBreakLabel" Text="5" Foreground="#eee" FontSize="14" Margin="0,5"/>
        <Slider x:Name="ShortBreakSlider" Minimum="1" Maximum="60" Value="5"
                TickFrequency="1" IsSnapToTickEnabled="True"
                ValueChanged="OnShortBreakSliderChanged"/>
    </StackPanel>

    <!-- Long Break Duration -->
    <StackPanel Grid.Row="3" Margin="0,10">
        <TextBlock Text="长休息时长（分钟）" Foreground="#aaa" FontSize="13"/>
        <TextBlock x:Name="LongBreakLabel" Text="15" Foreground="#eee" FontSize="14" Margin="0,5"/>
        <Slider x:Name="LongBreakSlider" Minimum="1" Maximum="120" Value="15"
                TickFrequency="1" IsSnapToTickEnabled="True"
                ValueChanged="OnLongBreakSliderChanged"/>
    </StackPanel>

    <!-- Pomodoros Before Long Break -->
    <StackPanel Grid.Row="4" Margin="0,10">
        <TextBlock Text="长休息间隔（番茄数）" Foreground="#aaa" FontSize="13"/>
        <TextBlock x:Name="PomodoroCountLabel" Text="4" Foreground="#eee" FontSize="14" Margin="0,5"/>
        <Slider x:Name="PomodoroCountSlider" Minimum="1" Maximum="10" Value="4"
                TickFrequency="1" IsSnapToTickEnabled="True"
                ValueChanged="OnPomodoroCountSliderChanged"/>
    </StackPanel>

    <!-- Sound Toggle -->
    <CheckBox Grid.Row="5" x:Name="SoundToggle" Content="启用声音提醒"
              Foreground="#aaa" FontSize="13" Margin="0,10" IsChecked="True"/>

    <!-- Buttons -->
    <StackPanel Grid.Row="7" Orientation="Horizontal" HorizontalAlignment="Center">
        <Button Content="保存" Click="OnSaveSettings"
                Style="{StaticResource TimerButton}" Margin="5"/>
        <Button Content="取消" Click="OnCancelSettings"
                Style="{StaticResource TimerButton}" Margin="5"/>
    </StackPanel>
</Grid>
```

- [ ] **Step 2: Add settings code-behind**

Add to `MainWindow.xaml.cs`:

```csharp
private void OnSettingsClick(object sender, RoutedEventArgs e)
{
    // Show settings, hide main content
    SettingsOverlay.Visibility = Visibility.Visible;
}

private void OnWorkSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    WorkDurationLabel.Text = ((int)e.NewValue).ToString();
}

private void OnShortBreakSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    ShortBreakLabel.Text = ((int)e.NewValue).ToString();
}

private void OnLongBreakSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    LongBreakLabel.Text = ((int)e.NewValue).ToString();
}

private void OnPomodoroCountSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
{
    PomodoroCountLabel.Text = ((int)e.NewValue).ToString();
}

private void OnSaveSettings(object sender, RoutedEventArgs e)
{
    var settings = new PomodoroSettings
    {
        WorkMinutes = (int)WorkSlider.Value,
        ShortBreakMinutes = (int)ShortBreakSlider.Value,
        LongBreakMinutes = (int)LongBreakSlider.Value,
        PomodorosBeforeLongBreak = (int)PomodoroCountSlider.Value,
        SoundEnabled = SoundToggle.IsChecked ?? true
    };

    var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
    var json = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(settingsPath, json);

    SettingsOverlay.Visibility = Visibility.Collapsed;
    MessageBox.Show("设置已保存，重启应用后生效。", "番茄钟");
}

private void OnCancelSettings(object sender, RoutedEventArgs e)
{
    SettingsOverlay.Visibility = Visibility.Collapsed;
}
```

- [ ] **Step 3: Build and verify**

```bash
cd D:/Chance/coding/01
dotnet build PomodoroTimer/
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
cd D:/Chance/coding/01
git add PomodoroTimer/
git commit -m "feat: add settings panel with duration sliders and sound toggle"
```

---

### Task 10: Final Polish and Manual Testing

- [ ] **Step 1: Run all tests**

```bash
cd D:/Chance/coding/01
dotnet test -v
```

Expected: All tests PASS.

- [ ] **Step 2: Run the app and manually test**

```bash
cd D:/Chance/coding/01
dotnet run --project PomodoroTimer/
```

Manual test checklist:
- [ ] Timer starts and counts down correctly
- [ ] Pause and resume work
- [ ] Reset returns to initial time
- [ ] Timer completes and switches to break
- [ ] Break completes and switches back to work
- [ ] Skip button works
- [ ] Settings panel opens and sliders work
- [ ] Settings save and persist across restart
- [ ] Stats update after completing a pomodoro
- [ ] Sound plays on completion
- [ ] System notification appears
- [ ] Minimize to tray works
- [ ] Tray context menu works

- [ ] **Step 3: Final commit**

```bash
cd D:/Chance/coding/01
git add -A
git commit -m "chore: final polish and verification"
```
