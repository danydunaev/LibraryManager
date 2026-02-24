using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using LibraryManager.Commands;
using LibraryManager.Models;
using LibraryManager.Services;
using LibraryManager.Views;
using System.Collections.Generic; // Добавлено

namespace LibraryManager.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly LibraryRepository _repository;
    private Func<Window>? _ownerWindow;
    private string _searchText = string.Empty;
    private Author? _selectedAuthorFilter;
    private Genre? _selectedGenreFilter;
    private Book? _selectedBook;
    private Book? _lastSelectedBook;

    private readonly AsyncRelayCommand _editBookCommand;
    private readonly AsyncRelayCommand _deleteBookCommand;
    private readonly AsyncRelayCommand _resetFiltersCommand;

    public ObservableCollection<Book> Books { get; } = new();
    public ObservableCollection<Author> Authors { get; } = new();
    public ObservableCollection<Genre> Genres { get; } = new();

    public MainWindowViewModel()
    {
        _repository = new LibraryRepository();
        
        AddBookCommand = new AsyncRelayCommand(_ => OpenAddBookDialogAsync());
        _editBookCommand = new AsyncRelayCommand(_ => OpenEditBookDialogAsync(), _ => SelectedBook != null);
        _deleteBookCommand = new AsyncRelayCommand(_ => DeleteBookAsync(), _ => SelectedBook != null);
        ManageAuthorsCommand = new AsyncRelayCommand(_ => OpenAuthorManagerAsync());
        ManageGenresCommand = new AsyncRelayCommand(_ => OpenGenreManagerAsync());
        _resetFiltersCommand = new AsyncRelayCommand(_ => ResetFiltersAsync());

        RefreshFilters();
        RefreshBooks();
    }

    public void SetOwnerWindow(Func<Window> ownerFactory) => _ownerWindow = ownerFactory;

    public ICommand AddBookCommand { get; }
    public ICommand EditBookCommand => _editBookCommand;
    public ICommand DeleteBookCommand => _deleteBookCommand;
    public ICommand ManageAuthorsCommand { get; }
    public ICommand ManageGenresCommand { get; }
    public ICommand ResetFiltersCommand => _resetFiltersCommand;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshBooks();
            }
        }
    }

    public Author? SelectedAuthorFilter
    {
        get => _selectedAuthorFilter;
        set
        {
            if (SetProperty(ref _selectedAuthorFilter, value))
            {
                RefreshBooks();
            }
        }
    }

    public Genre? SelectedGenreFilter
    {
        get => _selectedGenreFilter;
        set
        {
            if (SetProperty(ref _selectedGenreFilter, value))
            {
                RefreshBooks();
            }
        }
    }

    public Book? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (SetProperty(ref _selectedBook, value))
            {
                _editBookCommand.RaiseCanExecuteChanged();
                _deleteBookCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AvailabilitySummary => $"Всего книг в наличии: {Books.Sum(b => b.QuantityInStock)}";

    public void ResetFiltersManually()
    {
        Console.WriteLine("Ручной сброс фильтров...");
        
        SearchText = string.Empty;
        SelectedAuthorFilter = null;
        SelectedGenreFilter = null;
        
        ForceRefreshWithSelection();
        
        Console.WriteLine("Фильтры сброшены вручную");
    }

    private void ForceRefreshWithSelection()
    {
        _lastSelectedBook = SelectedBook;
        RefreshBooks();
        
        if (_lastSelectedBook != null)
        {
            SelectedBook = Books.FirstOrDefault(b => b.Id == _lastSelectedBook.Id);
        }
    }

    private async Task ResetFiltersAsync()
    {
        Console.WriteLine("Сброс фильтров через команду...");
        
        SearchText = string.Empty;
        SelectedAuthorFilter = null;
        SelectedGenreFilter = null;
        
        await Task.Run(() => ForceRefreshWithSelection());
        
        Console.WriteLine("Фильтры сброшены через команду");
        await Task.CompletedTask;
    }

    private async Task OpenAddBookDialogAsync()
    {
        var owner = _ownerWindow?.Invoke();
        if (owner == null) return;

        var dialogViewModel = new BookDialogViewModel(_repository, null);
        var dialog = new BookDialog
        {
            DataContext = dialogViewModel,
            Title = "Добавить книгу",
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var dialogResult = await dialog.ShowDialog<bool>(owner);
        if (dialogResult)
        {
            RefreshFilters();
            RefreshBooks();
        }
    }

    private async Task OpenEditBookDialogAsync()
    {
        if (SelectedBook == null) return;
        
        var owner = _ownerWindow?.Invoke();
        if (owner == null) return;

        var dialogViewModel = new BookDialogViewModel(_repository, SelectedBook);
        var dialog = new BookDialog
        {
            DataContext = dialogViewModel,
            Title = "Редактировать книгу",
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var dialogResult = await dialog.ShowDialog<bool>(owner);
        if (dialogResult)
        {
            RefreshFilters();
            RefreshBooks();
        }
    }

    private async Task DeleteBookAsync()
    {
        if (SelectedBook == null) return;
        
        _repository.DeleteBook(SelectedBook.Id);
        RefreshBooks();
        await Task.CompletedTask;
    }

    private async Task OpenAuthorManagerAsync()
    {
        var owner = _ownerWindow?.Invoke();
        if (owner == null) return;
        
        var window = new AuthorManagerWindow(_repository);
        await window.ShowDialog(owner);
        
        RefreshFilters();
        RefreshBooks();
    }

    private async Task OpenGenreManagerAsync()
    {
        var owner = _ownerWindow?.Invoke();
        if (owner == null) return;
        
        var window = new GenreManagerWindow(_repository);
        await window.ShowDialog(owner);
        
        RefreshFilters();
        RefreshBooks();
    }

    private void RefreshFilters()
    {
        Authors.Clear();
        foreach (var author in _repository.GetAuthors())
        {
            Authors.Add(author);
        }

        Genres.Clear();
        foreach (var genre in _repository.GetGenres())
        {
            Genres.Add(genre);
        }
    }

    private void RefreshBooks()
    {
        // Сохраняем ID выбранной книги
        int? selectedId = SelectedBook?.Id;
        
        Books.Clear();
        
        // ИСПРАВЛЕНИЕ: преобразуем одиночный жанр в список
        List<int>? genreIds = null;
        if (SelectedGenreFilter != null)
        {
            genreIds = new List<int> { SelectedGenreFilter.Id };
        }
        
        var books = _repository.GetBooks(SearchText, SelectedAuthorFilter?.Id, genreIds);
        foreach (var book in books)
        {
            Books.Add(book);
        }
        
        // Восстанавливаем выделение
        if (selectedId.HasValue)
        {
            SelectedBook = Books.FirstOrDefault(b => b.Id == selectedId.Value);
        }
        
        RaisePropertyChanged(nameof(AvailabilitySummary));
    }
}
