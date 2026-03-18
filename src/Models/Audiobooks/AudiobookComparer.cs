using System;
using System.Collections.Generic;
using System.Globalization;
using AudiobookPlayer.ViewModels;


// Stolen from https://www.conradakunga.com/blog/custom-sorting-to-ignore-articles-like-a-the-in-c-net/ 
public sealed class AudiobookComparer : IEqualityComparer<AudiobookListItem>
{
    // Define the articles to ignore in the sort
    private static readonly string[] Articles = ["a ", "an ", "the "];

    // Create a CompareInfo object for comparison
    private static readonly CompareInfo CompareInfo = CultureInfo.InvariantCulture.CompareInfo;
  
  
    // Static instance creator
    public static readonly AudiobookComparer Instance = new();

    // Sanitize our input strings
    private static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var trimmed = input.TrimStart();

        foreach (var article in Articles)
        {
            if (trimmed.StartsWith(article, StringComparison.InvariantCultureIgnoreCase))
                return trimmed.Substring(article.Length).TrimStart();
        }

        return trimmed;
    }

    // Do the comparison
    public int CompareTitles(AudiobookListItem? x, AudiobookListItem? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        return CompareNumericStrings(Normalize(x.Title), Normalize(y.Title));
    }

    public int CompareSeries(AudiobookListItem? x, AudiobookListItem? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;

        return CompareNumericStrings(Normalize(x.Series), Normalize(y.Series));
    }

    public int CompareNumericStrings(string? x, string? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var xSpan = x.AsSpan();
        var ySpan = y.AsSpan();
        
        var commonPrefixLength = xSpan.CommonPrefixLength(ySpan);

        while (commonPrefixLength > 0)
        {
            xSpan = xSpan[commonPrefixLength..];
            ySpan = ySpan[commonPrefixLength..];
            commonPrefixLength = xSpan.CommonPrefixLength(ySpan);
        }
        
        if (int.TryParse(xSpan, out var xNumber) && 
            int.TryParse(ySpan, out var yNumber))
        {
            return xNumber.CompareTo(yNumber);
        }

        return xSpan.CompareTo(ySpan, StringComparison.Ordinal);
    }

    // Equality override
    public bool Equals(AudiobookListItem? x, AudiobookListItem? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return string.Equals(Normalize(x.Title), Normalize(y.Title), StringComparison.InvariantCultureIgnoreCase);
    }

    // Hashcode override
    public int GetHashCode(AudiobookListItem? obj)
    {
        if (obj?.Title is null)
            return 0;

        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Normalize(obj.Title));
    }
}
