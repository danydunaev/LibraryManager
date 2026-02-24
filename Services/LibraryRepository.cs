using System;
using System.Collections.Generic;
using System.Linq;
using LibraryManager.Data;
using LibraryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManager.Services;

public class LibraryRepository
{
    public LibraryRepository()
    {
        using var context = CreateContext();
        context.Database.EnsureCreated();
        SeedIfEmpty(context);
    }

    private LibraryContext CreateContext() => new();

    private static void SeedIfEmpty(LibraryContext context)
    {
        if (context.Authors.Any()) return;

        var genre1 = new Genre { Name = "Роман", Description = "Художественная литература большого объёма" };
        var genre2 = new Genre { Name = "Фантастика", Description = "Научная и ненаучная фантастика" };
        var genre3 = new Genre { Name = "Детектив", Description = "Детективные истории" };
        context.Genres.AddRange(genre1, genre2, genre3);
        context.SaveChanges();

        var author1 = new Author { FirstName = "Лев", LastName = "Толстой", Country = "Россия", BirthDate = new DateOnly(1828, 9, 9) };
        var author2 = new Author { FirstName = "Фёдор", LastName = "Достоевский", Country = "Россия", BirthDate = new DateOnly(1821, 11, 11) };
        var author3 = new Author { FirstName = "Артур", LastName = "Конан Дойл", Country = "Великобритания", BirthDate = new DateOnly(1859, 5, 22) };
        context.Authors.AddRange(author1, author2, author3);
        context.SaveChanges();

        // Создаем книги без GenreId
        var book1 = new Book { Title = "Война и мир", ISBN = "978-5-17-019827-3", PublishYear = 1869, QuantityInStock = 5, AuthorId = author1.Id };
        var book2 = new Book { Title = "Анна Каренина", ISBN = "978-5-17-019828-0", PublishYear = 1878, QuantityInStock = 3, AuthorId = author1.Id };
        var book3 = new Book { Title = "Преступление и наказание", ISBN = "978-5-17-019829-7", PublishYear = 1866, QuantityInStock = 4, AuthorId = author2.Id };
        var book4 = new Book { Title = "Идиот", ISBN = "978-5-17-019830-3", PublishYear = 1869, QuantityInStock = 2, AuthorId = author2.Id };
        var book5 = new Book { Title = "Собака Баскервилей", ISBN = "978-5-17-019831-0", PublishYear = 1902, QuantityInStock = 7, AuthorId = author3.Id };
        
        context.Books.AddRange(book1, book2, book3, book4, book5);
        context.SaveChanges();

        // Добавляем связи с жанрами
        context.BookGenres.AddRange(
            new BookGenre { BookId = book1.Id, GenreId = genre1.Id }, // Война и мир - Роман
            new BookGenre { BookId = book2.Id, GenreId = genre1.Id }, // Анна Каренина - Роман
            new BookGenre { BookId = book3.Id, GenreId = genre1.Id }, // Преступление и наказание - Роман
            new BookGenre { BookId = book4.Id, GenreId = genre1.Id }, // Идиот - Роман
            new BookGenre { BookId = book5.Id, GenreId = genre3.Id }  // Собака Баскервилей - Детектив
        );
        
        context.SaveChanges();
    }

    public IReadOnlyList<Book> GetBooks(string? titleFilter = null, int? authorId = null, List<int>? genreIds = null)
    {
        using var context = CreateContext();
        var query = context.Books
            .Include(b => b.Author)
            .Include(b => b.BookGenres)
                .ThenInclude(bg => bg.Genre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(titleFilter))
        {
            var searchText = titleFilter.Trim();
            query = query.Where(book => EF.Functions.Like(book.Title, $"%{searchText}%"));
        }

        if (authorId.HasValue && authorId.Value > 0)
        {
            query = query.Where(book => book.AuthorId == authorId.Value);
        }

        if (genreIds != null && genreIds.Any())
        {
            query = query.Where(book => book.BookGenres.Any(bg => genreIds.Contains(bg.GenreId)));
        }

        return query.OrderBy(b => b.Title).ToList();
    }

    public IReadOnlyList<Author> GetAuthors()
    {
        using var context = CreateContext();
        return context.Authors.AsNoTracking()
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToList();
    }

    public IReadOnlyList<Genre> GetGenres()
    {
        using var context = CreateContext();
        return context.Genres.AsNoTracking()
            .OrderBy(g => g.Name)
            .ToList();
    }

    public Book AddBook(Book book, List<int> genreIds)
    {
        using var context = CreateContext();
        
        // Добавляем книгу
        context.Books.Add(book);
        context.SaveChanges();
        
        // Добавляем связи с жанрами
        foreach (var genreId in genreIds)
        {
            context.BookGenres.Add(new BookGenre
            {
                BookId = book.Id,
                GenreId = genreId
            });
        }
        
        context.SaveChanges();
        return book;
    }

    public void UpdateBook(Book book, List<int> genreIds)
    {
        using var context = CreateContext();
        
        var entity = context.Books
            .Include(b => b.BookGenres)
            .FirstOrDefault(b => b.Id == book.Id);
            
        if (entity == null) return;
        
        // Обновляем поля книги
        entity.Title = book.Title;
        entity.PublishYear = book.PublishYear;
        entity.ISBN = book.ISBN;
        entity.QuantityInStock = book.QuantityInStock;
        entity.AuthorId = book.AuthorId;
        
        // Удаляем старые связи с жанрами
        context.BookGenres.RemoveRange(entity.BookGenres);
        
        // Добавляем новые связи
        foreach (var genreId in genreIds)
        {
            context.BookGenres.Add(new BookGenre
            {
                BookId = book.Id,
                GenreId = genreId
            });
        }
        
        context.SaveChanges();
    }

    public void DeleteBook(int bookId)
    {
        using var context = CreateContext();
        var entity = context.Books.Find(bookId);
        if (entity == null)
        {
            return;
        }

        context.Books.Remove(entity);
        context.SaveChanges();
    }

    public Author AddAuthor(Author author)
    {
        using var context = CreateContext();
        context.Authors.Add(author);
        context.SaveChanges();
        return author;
    }

    public void UpdateAuthor(Author author)
    {
        using var context = CreateContext();
        var entity = context.Authors.Find(author.Id);
        if (entity == null)
        {
            return;
        }

        entity.FirstName = author.FirstName;
        entity.LastName = author.LastName;
        entity.Country = author.Country;
        entity.BirthDate = author.BirthDate;
        context.SaveChanges();
    }

    public void DeleteAuthor(int authorId)
    {
        using var context = CreateContext();
        var entity = context.Authors.Include(a => a.Books).FirstOrDefault(a => a.Id == authorId);
        if (entity == null)
        {
            return;
        }

        context.Authors.Remove(entity);
        context.SaveChanges();
    }

    public Genre AddGenre(Genre genre)
    {
        using var context = CreateContext();
        context.Genres.Add(genre);
        context.SaveChanges();
        return genre;
    }

    public void UpdateGenre(Genre genre)
    {
        using var context = CreateContext();
        var entity = context.Genres.Find(genre.Id);
        if (entity == null)
        {
            return;
        }

        entity.Name = genre.Name;
        entity.Description = genre.Description;
        context.SaveChanges();
    }

    public void DeleteGenre(int genreId)
    {
        using var context = CreateContext();
        var entity = context.Genres.Include(g => g.BookGenres).FirstOrDefault(g => g.Id == genreId);
        if (entity == null)
        {
            return;
        }

        context.Genres.Remove(entity);
        context.SaveChanges();
    }
}