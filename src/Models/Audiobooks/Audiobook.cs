using System;
using Newtonsoft.Json;

namespace Babel.Models;

public class AudiobookItem
{
    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Narrator { get; set; } = null!;
    public string? Publisher { get; set; }
    public string? Series { get; set; }
    public string? SeriesEntry { get; set; }
    public string? Description { get; set; }
    public string? Language { get; set; }
    public string? CoverImageRef { get; set; }
    public DateTime ReleaseDate { get; set; }

    public TimeSpan Duration { get; set; }
    public ChapterItem[] Chapters { get; set; } = [];

    public bool? Favorite { get; set; } = false;
    public bool? Finished { get; set; } = false;

    public string FilePath { get; set; } = null!;
    public bool IsValid { get; set; }
}

public class ChapterItem
{
    public int ID { get; set; }
    public string Name { get; set; } = null!;
    public TimeSpan TimestampStart { get; set; }
    public TimeSpan TimestampEnd { get; set; }
   
    [JsonIgnore] public TimeSpan ChapterDuration => TimestampEnd - TimestampStart;
    [JsonIgnore] public string TimestampStartString => $"{(int)TimestampStart.TotalHours}h {TimestampStart.Minutes}m";
    [JsonIgnore] public string TimestampEndString => $"{(int)TimestampEnd.TotalHours}h {TimestampEnd.Minutes}m";
    [JsonIgnore] public string ChapterDurationString => $"{(int)ChapterDuration.TotalMinutes}m {ChapterDuration.Seconds:D2}s";

}

public class BookmarkItem
{
    public string BookTitle { get; set; } = null!;
    public TimeSpan Timestamp { get; set; }
}