using Avalonia.Controls;
using LibraryManager.Services;
using LibraryManager.ViewModels;

namespace LibraryManager.Views;

public partial class GenreManagerWindow : Window
{
    public GenreManagerWindow()
        : this(new LibraryRepository())
    {
    }

    public GenreManagerWindow(LibraryRepository repository)
    {
        InitializeComponent();
        DataContext = new GenreManagerViewModel(repository);
    }
}
