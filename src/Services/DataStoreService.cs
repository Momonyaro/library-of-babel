using System;
using System.IO;
using System.Linq;
using AudiobookPlayer.ViewModels;
using Babel.Models;
using Newtonsoft.Json;

namespace Babel.Services;

public sealed class DataStoreService
{
    const string PersistFileName = "library.dat";

    private static readonly Lazy<DataStoreService> _lazy = new(() => new DataStoreService());
    public static DataStoreService Instance { get { return _lazy.Value; } }
    
    private PersistantData? _data = null;


    public void Initialize()
    {
        if (!IOService.IsInitialized)
        {
            Console.WriteLine("[DataStoreService] Failed to initialize, IOService was not started!");
            return;
        }

        string persistDataPath = IOService.GetUserDataPath() + "/" + PersistFileName;
        if (IOService.TryReadTextFile(persistDataPath, out string jsonText))
        {
            _data = JsonConvert.DeserializeObject<PersistantData>(jsonText);
        }
        else
            _data = new();
    }

    private void Save()
    {
        string persistDataPath = IOService.GetUserDataPath() + "/" + PersistFileName;
        
        try
        {
            var jsonString = JsonConvert.SerializeObject(_data);
            IOService.WriteTextFile(persistDataPath, jsonString);
        } 
        catch (Exception) { throw; }
    }


#region Data Append

    public void AppendDirectPaths(string[] paths)
    {
        if (_data == null)
            return;

        var currentPaths = _data.DirectPaths.ToHashSet(); // Check for duplicates
        foreach (var path in paths)
        {
            currentPaths.Add(path);
        }

        _data.DirectPaths = [.. currentPaths];
        Save();
    }

    public void AppendContainerPaths(string[] paths)
    {
        if (_data == null)
            return;

        var currentPaths = _data.ContainerPaths.ToHashSet(); // Check for duplicates
        foreach (var path in paths)
        {
            currentPaths.Add(path);
        }

        _data.ContainerPaths = [.. currentPaths];
        Save();
    }

    public void UpdateBookmark(string bookTitle, TimeSpan timestamp)
    {
        if (_data == null)
            return;

        var bookmarks = _data.Bookmarks;
        var filtered = bookmarks.Where(bm => bm.BookTitle != bookTitle).ToList();

        filtered.Add(new () { BookTitle = bookTitle, Timestamp = timestamp });

        SetStoredBookmarks([.. filtered]);
    }

    public void SetAudiobookFavorite(string bookTitle, bool favorite)
    {
        if (_data == null)
            return;

        var books = _data.Books;
        for (int i = 0; i < books.Length; i++)
        {
            if (books[i]?.Title == bookTitle)
            {
                books[i].Favorite = favorite;
            }
        }

        _data.Books = [.. books];
        Save();
    }

    public void SetAudiobookFinished(string bookTitle, bool finished)
    {
        if (_data == null)
            return;

        var books = _data.Books;
        for (int i = 0; i < books.Length; i++)
        {
            if (books[i]?.Title == bookTitle)
            {
                books[i].Finished = finished;
            }
        }

        _data.Books = [.. books];
        Save();
    }

#endregion

#region Data Setters

    public void SetDirectPaths(string[] paths)
    {
        _data!.DirectPaths = paths;
        Save();
    }

    public void SetContainerPaths(string[] paths)
    {
        _data!.DirectPaths = paths;
        Save();
    }

    public void SetStoredBooks(AudiobookItem[] books)
    {
        _data!.Books = books;
        Save();
    }

    public void SetStoredBookmarks(BookmarkItem[] bookmarks)
    {
        _data!.Bookmarks = bookmarks;
        Save();
    }

    public void SetLatestListen(string audiobookTitle)
    {
        if (_data!.LatestListen == audiobookTitle)
            return;

        _data!.LatestListen = audiobookTitle;
        Save();
    }

#endregion

#region Data Getters

    public string[] GetDirectPaths()
    {
        if (_data == null)
            return [];
        
        return _data.DirectPaths;
    }

    public string[] GetContainerPaths()
    {
        if (_data == null)
            return [];
        
        return _data.ContainerPaths;
    }

    public AudiobookItem[] GetStoredBooks()
    {
        if (_data == null)
            return [];
        
        return _data.Books;
    }

    public AudiobookListItem[] GetStoredBooksAsListItems()
    {
        if (_data == null)
            return [];
        
        return [.. _data.Books.Select(b => new AudiobookListItem(
            b.Title, 
            b.Author, 
            b.Series ?? "", 
            b.SeriesEntry ?? "", 
            b.CoverImageRef ?? "", 
            b.Duration, 
            b.Favorite ?? false, 
            b.Finished ?? false
        ))];
    }

    public AudiobookListItem GetAudiobookListItem(AudiobookItem audiobook)
    {
        return new AudiobookListItem(
            audiobook.Title, 
            audiobook.Author, 
            audiobook.Series ?? "", 
            audiobook.SeriesEntry ?? "", 
            audiobook.CoverImageRef ?? "",
            audiobook.Duration,
            audiobook.Favorite ?? false, 
            audiobook.Finished ?? false
        );
    }

    public AudiobookItem? GetAudibookByName(string name)
    {
        if (_data == null)
            return null;
        
        return _data.Books.First(b => b.Title == name);
    }

    public AudiobookItem? GetLatestListen()
    {
        if (_data == null)
            return null;

        if (_data.LatestListen == null)
            return null;

        return GetAudibookByName(_data.LatestListen);
    }

    public BookmarkItem[] GetStoredBookmarks()
    {
        if (_data == null)
            return [];
        
        return _data.Bookmarks;
    }

    public BookmarkItem? GetBookmarkByName(string name)
    {
        if (_data == null)
            return null;
        
        return _data.Bookmarks.FirstOrDefault(b => b?.BookTitle == name, null);
    }

#endregion
}