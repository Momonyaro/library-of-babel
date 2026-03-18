using Avalonia;
using System;
using LibVLCSharp.Shared;
using Babel.Services;
using System.Threading.Tasks;

namespace AudiobookPlayer;


sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Init Dependencies
        Core.Initialize();

        // Init Services
        IOService.Initialize();
        DataStoreService.Instance.Initialize();
        LibraryScanService.Instance.StartLibraryScan();

        // Start app main-loop
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
