using Fuck.Models;
using System.Globalization;

namespace Fuck.Services
{
    public class LibraryService
    {
        private readonly string booksPath = "./Data/Books.csv";
        private readonly string usersPath = "./Data/Users.csv";

        protected List<Book> InternalBooks { get; set; } = new();
        protected List<User> InternalUsers { get; set; } = new();
        protected Dictionary<int, List<Book>> InternalBorrowedBooks { get; set; } = new();

        // Public read-only wrappers for UI and components
        public List<Book> Books => InternalBooks;
        public List<User> Users => InternalUsers;
        public Dictionary<int, List<Book>> BorrowedBooks => InternalBorrowedBooks;

        public LibraryService()
        {
            LoadData();
        }

        public void LoadData()
        {
            ReadBooks();
            ReadUsers();
        }

        // ========================
        // BOOKS
        // ========================

        public void ReadBooks()
        {
            Books.Clear();
            if (!File.Exists(booksPath)) return;

            foreach (var line in File.ReadLines(booksPath))
            {
                var fields = line.Split(',');

                if (fields.Length >= 4)
                {
                    Books.Add(new Book
                    {
                        Id = int.Parse(fields[0]),
                        Title = fields[1],
                        Author = fields[2],
                        ISBN = fields[3]
                    });
                }
            }
        }

        protected virtual void WriteBooks()
        {
            var lines = Books.Select(b => $"{b.Id},{b.Title},{b.Author},{b.ISBN}");
            File.WriteAllLines(booksPath, lines);
        }

        public void AddBook(Book book)
        {
            book.Id = Books.Any() ? Books.Max(b => b.Id) + 1 : 1;
            InternalBooks.Add(book);
            WriteBooks();
        }

        public void EditBook(Book updatedBook)
        {
            var book = Books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (book != null)
            {
                book.Title = updatedBook.Title;
                book.Author = updatedBook.Author;
                book.ISBN = updatedBook.ISBN;
                WriteBooks();
            }
        }

        public void DeleteBook(int id)
        {
            var book = Books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                Books.Remove(book);
                WriteBooks();
            }
        }

        // ========================
        // USERS
        // ========================

        public void ReadUsers()
        {
            Users.Clear();
            if (!File.Exists(usersPath)) return;

            foreach (var line in File.ReadLines(usersPath))
            {
                var fields = line.Split(',');

                if (fields.Length >= 3)
                {
                    Users.Add(new User
                    {
                        Id = int.Parse(fields[0]),
                        Name = fields[1],
                        Email = fields[2]
                    });
                }
            }
        }

        protected virtual void WriteUsers()
        {
            var lines = Users.Select(u => $"{u.Id},{u.Name},{u.Email}");
            File.WriteAllLines(usersPath, lines);
        }

        public void AddUser(User user)
        {
            user.Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1;
            InternalUsers.Add(user);
            WriteUsers();
        }

        public void EditUser(User updatedUser)
        {
            var user = Users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user != null)
            {
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                WriteUsers();
            }
        }

        public void DeleteUser(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                Users.Remove(user);
                BorrowedBooks.Remove(user.Id);
                WriteUsers();
            }
        }

        // ========================
        // BORROW/RETURN
        // ========================

        public bool BorrowBook(int bookId, int userId)
        {
            var book = Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null) return false;

            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            if (!BorrowedBooks.ContainsKey(userId))
                BorrowedBooks[userId] = new List<Book>();

            BorrowedBooks[userId].Add(book);
            Books.Remove(book);
            WriteBooks();
            return true;
        }

        public bool ReturnBook(int userId, int bookId)
        {
            if (!BorrowedBooks.ContainsKey(userId))
                return false;

            var book = BorrowedBooks[userId].FirstOrDefault(b => b.Id == bookId);
            if (book == null)
                return false;

            BorrowedBooks[userId].Remove(book);
            InternalBooks.Add(book);
            WriteBooks();
            return true;
        }


        public List<Book> GetBorrowedBooksByUser(int userId)
        {
            if (BorrowedBooks.ContainsKey(userId))
                return BorrowedBooks[userId];
            return new List<Book>();
        }
    }
}
