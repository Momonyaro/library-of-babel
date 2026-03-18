using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Babel.Models;

namespace Babel.Services;

public sealed class ImageScanService
{
    private static readonly Lazy<ImageScanService> _lazy = new(() => new ImageScanService());
    public static ImageScanService Instance { get { return _lazy.Value; } }

    public bool Scanning => _scanning;

    public event Action? OnScanInitiated;
    public event Action? OnScanCompleted; 
    public event Action? OnImageFetched;

    private bool _scanning;
    private Queue<string> _imgRequestQueue = [];
    private HashSet<string> _fetchedImages = [];


    public bool TryRequestCoverArt(string imageRef, out string filePath)
    {
        var coverArtPath = GetPathToImage(imageRef);

        if (File.Exists(coverArtPath))
        {
            filePath = coverArtPath;
            return true;
        }
        else
        {
            filePath = "";
            if (_fetchedImages.Contains(imageRef))
                return false;

            if (!_imgRequestQueue.Contains(imageRef))
                _imgRequestQueue.Enqueue(imageRef); // Try to find the file and get the cover through ffmpeg
            

            if (!_scanning)
                StartImageScan();

            return false;
        }
    }

    public void StartImageScan()
    {
        _scanning = true;
        OnScanInitiated?.Invoke();
        AudiobookItem[] library = DataStoreService.Instance.GetStoredBooks();

        Dictionary<string, AudiobookItem?> coverRefToAudiobook = [];

        for (int i = 0; i < library.Length; i++)
        {
            if (library[i].CoverImageRef != null) 
                coverRefToAudiobook.Add(library[i].CoverImageRef!, library[i]);
        }

        Task.Run(() => PerformScanAsync(coverRefToAudiobook)).ContinueWith((x) =>
        {
            _scanning = false;
            OnScanCompleted?.Invoke();
        });
    }

    public async Task PerformScanAsync(Dictionary<string, AudiobookItem?> imgToAudibookMap)
    {
        while (_imgRequestQueue.Any())
        {
            string currentImageRef = _imgRequestQueue.Peek();

            AudiobookItem? audiobook = imgToAudibookMap.GetValueOrDefault(currentImageRef, null);
            if (audiobook == null)
            {
                Console.WriteLine("Failed to find audiobook with image ref: " + currentImageRef);
                continue;
            }

            var outputPath = GetPathToImage(currentImageRef);
            await RunFFMpegImageFetch(audiobook.FilePath, outputPath);

            _fetchedImages.Add(_imgRequestQueue.Dequeue());
            Console.WriteLine("Fetched image! path -> " + outputPath);
            OnImageFetched?.Invoke();
        }
    }

    private async Task RunFFMpegImageFetch(string filePath, string outputPath)
    {
        try
        {
            var args = $"-i \"{filePath}\" -frames:v 1 -vf \"scale=512:512:force_original_aspect_ratio=decrease\" -y \"{outputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private string GetPathToImage(string imageRef)
    {
        var imageFolderPath = IOService.GetImagePath();
        return Path.Combine(imageFolderPath, $"{imageRef}.jpg");
    }
}