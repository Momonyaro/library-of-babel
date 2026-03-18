using Avalonia;
using Avalonia.Controls;
using Babel.Enums;
using Babel.Utils;
using System;
using System.Windows.Input;

namespace AudiobookPlayer.Controls;

public partial class PillControl : UserControl
{
    public static readonly StyledProperty<MainSortingTab> SelectedTabProperty =
        AvaloniaProperty.Register<PillControl, MainSortingTab>(nameof(SelectedTab));

    public MainSortingTab SelectedTab
    {
        get => GetValue(SelectedTabProperty);
        set => SetValue(SelectedTabProperty, value);
    }

    public ICommand SetTabCommand { get; }

    public PillControl()
    {
        InitializeComponent();

        // Command sets SelectedTab directly
        SetTabCommand = new RelayCommand<string>(tab =>
        {
            if (Enum.TryParse<MainSortingTab>(tab, out var result))
            {
                SelectedTab = result;
            }
        });
    }
}