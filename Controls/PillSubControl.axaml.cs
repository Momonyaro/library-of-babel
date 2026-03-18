using Avalonia;
using Avalonia.Controls;
using Babel.Enums;

namespace AudiobookPlayer.Controls;

public partial class PillSubControl : UserControl
{
    public static readonly StyledProperty<DetailsContentTab> SelectedTabProperty =
        AvaloniaProperty.Register<PillSubControl, DetailsContentTab>(nameof(SelectedTab));

    public DetailsContentTab SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public PillSubControl()
    {
        InitializeComponent();
    }
}