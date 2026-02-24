using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LibraryManager.Models;
using LibraryManager.Services;
using LibraryManager.Data; // Добавь эту строку

namespace LibraryManager.ViewModels;

public class BookDialogViewModel : ViewModelBase
{
    private readonly LibraryRepository _repository;
    private readonly bool _isEdit;
    private readonly int? _editingBookId;
    private string _errorMessage = string.Empty;

    public BookDialogViewModel(LibraryRepository repository, Book? editingBook)
    {
        _repository = repository;
        Authors = repository.GetAuthors();
        
        var allGenres = repository.GetGenres();
        Genres = new ObservableCollection<Genre>(allGenres);
        SelectedGenres = new ObservableCollection<Genre>();
        
        PublishYear = (decimal?)DateTime.Now.Year;
        Quantity = 1m;

        if (editingBook is not null)
        {
            _isEdit = true;
            _editingBookId = editingBook.Id;
            Title = editingBook.Title;
            ISBN = editingBook.ISBN;
            PublishYear = (decimal?)editingBook.PublishYear;
            Quantity = (decimal?)editingBook.QuantityInStock;
            SelectedAuthor = Authors.FirstOrDefault(a => a.Id == editingBook.AuthorId);
            
            // Загружаем выбранные жанры для редактируемой книги
            using var context = new Data.LibraryContext();
            var genreIds = context.BookGenres
                .Where(bg => bg.BookId == editingBook.Id)
                .Select(bg => bg.GenreId)
                .ToList();
                
            foreach (var genre in Genres.Where(g => genreIds.Contains(g.Id)))
            {
                SelectedGenres.Add(genre);
            }
        }
    }

    public IReadOnlyList<Author> Authors { get; }
    public ObservableCollection<Genre> Genres { get; }
    public ObservableCollection<Genre> SelectedGenres { get; }

    public string DialogTitle => _isEdit ? "Редактировать книгу" : "Новая книга";

    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public decimal? PublishYear { get; set; }
    public decimal? Quantity { get; set; }
    public Author? SelectedAuthor { get; set; }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool TrySave()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Название книги обязательно.";
            return false;
        }

        if (SelectedAuthor is null)
        {
            ErrorMessage = "Выберите автора.";
            return false;
        }

        if (SelectedGenres.Count == 0)
        {
            ErrorMessage = "Выберите хотя бы один жанр.";
            return false;
        }

        var year = (int)(PublishYear ?? (decimal)DateTime.Now.Year);
        var qty = (int)(Quantity ?? 0m);

        if (year < 1450 || year > DateTime.Now.Year + 1)
        {
            ErrorMessage = "Укажите корректный год публикации.";
            return false;
        }

        if (qty < 0)
        {
            ErrorMessage = "Количество на складе не может быть отрицательным.";
            return false;
        }

        var book = new Book
        {
            Id = _editingBookId ?? 0,
            Title = Title.Trim(),
            ISBN = ISBN?.Trim() ?? string.Empty,
            PublishYear = year,
            QuantityInStock = qty,
            AuthorId = SelectedAuthor.Id
        };

        var genreIds = SelectedGenres.Select(g => g.Id).ToList();

        try
        {
            if (_isEdit)
                _repository.UpdateBook(book, genreIds);
            else
                _repository.AddBook(book, genreIds);

            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка при сохранении: {ex.Message}";
            return false;
        }
    }
}