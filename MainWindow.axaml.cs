using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryManager.ViewModels; // этот using нужен для ViewModel

namespace LibraryManager; // Файл в корне, поэтому namespace LibraryManager

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Создаем ViewModel
        var vm = new MainWindowViewModel();
        vm.SetOwnerWindow(() => this);
        DataContext = vm;
    }

    private void ResetFilters_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SearchText = string.Empty;
            vm.SelectedAuthorFilter = null;
            vm.SelectedGenreFilter = null;
            vm.ResetFiltersManually();
        }
    }
}