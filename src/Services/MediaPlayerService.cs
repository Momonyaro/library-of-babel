using System;
using System.Linq;
using Babel.Models;
using LibVLCSharp.Shared;

namespace Babel.Services;

public sealed class MediaPlayerService : IDisposable
{
    private static readonly Lazy<MediaPlayerService> _lazy = new(() => new MediaPlayerService());
    public static MediaPlayerService Instance { get { return _lazy.Value; } }

    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;

    public event Action? OnMediaMounted;
    public event Action? OnTimeUpdated;
    public event Action? OnChapterUpdated;
    public event Action? OnPlaybackChanged;
    public event Action? OnMediaEnd;

    public AudiobookItem? CurrentlyPlaying { get; private set; }
    public TimeSpan TimeRemaining => GetRemainingTime();
    public TimeSpan Time => GetTime();
    public ChapterItem? CurrentChapter { get; private set; }
    public double CurrentChapterProgress => GetCurrentChapterProgress();
    public TimeSpan ChapterOffsetTimestamp => GetChapterOffsetTimestamp();
    public TimeSpan ChapterOffsetEndTimestamp => GetChapterOffsetEndTimestamp();

    public int Volume => _mediaPlayer.Volume;
    public bool IsPlaying => _mediaPlayer.IsPlaying;
    public bool HasMedia => _mediaPlayer.Media != null;
    private bool _pauseNextMedia = false;

    private MediaPlayerService()
    {
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);

        _mediaPlayer.TimeChanged += OnTimeChanged;
        _mediaPlayer.Playing += OnPlaybackStarted;
        _mediaPlayer.Paused += OnPlaybackPaused;
        _mediaPlayer.Stopped += OnPlaybackStopped;
        _mediaPlayer.MediaChanged += OnMediaChanged;
        _mediaPlayer.ChapterChanged += OnChapterChanged;
        _mediaPlayer.EndReached += OnEndReached;

