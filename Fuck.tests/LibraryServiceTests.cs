using Xunit;
using Fuck.Models;
using Fuck.Services;
using System.Collections.Generic;
using System.Linq;

public class LibraryServiceTests
{
    // Test subclass that disables file reading/writing
    private class TestLibraryService : LibraryService
    {
        public TestLibraryService()
        {
            InternalBooks = new List<Book>();
            InternalUsers = new List<User>();
            InternalBorrowedBooks = new Dictionary<int, List<Book>>();
        }

        protected override void WriteBooks() { /* Skip file I/O */ }
        protected override void WriteUsers() { /* Skip file I/O */ }
    }

    [Fact]
    public void AddBook_ShouldAssignIdAndAddToBooks()
    {
        var service = new TestLibraryService();
        var book = new Book { Title = "Sample", Author = "Author", ISBN = "1234" };

        service.AddBook(book);

        Assert.Single(service.Books);
        Assert.Equal(1, service.Books.First().Id);
    }

    [Fact]
    public void EditBook_ShouldUpdateBookFields()
    {
        var service = new TestLibraryService();
        var book = new Book { Title = "Old", Author = "Author", ISBN = "1234" };
        service.AddBook(book);

        var updated = new Book { Id = 1, Title = "New", Author = "New Author", ISBN = "5678" };
        service.EditBook(updated);

        var result = service.Books.First();
        Assert.Equal("New", result.Title);
        Assert.Equal("New Author", result.Author);
        Assert.Equal("5678", result.ISBN);
    }

    [Fact]
    public void DeleteBook_ShouldRemoveBookById()
    {
        var service = new TestLibraryService();
        service.AddBook(new Book { Title = "To Delete", Author = "X", ISBN = "111" });

        service.DeleteBook(1);

        Assert.Empty(service.Books);
    }

    [Fact]
    public void BorrowBook_ShouldRemoveFromBooksAndTrackInBorrowed()
    {
        var service = new TestLibraryService();
        var book = new Book { Title = "Borrow", Author = "Y", ISBN = "222" };
        var user = new User { Name = "User1", Email = "u1@test.com" };
        service.AddBook(book);
        service.AddUser(user);

        var success = service.BorrowBook(1, 1);

        Assert.True(success);
        Assert.Empty(service.Books);
        Assert.Single(service.GetBorrowedBooksByUser(1));
    }

    [Fact]
    public void ReturnBook_ShouldMoveBookBackToBooks()
    {
        var service = new TestLibraryService();
        var book = new Book { Title = "Return Me", Author = "Z", ISBN = "333" };
        var user = new User { Name = "Returner", Email = "ret@test.com" };
        service.AddBook(book);
        service.AddUser(user);
        service.BorrowBook(1, 1);

        var success = service.ReturnBook(1, 1);

        Assert.True(success);
        Assert.Single(service.Books);
        Assert.Empty(service.GetBorrowedBooksByUser(1));
    }

    [Fact]
    public void ReturnBook_ShouldFail_IfBookWasNotBorrowed()
    {
        var service = new TestLibraryService();
        var user = new User { Name = "NoBorrow", Email = "none@test.com" };
        service.AddUser(user);

        var success = service.ReturnBook(1, 99);

        Assert.False(success);
    }
}
