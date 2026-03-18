
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Babel.Models;

public class FFProbeResult
{
    public FFChapter[] Chapters { get; set; } = [];
    public FFFormat Format { get; set; } = null!;
    public FFStream[] Streams { get; set; } = [];
}

public class FFChapter
{
    public int Id { get; set; }
    [JsonPropertyName("time_base")] public string? TimeBase { get; set; }
    public long Start { get; set; }
    public long End { get; set; }
    [JsonPropertyName("start_time")] public string? StartTime { get; set; }
    [JsonPropertyName("end_time")] public string? EndTime { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
}

public class FFFormat
{
    public string? Filename { get; set; }
    public string? Duration { get; set; }
    public string? Size { get; set; }
    [JsonPropertyName("bit_rate")] public string? BitRate { get; set; }
    public Dictionary<string, string> Tags { get; set; } = [];
}

public class FFStream
{
    public int Index { get; set; }
    public string? Codec_Name { get; set; }
    public string? Codec_Type { get; set; }
    public int? Channels { get; set; }
    public string? Sample_Rate { get; set; }
    public string? Bit_Rate { get; set; }
    public Dictionary<string, int> Disposition { get; set; } = [];
    public Dictionary<string, string> Tags { get; set; } = [];
}