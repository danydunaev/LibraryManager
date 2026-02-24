using System;
using System.IO;
using LibraryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManager.Data;

public class LibraryContext : DbContext
{
    private static readonly string AppDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LibraryManager");
    private static readonly string DatabaseFile = Path.Combine(AppDirectory, "library.db");

    public static string DatabasePath => DatabaseFile;
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<BookGenre> BookGenres => Set<BookGenre>(); // Новая таблица

    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            Directory.CreateDirectory(AppDirectory);
            optionsBuilder.UseSqlite($"Data Source={DatabaseFile}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка Author
        modelBuilder.Entity<Author>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Country).IsRequired().HasMaxLength(100);
            builder.Property(a => a.BirthDate).IsRequired();
        });

        // Настройка Genre
        modelBuilder.Entity<Genre>(builder =>
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
            builder.Property(g => g.Description).HasMaxLength(500);
            
            // Убираем старую связь с Books
        });

        // Настройка Book
        modelBuilder.Entity<Book>(builder =>
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
            builder.Property(b => b.ISBN).HasMaxLength(20);
            builder.Property(b => b.PublishYear).IsRequired();
            builder.Property(b => b.QuantityInStock).IsRequired().HasDefaultValue(0);

            // Связь с Author
            builder.HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Убираем старую связь с Genre
        });

        // Настройка BookGenre (связь многие-ко-многим)
        modelBuilder.Entity<BookGenre>(builder =>
        {
            // Составной первичный ключ
            builder.HasKey(bg => new { bg.BookId, bg.GenreId });

            // Связь с Book
            builder.HasOne(bg => bg.Book)
                .WithMany(b => b.BookGenres)
                .HasForeignKey(bg => bg.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Связь с Genre
            builder.HasOne(bg => bg.Genre)
                .WithMany(g => g.BookGenres)
                .HasForeignKey(bg => bg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}