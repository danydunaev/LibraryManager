using System.Collections.Generic;

namespace LibraryManager.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Изменяем связь с Book на BookGenre
    public ICollection<BookGenre> BookGenres { get; set; } = new List<BookGenre>();
}