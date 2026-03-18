using System;
using System.Collections.Generic;
using Babel.Models;

namespace Babel.Utils;

public static class MetadataUtils
{

    public static AudiobookItem CreateAudiobookItem(FFProbeResult fProbeResult, string filePath)
    {
        FFFormat formatSection = fProbeResult.Format;

        return new()
        {
            Title = GetTitle(formatSection).Trim(),
            Author = GetAuthor(formatSection).Trim(),
            Narrator = GetNarrator(formatSection).Trim(),
            Publisher = GetPublisher(formatSection),
            Series = GetSeries(formatSection),
            SeriesEntry = GetSeriesEntry(formatSection),
            Description = GetDescription(formatSection),
            Language = GetLanguage(formatSection),
            CoverImageRef = ContainsImage(fProbeResult.Streams),
            ReleaseDate = GetReleaseDate(formatSection),

            Duration = GetDuration(formatSection),

            FilePath = filePath,
            IsValid = true
        };
    }

    public static ChapterItem CreateChapterItem(FFChapter fChapter)
    {
        return new()
        {
            ID = fChapter.Id,
            Name = fChapter.Tags.GetValueOrDefault("title", "Unknown Chapter").Trim(),
            TimestampStart = TimeSpan.FromSeconds(Convert.ToDouble(fChapter.StartTime)),
            TimestampEnd = TimeSpan.FromSeconds(Convert.ToDouble(fChapter.EndTime)),
        };
    }

    // ----------------------------

    public static string GetTitle(FFFormat fFormat)
    {
        return fFormat.Tags.GetValueOrDefault("title", "Unknown Title");
    }
    
    public static string GetAuthor(FFFormat fFormat)
    {
        return fFormat.Tags.GetValueOrDefault("artist", "Unknown Artist");
    }

    public static string? GetSeries(FFFormat fFormat)
    {
        if (IsAudible(fFormat))
            return fFormat.Tags.GetValueOrDefault("SERIES");
        else
            return null;
    }

    public static string? GetSeriesEntry(FFFormat fFormat)
    {
        if (IsAudible(fFormat))
            return fFormat.Tags.GetValueOrDefault("PART");
        else
            return null;
    }

    public static string GetNarrator(FFFormat fFormat)
    {
        if (IsAudible(fFormat))
            return fFormat.Tags.GetValueOrDefault("composer", "Unknown Narrator");
        else
            return "Unknown Narrator";
    }

    public static string? GetDescription(FFFormat fFormat)
    {
        return fFormat.Tags.GetValueOrDefault("comment");
    }

    public static string? GetPublisher(FFFormat fFormat)
    {
        if (IsAudible(fFormat))
            return fFormat.Tags.GetValueOrDefault("PUBLISHER");
        else
            return null!;
    }

    public static string? GetLanguage(FFFormat fFormat)
    {
        if (IsAudible(fFormat))
            return fFormat.Tags.GetValueOrDefault("LANGUAGE");
        else
            return null!;
    }

    public static DateTime GetReleaseDate(FFFormat fFormat)
    {
        return DateTime.Parse(
            fFormat.Tags.GetValueOrDefault("creation_time", new DateTime().ToLongDateString())
        );
    }

    public static TimeSpan GetDuration(FFFormat fFormat)
    {
        return TimeSpan.FromSeconds(Convert.ToDouble(fFormat.Duration));
    }

    public static string? ContainsImage(FFStream[] streams)
    {
        bool containsImage = false;

        for (int i = 0; i < streams.Length; i++)
        {
            if (streams[i].Disposition.GetValueOrDefault("attached_pic", 0) == 1)
                containsImage = true;    
        }

        if (containsImage == false)
            return null;

        // Create an image ID for future service to use
        return Guid.NewGuid().ToString("N")[..12].ToLowerInvariant(); // 12 hex chars
    }

    private static bool IsAudible(FFFormat fFormat)
    {
        return fFormat.Tags.ContainsKey("AUDIBLE_ASIN");
    }
}