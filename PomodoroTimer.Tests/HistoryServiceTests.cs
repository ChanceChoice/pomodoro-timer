using System.IO;
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
