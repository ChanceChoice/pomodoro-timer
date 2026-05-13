using System.ComponentModel;
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

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        _viewModel.MinimizeToTrayCommand.Execute(null);
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        MainContent.Visibility = Visibility.Collapsed;
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
        MainContent.Visibility = Visibility.Visible;
        MessageBox.Show("设置已保存，重启应用后生效。", "番茄钟");
    }

    private void OnCancelSettings(object sender, RoutedEventArgs e)
    {
        SettingsOverlay.Visibility = Visibility.Collapsed;
        MainContent.Visibility = Visibility.Visible;
    }
}
