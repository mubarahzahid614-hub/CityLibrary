namespace CityLibrary;


public abstract class LibraryException : Exception
{
    protected LibraryException(string message) : base(message) { }
}


public class BookNotAvailableException : LibraryException
{
    public BookNotAvailableException(string message) : base(message) { }
}


public class MemberNotFoundException : LibraryException
{
    public MemberNotFoundException(string message) : base(message) { }
}


public class BookNotFoundException : LibraryException
{
    public BookNotFoundException(string message) : base(message) { }
}


public class InvalidInputException : LibraryException
{
    public InvalidInputException(string message) : base(message) { }
}