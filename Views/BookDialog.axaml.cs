using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManager.ViewModels;

namespace LibraryManager.Views;

public partial class BookDialog : Window
{
    public BookDialog()
    {
        InitializeComponent();
    }

    private void SaveOnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is BookDialogViewModel vm && vm.TrySave())
        {
            Close(true);
        }
    }

    private void CancelOnClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
