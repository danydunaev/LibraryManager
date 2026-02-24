using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Commands;
using LibraryManager.Models;
using LibraryManager.Services;

namespace LibraryManager.ViewModels;

public class AuthorManagerViewModel : ViewModelBase
{
    private readonly LibraryRepository _repository;
    private Author? _selectedAuthor;
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _country = string.Empty;
    private DateTimeOffset _birthDate = new DateTimeOffset(DateTime.Today.AddYears(-30));
    private string _errorMessage = string.Empty;

    public AuthorManagerViewModel(LibraryRepository repository)
    {
        _repository = repository;
        AddAuthorCommand = new RelayCommand(_ => AddAuthor());
        UpdateAuthorCommand = new RelayCommand(_ => UpdateAuthor());
        DeleteAuthorCommand = new RelayCommand(_ => DeleteAuthor());
        LoadAuthors();
    }

    public ObservableCollection<Author> Authors { get; } = new();

    public ICommand AddAuthorCommand { get; }
    public ICommand UpdateAuthorCommand { get; }
    public ICommand DeleteAuthorCommand { get; }

    public Author? SelectedAuthor
    {
        get => _selectedAuthor;
        set
        {
            if (SetProperty(ref _selectedAuthor, value))
            {
                if (value is not null)
                {
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                    Country = value.Country;
                BirthDate = new DateTimeOffset(value.BirthDate.ToDateTime(new TimeOnly(0, 0)));
                }
                else
                {
                    ResetEntryFields();
                }
            }
        }
    }

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    public DateTimeOffset BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    private void LoadAuthors()
    {
        Authors.Clear();
        foreach (var author in _repository.GetAuthors())
        {
            Authors.Add(author);
        }
    }

    private void AddAuthor()
    {
        if (!ValidateAuthor())
        {
            return;
        }

        var author = new Author
        {
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Country = Country.Trim(),
            BirthDate = DateOnly.FromDateTime(BirthDate.DateTime)
        };

        _repository.AddAuthor(author);
        LoadAuthors();
        SelectedAuthor = null;
    }

    private void UpdateAuthor()
    {
        if (SelectedAuthor is null)
        {
            ErrorMessage = "Выберите автора для обновления.";
            return;
        }

        if (!ValidateAuthor())
        {
            return;
        }

        var author = new Author
        {
            Id = SelectedAuthor.Id,
            FirstName = FirstName.Trim(),
            LastName = LastName.Trim(),
            Country = Country.Trim(),
            BirthDate = DateOnly.FromDateTime(BirthDate.DateTime)
        };

        _repository.UpdateAuthor(author);
        LoadAuthors();
        SelectedAuthor = null;
    }

    private void DeleteAuthor()
    {
        if (SelectedAuthor == null)
        {
            ErrorMessage = "Выберите автора для удаления.";
            return;
        }

        _repository.DeleteAuthor(SelectedAuthor.Id);
        LoadAuthors();
        SelectedAuthor = null;
    }

    private bool ValidateAuthor()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "Имя автора обязательно.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "Фамилия автора обязательна.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Country))
        {
            ErrorMessage = "Укажите страну автора.";
            return false;
        }

        return true;
    }

    private void ResetEntryFields()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Country = string.Empty;
        BirthDate = new DateTimeOffset(DateTime.Today.AddYears(-30));
        ErrorMessage = string.Empty;
    }
}
