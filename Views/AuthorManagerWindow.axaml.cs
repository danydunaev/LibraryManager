using Avalonia.Controls;
using LibraryManager.Services;
using LibraryManager.ViewModels;

namespace LibraryManager.Views;

public partial class AuthorManagerWindow : Window
{
    public AuthorManagerWindow()
        : this(new LibraryRepository())
    {
    }

    public AuthorManagerWindow(LibraryRepository repository)
    {
        InitializeComponent();
        DataContext = new AuthorManagerViewModel(repository);
    }
}
