using Babel.Models;

public class PersistantData
{
    public string[] DirectPaths { get; set; } = [];
    public string[] ContainerPaths { get; set; } = [];

    public string? LatestListen { get; set; }

    public AudiobookItem[] Books { get; set; } = [];
    public BookmarkItem[] Bookmarks { get; set; } = [];
}