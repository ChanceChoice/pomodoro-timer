using System.IO;
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