        OnPlaybackChanged += CheckToPauseNextMedia;
    }

    public void PlayAudiobook(AudiobookItem? audiobook, int chapterId = -1)
    {
        if (audiobook == null)
            return;

        CurrentlyPlaying = audiobook;
        DataStoreService.Instance.SetLatestListen(audiobook.Title);

        string filepath = audiobook.FilePath;

        using var media = new Media(_libVLC, new Uri(filepath));
        _mediaPlayer.Play(media);

        if (chapterId >= 0)
            SeekToChapter(chapterId);
        else
        {
            // Check for bookmark and set that as the active chapter
            BookmarkItem? bookmark = DataStoreService.Instance.GetBookmarkByName(audiobook.Title);
            if (bookmark != null)
                SeekToTimestamp(bookmark.Timestamp);
        }
    }

    public void SetAudiobookNoPlay(AudiobookItem? audiobook)
    {
        _pauseNextMedia = true;
        PlayAudiobook(audiobook);
    }

    public void SeekToChapter(int chapterId)
    {
        if (CurrentlyPlaying == null || HasMedia == false)
        {
            Console.WriteLine("Failed to seek to chapter, no media is currently playing or queued for playing");
            return;
        }

        if (_mediaPlayer.IsSeekable == false)
        {
            Console.WriteLine("Media is not seekable? Please double check file!");
            return;
        }

        var chapter = CurrentlyPlaying.Chapters.First(c => c.ID == chapterId);
        if (chapter == null)
            return;
        
        CurrentChapter = chapter;
        SeekToTimestamp(chapter.TimestampStart);
    }

    public void SeekToTimestamp(TimeSpan timestamp)
    {
        if (CurrentlyPlaying == null || HasMedia == false)
        {
            Console.WriteLine("Failed to seek to chapter, no media is currently playing or queued for playing");
            return;
        }

        if (_mediaPlayer.IsSeekable == false)
        {
            Console.WriteLine("Media is not seekable? Please double check file!");
            return;
        }

        long totalMs = timestamp.Ticks / TimeSpan.TicksPerMillisecond;
        _mediaPlayer.Time = totalMs;
    }

    public void SeekRelativeSeconds(float seconds)
    {
        if (_mediaPlayer.IsSeekable == false)
            return;

        TimeSpan asTimespan = TimeSpan.FromSeconds(seconds);
        TimeSpan newFileTime = TimeSpan.FromMilliseconds(_mediaPlayer.Time) + asTimespan;

        _mediaPlayer.SeekTo(newFileTime);
    }

    public void SeekNextChapter()
    {
        if (_mediaPlayer.IsSeekable == false)
            return;

        _mediaPlayer.NextChapter();
    }

    public void SeekLastChapter()
    {
        if (_mediaPlayer.IsSeekable == false)
            return;
            
        _mediaPlayer.PreviousChapter();
    }

    public void Resume()
    {
        if (!_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Play();
            OnPlaybackChanged?.Invoke();
        }
    }

    public void Pause()
    {
        if (_mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Pause();
            OnPlaybackChanged?.Invoke();
        }
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
        CurrentlyPlaying = null;
        OnPlaybackChanged?.Invoke();
    }

    public void Dispose()
    {
        _mediaPlayer.TimeChanged -= OnTimeChanged;

        _mediaPlayer.Dispose();
        _libVLC.Dispose();
    }

    public void SetVolume(int volume)
    {
        _mediaPlayer.Volume = int.Clamp(volume, 0, 100);
    }

    private ChapterItem? GetCurrentChapter()
    {
        // Should be populated in this context since it's playing
        var mpCurrentChapter = _mediaPlayer.Chapter;
        var chapters = CurrentlyPlaying!.Chapters;

        var match = chapters.First(c => c.ID == mpCurrentChapter);
        return match;
    }

    private TimeSpan GetRemainingTime()
    {
        long msRemaining = _mediaPlayer.Length - _mediaPlayer.Time;
        return TimeSpan.FromMilliseconds(msRemaining);
    }

    private TimeSpan GetTime()
    {
        return TimeSpan.FromMilliseconds(_mediaPlayer.Time);
    }

    private double GetCurrentChapterProgress()
    {
        if (CurrentChapter == null || !HasMedia)
            return 0;
        
        var chapterLength = CurrentChapter.TimestampEnd - CurrentChapter.TimestampStart;
        var positionInChapter = TimeSpan.FromMilliseconds(_mediaPlayer.Time) - CurrentChapter.TimestampStart;

        return Math.Clamp(positionInChapter.TotalMilliseconds / chapterLength.TotalMilliseconds, 0, 1);
    }

    private TimeSpan GetChapterOffsetTimestamp()
    {
        if (CurrentChapter == null || !HasMedia)
            return TimeSpan.FromSeconds(0);
        
        TimeSpan truncated = TimeSpan.FromSeconds(Math.Floor(TimeSpan.FromMilliseconds(_mediaPlayer.Time).TotalSeconds));
        TimeSpan truncatedOffset = TimeSpan.FromSeconds(Math.Floor(CurrentChapter.TimestampStart.TotalSeconds));

        return truncated - truncatedOffset;
    }

    private TimeSpan GetChapterOffsetEndTimestamp()
    {
        if (CurrentChapter == null || !HasMedia)
            return TimeSpan.FromSeconds(0);
        
        TimeSpan truncated = TimeSpan.FromSeconds(Math.Floor(TimeSpan.FromMilliseconds(_mediaPlayer.Time).TotalSeconds));
        TimeSpan truncatedOffset = TimeSpan.FromSeconds(Math.Floor(CurrentChapter.TimestampEnd.TotalSeconds));
        
        return truncated - truncatedOffset;
    }

#region Events

    private void CheckToPauseNextMedia()
    {
        if (_pauseNextMedia == false)
            return;
        
        _pauseNextMedia = false;
        _mediaPlayer.Pause();
    }

    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        OnTimeUpdated?.Invoke();
    }

    private void OnChapterChanged(object? sender, MediaPlayerChapterChangedEventArgs e)
    {
        var lastChapter = CurrentChapter;
        CurrentChapter = GetCurrentChapter();

        OnChapterUpdated?.Invoke();
    }

    private void OnEndReached(object? sender, EventArgs e)
    {
        DataStoreService.Instance.SetAudiobookFinished(CurrentlyPlaying!.Title, true);
        OnMediaEnd?.Invoke();
    }

    private void OnPlaybackStarted(object? sender, EventArgs e) => OnPlaybackChanged?.Invoke();
    private void OnPlaybackPaused(object? sender, EventArgs e) => OnPlaybackChanged?.Invoke();
    private void OnPlaybackStopped(object? sender, EventArgs e) => OnPlaybackChanged?.Invoke();
    private void OnMediaChanged(object? sender, MediaPlayerMediaChangedEventArgs e) => OnMediaMounted?.Invoke();

#endregion
}