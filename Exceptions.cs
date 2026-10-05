namespace CityLibrary;

// CONCEPT 9: Custom Exception
// LibraryException abstract hai -> isko direct "new" nahi kar sakte, sirf subclasses use hongi.
// Ye System.Exception se inherit karta hai.
public abstract class LibraryException : Exception
{
    protected LibraryException(string message) : base(message) { }
}

// Book already issued ho, ya issue na hui book return karni ho
public class BookNotAvailableException : LibraryException
{
    public BookNotAvailableException(string message) : base(message) { }
}

// Member ID exist nahi karta
public class MemberNotFoundException : LibraryException
{
    public MemberNotFoundException(string message) : base(message) { }
}

// Book ID exist nahi karta, ya search me kuch nahi mila
public class BookNotFoundException : LibraryException
{
    public BookNotFoundException(string message) : base(message) { }
}

// Empty name, price <= 0, ya corrupted data
public class InvalidInputException : LibraryException
{
    public InvalidInputException(string message) : base(message) { }
}