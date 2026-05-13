using System.Windows;

namespace PomodoroTimer.Services;

public class NotificationService
{
    public void ShowWorkComplete(int pomodoroCount)
    {
        Show("番茄钟", $"工作完成！今日第 {pomodoroCount} 个番茄");
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
