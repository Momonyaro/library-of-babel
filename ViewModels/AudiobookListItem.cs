using System;
using System.IO;
using Avalonia.Media.Imaging;
using Babel.Models;
using Babel.Services;

namespace AudiobookPlayer.ViewModels;

public class AudiobookListItem
{
    public string Title { get; set; }
    public string Author { get; set; }
    public bool HasSeries { get; private set; }
    public string Series { get; set; }
    public bool HasCoverArt { get; private set; }
    public string CoverArtPath { get; private set; }
    public string DurationString { get; private set; }
    public bool IsPlaying { get; set; }
    public bool Favorite { get; set; }
    public bool Finished { get; set; }

    public Bitmap? CoverArt { get; private set; }
    public AudiobookListItem(string title, string author, string series, string seriesEntry, string coverArtRef, TimeSpan duration, bool favorite, bool finished)
    {
        Title = title;
        Author = author;

        Series = $"{series}, Book {seriesEntry}";
        HasSeries = series != string.Empty;
        CoverArtPath = "";
        Favorite = favorite;
        Finished = finished;

        if (coverArtRef == string.Empty)
            HasCoverArt = false;
        else if (ImageScanService.Instance.TryRequestCoverArt(coverArtRef, out string coverArtPath))
        {
            CoverArtPath = coverArtPath;
            HasCoverArt = true;

            using var stream = File.OpenRead(CoverArtPath);
            CoverArt = new Bitmap(stream);
        }

        DurationString = $"{(int)duration.TotalHours}h {duration.Minutes}m";

        if (MediaPlayerService.Instance.IsPlaying)
        {
            IsPlaying = MediaPlayerService.Instance.CurrentlyPlaying!.Title == Title;
        }
    }
}

public class AudiobookDetailsTuple
{
    public AudiobookItem? Audiobook { get; set; }
    public AudiobookListItem? ListItem { get; set; }
}