using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Babel.Services;

public class IOService
{
    const string UserFolder = "babel-player";
    const string imageDirectory = "images";

    public static bool IsInitialized { get; private set; }

    public static void Initialize()
    {  
        var userPath = GetUserDataPath();
        var imagePath = GetImagePath();

        // Ensure user dir exists
        if (Directory.Exists(userPath) == false)
            Directory.CreateDirectory(userPath);
        
        if (Directory.Exists(imagePath) == false)
            Directory.CreateDirectory(imagePath);
        
        IsInitialized = true;
    }

    public static bool TryReadTextFile(string path, out string contents)
    {
        if (File.Exists(path) == false)
        {
            contents = "";
            return false;
        }

        contents = File.ReadAllText(path);
        return true;
    }

    public static void WriteTextFile(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    public static string[] FindFilesOfTypeAtPath(string path, string targetExtension, int maxDepth)
    {
        HashSet<string> toReturn = [];
        Stack<(string Path, int Depth)> dirStack = new();
        dirStack.Push((path, 0));

        while (dirStack.Count > 0)
        {
            var current = dirStack.Pop();

            if (Directory.Exists(current.Path) == false)
            {
                Console.WriteLine("Failed to find path: " + current.Path);
                continue;
            }

            string[] filesInDir = Directory.GetFiles(current.Path);
            for (int i = 0; i < filesInDir.Length; i++)
            {
                var fileExtension = Path.GetExtension(filesInDir[i]);
                if (fileExtension == targetExtension)
                    toReturn.Add(filesInDir[i]);
            }

            if ((current.Depth + 1) < maxDepth)
            {
                string[] subDirectories = Directory.GetDirectories(current.Path);
                for (int i = 0; i < subDirectories.Length; i++)
                {
                    dirStack.Push((subDirectories[i], current.Depth + 1));
                }
            }
        }

        return [.. toReturn];
    }

    public static string GetUserDataPath()
    {
        string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(baseFolder, UserFolder);
    }

    public static string GetImagePath()
    {
        string userPath = GetUserDataPath();
        return Path.Combine(userPath, imageDirectory);
    }
}