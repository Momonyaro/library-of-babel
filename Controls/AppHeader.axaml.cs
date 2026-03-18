using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Babel.Services;

namespace AudiobookPlayer.Controls;

public partial class AppHeader : UserControl
{
    public AppHeader()
    {
        InitializeComponent();
    }

    public void OnClick_RefreshLibrary(object sender, RoutedEventArgs args)
    {
        LibraryScanService.Instance.StartLibraryScan();
    }

    private async void OnClick_AddFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
            return;

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new("Audio Files")
                {
                    Patterns = ["*.m4b"]
                }
            ]   
        });

        if (files.Count > 0)
        {
            var paths = files.Select(f => f.TryGetLocalPath()).Where(p => p != null).ToArray();
            if (paths.Length > 0)
            {
                DataStoreService.Instance.AppendDirectPaths(paths!);
                LibraryScanService.Instance.StartLibraryScan();
            }
        }
    }

    private async void OnClick_AddFolder(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel == null)
            return;

        // Start async operation to open the dialog.
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Add Folder",
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var paths = folders.Select(f => f.TryGetLocalPath()).Where(p => p != null).ToArray();
            if (paths.Length > 0)
            {
                DataStoreService.Instance.AppendContainerPaths(paths!);
                LibraryScanService.Instance.StartLibraryScan();
            }
        }
    }
}