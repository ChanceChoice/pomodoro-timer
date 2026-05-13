using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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

    public TimerState TimerStateForColor => _timer.State;
    public int SessionPomodoroCount => _timer.SessionPomodoroCount;

    public ICommand StartCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SkipCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand MinimizeToTrayCommand { get; }

    public event Action? WorkCompleted;
    public event Action? BreakCompleted;

    public MainViewModel(TimerService timer, HistoryService history, PomodoroSettings settings)
    {
        _timer = timer;
        _history = history;
        _settings = settings;

        StartCommand = new RelayCommand(ToggleStartPause);
        ResetCommand = new RelayCommand(Reset);
        SkipCommand = new RelayCommand(Skip);
        ShowWindowCommand = new RelayCommand(ShowWindow);
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
        MinimizeToTrayCommand = new RelayCommand(MinimizeToTray);

        _timer.Tick += OnTick;
        _timer.StateChanged += OnStateChanged;
        _timer.WorkCompleted += OnWorkCompleted;
        _timer.BreakCompleted += OnBreakCompleted;

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

    private void OnTick()
    {
        UpdateDisplay();
    }

    private void OnStateChanged()
    {
        UpdateDisplay();
        IsRunning = _timer.State != TimerState.Idle;
        OnPropertyChanged(nameof(TimerStateForColor));

        if (_timer.State == TimerState.Idle)
            _dispatcherTimer.Stop();
    }

    private void OnWorkCompleted()
    {
        _history.AddCompletedPomodoro();
        UpdateStats();
        WorkCompleted?.Invoke();
    }

    private void OnBreakCompleted()
    {
        BreakCompleted?.Invoke();
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
