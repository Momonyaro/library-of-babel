using Avalonia.Controls;

namespace AudiobookPlayer.Views;

public partial class PlayerView : UserControl
{
    public PlayerView()
    {
        InitializeComponent();

        ChapterButton.Click += (s, e) =>
        {
            // Explicitly set the DataContext of the popup to the UserControl's DataContext (main VM)
            ChapterPopup.DataContext = this.DataContext;
            ChapterPopup.IsOpen = !ChapterPopup.IsOpen;
        };
    }
}