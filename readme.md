# LibraryManager

Приложение для управления библиотекой книг, разработанное на платформе Avalonia UI с использованием Entity Framework Core и SQLite. Поддерживает macOS, Windows и Linux.

## Функциональность

### Управление книгами
- 📚 Просмотр списка всех книг в библиотеке
- 🔍 Поиск книг по названию
- 🏷️ Фильтрация книг по автору и жанру
- ➕ Добавление новых книг
- ✏️ Редактирование информации о книгах
- 🗑️ Удаление книг
- 📊 Отображение количества экземпляров в наличии

### Управление авторами
- 👤 Добавление новых авторов
- ✏️ Редактирование информации об авторах
- 🗑️ Удаление авторов (с каскадным удалением связанных книг)

### Управление жанрами
- 🏷️ Добавление новых жанров
- ✏️ Редактирование жанров
- 🗑️ Удаление жанров (с проверкой на наличие связанных книг)

## Технологии

- **Avalonia UI** 11.2.0 — кроссплатформенный фреймворк для создания GUI
- **.NET 8.0** — платформа разработки
- **Entity Framework Core 8.0** — ORM для работы с базой данных
- **SQLite** — легковесная встраиваемая база данных
- **CommunityToolkit.Mvvm** — реализация паттерна MVVM
- **Avalonia.Controls.DataGrid** — компонент для отображения табличных данных

## Структура проекта
```
LibraryManager/
├── Models/ # Модели данных
│ ├── Book.cs # Книга
│ ├── Author.cs # Автор
│ └── Genre.cs # Жанр
| └── BookGenre.cs
├── Data/ # Работа с базой данных
│ └── AppDbContext.cs # Контекст Entity Framework
├── ViewModels/ # ViewModel'и
│ ├── MainWindowViewModel.cs
│ ├── BookEditViewModel.cs
│ ├── AuthorsViewModel.cs
│ └── GenresViewModel.cs
├── Views/ # Представления (XAML)
│ ├── MainWindow.axaml
│ ├── BookEditWindow.axaml
│ ├── AuthorsWindow.axaml
│ └── GenresWindow.axaml
├── App.axaml # Ресурсы и стили приложения
└── Program.cs # Точка входа
```
## Установка и запуск

### Требования
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/) (опционально)

### Клонирование и запуск

```
# Клонировать репозиторий
git clone https://github.com/yourusername/LibraryManager.git
cd LibraryManager

# Восстановить зависимости
dotnet restore

# Создать базу данных и применить миграции
dotnet ef database update

# Запустить приложение
dotnet run
```
