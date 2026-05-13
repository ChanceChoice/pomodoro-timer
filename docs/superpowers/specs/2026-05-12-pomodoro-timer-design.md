# Pomodoro Timer - Windows Desktop App

## Overview

A minimal, clean Pomodoro timer Windows desktop app built with WPF (C#). Features customizable work/break durations, sound notifications, system tray integration, and daily statistics tracking.

## Tech Stack

- **Framework:** WPF (.NET 8)
- **Pattern:** MVVM (Model-View-ViewModel)
- **System Tray:** Hardcodet.NotifyIcon.Wpf (NuGet)
- **Storage:** System.Text.Json (local JSON files)
- **Sound:** System.Media.SoundPlayer

## Features

### Core Timer
- Default: 25 min work / 5 min short break / 15 min long break
- Long break triggers every 4 pomodoros
- States: Idle, Working, ShortBreak, LongBreak
- Auto-transition between work and break states
- Start / Pause / Reset controls

### Customization
- Work duration (1-120 min)
- Short break duration (1-60 min)
- Long break duration (1-120 min)
- Pomodoros before long break (1-10)
- Sound on/off toggle
- Settings persisted to `settings.json` in app directory

### Notifications
- Sound alert on state change (different tones for work end vs break end)
- Windows system notification toast on state change
- System tray icon with context menu (Show / Exit)
- Minimize to tray on window close (optional)

### Statistics
- Daily completed pomodoro count
- Current streak (consecutive days with at least 1 pomodoro)
- Stored in `history.json` in app directory
- Resets display at midnight, preserves history

## UI Design

### Main Window (320x400, non-resizable)

```
┌─────────────────────────────┐
│          番茄钟              │
│                             │
│          25:00              │  ← 72pt monospace, red (work) / green (break)
│          工作中              │  ← status label
│                             │
│     [开始]  [重置]  ⚙       │  ← buttons
│                             │
│   今日: 3 个   连续: 5 天    │  ← stats
└─────────────────────────────┘
```

### Settings Panel
- Slides in or opens as overlay within the same window
- Sliders or number inputs for durations
- Toggle for sound
- Save / Cancel buttons

### Color Scheme
- Dark background (#1a1a2e)
- Red timer text (#e94560) during work
- Green timer text (#0f3460) during break
- Light text (#eee)
- Accent buttons with rounded corners

## Architecture

### Project Structure
```
PomodoroTimer/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / MainWindow.xaml.cs
├── ViewModels/
│   └── MainViewModel.cs
├── Models/
│   ├── PomodoroSettings.cs
│   └── PomodoroRecord.cs
├── Services/
│   ├── TimerService.cs
│   ├── HistoryService.cs
│   ├── SoundService.cs
│   └── NotificationService.cs
├── Converters/
│   └── StateToColorConverter.cs
└── Resources/
    └── sounds/
        ├── work-end.wav
        └── break-end.wav
```

### Data Models

**PomodoroSettings**
- WorkMinutes (int, default 25)
- ShortBreakMinutes (int, default 5)
- LongBreakMinutes (int, default 15)
- PomodorosBeforeLongBreak (int, default 4)
- SoundEnabled (bool, default true)

**PomodoroRecord**
- Date (DateOnly)
- CompletedCount (int)

### Services

**TimerService**
- Manages countdown with DispatcherTimer (1s interval)
- Exposes: Start(), Pause(), Reset(), Skip()
- Events: Tick, StateChanged, Completed
- Tracks current state and pomodoro count in session

**HistoryService**
- Load/Save history from history.json
- AddCompletedPomodoro(date)
- GetTodayCount(), GetStreak()

**SoundService**
- Play work-end or break-end sound
- Uses embedded resources or app directory sounds

**NotificationService**
- ShowWindowsToast(title, message)
- Uses Windows.UI.Notifications or simple MessageBox fallback

## Error Handling
- Invalid settings values: clamp to min/max range
- Missing sound files: silent fallback, no crash
- Corrupted JSON: reset to defaults
- Single instance: prevent multiple app instances

## Testing Strategy
- Manual testing of timer states and transitions
- Verify settings persistence across restarts
- Verify stats accuracy and streak calculation
- Test system tray behavior (minimize, restore, exit)
