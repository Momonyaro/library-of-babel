
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Babel.Models;
using Babel.Services;

namespace AudiobookPlayer.ViewModels;

public class AudiobookPlayerData : INotifyPropertyChanged
{
    public bool CurrentExists => MediaPlayerService.Instance.HasMedia;
    public bool CurrentIsPlaying => MediaPlayerService.Instance.IsPlaying;
    public string CurrentTitle => MediaPlayerService.Instance.CurrentlyPlaying?.Title ?? "Nothing Playing";
    public string CurrentAuthor => MediaPlayerService.Instance.CurrentlyPlaying?.Author ?? "";
    public ChapterItem[] ChapterList => MediaPlayerService.Instance.CurrentlyPlaying?.Chapters ?? [];
    public string CurrentChapter => MediaPlayerService.Instance.CurrentChapter?.Name.Trim() ?? "Unknown Chapter";
    public double CurrentChapterProgress => MediaPlayerService.Instance.CurrentChapterProgress;
    public string CurrentTimeRemaining => ($"{(int)_remainingTime.TotalHours}h {_remainingTime.Minutes}m" ?? "--:--") + " left";
    public string CurrentChapterOffset => $"{(int)Math.Abs(_chOffsetStart.TotalMinutes):D2}:{Math.Abs(_chOffsetStart.Seconds):D2}";
    public string CurrentChapterEndOffset => $"-{(int)Math.Abs(_chOffsetEnd.TotalMinutes):D2}:{Math.Abs(_chOffsetEnd.Seconds):D2}";
    public bool CurrentHasCover => _listItem?.HasCoverArt ?? false;
    public Bitmap? CurrentCover => _listItem?.CoverArt ?? null;


    private AudiobookListItem? _listItem;
    private TimeSpan _remainingTime = MediaPlayerService.Instance.TimeRemaining;
    private TimeSpan _chOffsetStart = MediaPlayerService.Instance.ChapterOffsetTimestamp;
    private TimeSpan _chOffsetEnd = MediaPlayerService.Instance.ChapterOffsetEndTimestamp;

    public AudiobookPlayerData(AudiobookListItem? listItem)
    {
        _listItem = listItem;

        MediaPlayerService.Instance.OnMediaMounted += OnMediaMounted;
        MediaPlayerService.Instance.OnPlaybackChanged += OnPlaybackChanged;
        MediaPlayerService.Instance.OnTimeUpdated += OnTimeUpdated;
        MediaPlayerService.Instance.OnChapterUpdated += OnChapterUpdated;
    }

    private void OnMediaMounted()
    {
        UpdateEverything();
    }

    private void OnPlaybackChanged()
    {
        UpdateEverything();
    }

    private void OnChapterUpdated()
    {
        Raise(nameof(CurrentChapter));
    }

    private void OnTimeUpdated()
    {
        _remainingTime = MediaPlayerService.Instance.TimeRemaining;
        _chOffsetStart = MediaPlayerService.Instance.ChapterOffsetTimestamp;
        _chOffsetEnd   = MediaPlayerService.Instance.ChapterOffsetEndTimestamp;

        Raise(nameof(CurrentTimeRemaining));
        Raise(nameof(CurrentChapterProgress));
        Raise(nameof(CurrentChapterOffset));
        Raise(nameof(CurrentChapterEndOffset));
    }

    private void UpdateEverything()
    {
        Raise(nameof(CurrentExists));
        Raise(nameof(CurrentIsPlaying));
        Raise(nameof(CurrentTitle));
        Raise(nameof(CurrentAuthor));
        Raise(nameof(ChapterList));
        
        OnTimeUpdated();
    }

    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void Raise([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}