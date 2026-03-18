using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Babel.Models;
using Babel.Utils;

namespace Babel.Services;

public sealed class LibraryScanService
{
    private static readonly Lazy<LibraryScanService> _lazy = new(() => new LibraryScanService());
    public static LibraryScanService Instance { get { return _lazy.Value; } }

    public bool Scanning => _scanning;

    public event Action? OnScanInitiated;
    public event Action? OnScanCompleted; 

    private bool _scanning;


    public void StartLibraryScan()
    {
        AudiobookItem[] library = DataStoreService.Instance.GetStoredBooks();
        string[] directPaths = DataStoreService.Instance.GetDirectPaths();
        string[] containerPaths = DataStoreService.Instance.GetContainerPaths();

        // Here we need to check the existing library for if any files have been moved and mark the entry as invalid.

        // Get the already scanned file paths so we can filter those out after scanning the containers
        string[] libraryFilePaths = [.. library.Select(a => a.FilePath)];

        // Filter out direct paths to existing library files.
        directPaths = [.. directPaths.Where(dp => !libraryFilePaths.Contains(dp))];

        // Get all the files of desired type in container paths
        HashSet<string> containerFiles = [];
        for (int i = 0; i < containerPaths.Length; i++)
        {
            var results = IOService.FindFilesOfTypeAtPath(containerPaths[i], ".m4b", 2);
            for (int j = 0; j < results.Length; j++)
            {
                containerFiles.Add(results[j]);
            }
        }

        string[] filteredContainerFiles = [.. containerFiles.Where(dp => !libraryFilePaths.Contains(dp) && !directPaths.Contains(dp))];
        string[] finalFiles = [..directPaths, ..filteredContainerFiles];

        Task.Run(() => PerformScanAsync(library, finalFiles));
    }

    public async Task PerformScanAsync(AudiobookItem[] existingLibrary, string[] filePaths)
    {
        // Iterate through the existing library and for each item, check the filepath and try parsing it again.
        // If the file doesn't exist mark the AudiobookItem as such and move on.

        // When done with all the books, return the newly updated array as the valid library.

        _scanning = true;
        OnScanInitiated?.Invoke();

        List<AudiobookItem> refreshedLibrary = [..existingLibrary];
        
        for (int i = 0; i < filePaths.Length; i++)
        {

            if (System.IO.File.Exists(filePaths[i]) == false)
            {
                Console.WriteLine("Unable to find file at path during scan: " + filePaths[i]);
                continue;
            }
            
            // Here's where we go apeshit apparently...
            FFProbeResult? probeResult = await RunFFProbe(filePaths[i]);
            if (probeResult == null || probeResult.Format!.Tags == null)
            {
                Console.WriteLine("Unable to probe file at path during scan: " + filePaths[i]);
                continue;
            }
            
            Console.WriteLine("Found file -> " + probeResult.Format.Tags.GetValueOrDefault("title", "Unknown Title"));

            AudiobookItem updated = MetadataUtils.CreateAudiobookItem(probeResult, filePaths[i]);

            ChapterItem[] chapters = new ChapterItem[probeResult.Chapters.Length];
            for (int c = 0; c < chapters.Length; c++)
            {
                chapters[c] = MetadataUtils.CreateChapterItem(probeResult.Chapters[c]);
            }

            updated.Chapters = chapters;
            refreshedLibrary.Add(updated);
        }


        // Save it all back in the data store
        if (refreshedLibrary.Count - existingLibrary.Length != 0)
        DataStoreService.Instance.SetStoredBooks([.. refreshedLibrary]);

        Console.WriteLine($"Performed scan! Retrieved {refreshedLibrary.Count - existingLibrary.Length} new book(s)!");

        _scanning = false;
        OnScanCompleted?.Invoke();
    }

    private async Task<FFProbeResult?> RunFFProbe(string path)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v quiet -print_format json -show_format -show_streams -show_chapters \"{path}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return null;

            string json = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return JsonSerializer.Deserialize<FFProbeResult>(json, new JsonSerializerOptions{
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}