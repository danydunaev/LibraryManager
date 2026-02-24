using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManager.Commands;
using LibraryManager.Models;
using LibraryManager.Services;

namespace LibraryManager.ViewModels;

public class GenreManagerViewModel : ViewModelBase
{
    private readonly LibraryRepository _repository;
    private Genre? _selectedGenre;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private string _errorMessage = string.Empty;

    public GenreManagerViewModel(LibraryRepository repository)
    {
        _repository = repository;
        AddGenreCommand = new RelayCommand(_ => AddGenre());
        UpdateGenreCommand = new RelayCommand(_ => UpdateGenre());
        DeleteGenreCommand = new RelayCommand(_ => DeleteGenre());
        LoadGenres();
    }

    public ObservableCollection<Genre> Genres { get; } = new();

    public ICommand AddGenreCommand { get; }
    public ICommand UpdateGenreCommand { get; }
    public ICommand DeleteGenreCommand { get; }

    public Genre? SelectedGenre
    {
        get => _selectedGenre;
        set
        {
            if (SetProperty(ref _selectedGenre, value))
            {
                if (value is not null)
                {
                    Name = value.Name;
                    Description = value.Description;
                }
                else
                {
                    ResetForm();
                }
            }
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    private void LoadGenres()
    {
        Genres.Clear();
        foreach (var genre in _repository.GetGenres())
        {
            Genres.Add(genre);
        }
    }

    private void AddGenre()
    {
        if (!Validate())
        {
            return;
        }

        var genre = new Genre
        {
            Name = Name.Trim(),
            Description = Description.Trim()
        };

        _repository.AddGenre(genre);
        LoadGenres();
        SelectedGenre = null;
    }

    private void UpdateGenre()
    {
        if (SelectedGenre is null)
        {
            ErrorMessage = "Выберите жанр для изменения.";
            return;
        }

        if (!Validate())
        {
            return;
        }

        var genre = new Genre
        {
            Id = SelectedGenre.Id,
            Name = Name.Trim(),
            Description = Description.Trim()
        };

        _repository.UpdateGenre(genre);
        LoadGenres();
        SelectedGenre = null;
    }

    private void DeleteGenre()
    {
        if (SelectedGenre is null)
        {
            ErrorMessage = "Выберите жанр для удаления.";
            return;
        }

        _repository.DeleteGenre(SelectedGenre.Id);
        LoadGenres();
        SelectedGenre = null;
    }

    private bool Validate()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Название жанра обязательно.";
            return false;
        }

        return true;
    }

    private void ResetForm()
    {
        Name = string.Empty;
        Description = string.Empty;
        ErrorMessage = string.Empty;
    }
}
