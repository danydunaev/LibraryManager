using System.Collections.Generic;
using System.Linq;

namespace LibraryManager.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublishYear { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }

    public int AuthorId { get; set; }
    public Author? Author { get; set; }

    // Связь многие-ко-многим с жанрами
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

    // Для обратной совместимости и удобства отображения
    public string GenresList => BookGenres != null && BookGenres.Any() 
        ? string.Join(", ", BookGenres.Select(bg => bg.Genre?.Name ?? "Unknown"))
        : "Нет жанров";
    
    // Вспомогательное свойство для получения ID жанров
    public IEnumerable<int> GenreIds => BookGenres?.Select(bg => bg.GenreId) ?? Enumerable.Empty<int>();
    
    public string AuthorName => Author?.FullName ?? string.Empty;
}