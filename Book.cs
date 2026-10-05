namespace CityLibrary;

public class Book
{
    // CONCEPT 8: Static member -> auto-increment ID
    private static int _nextBookId = 1;

    // CONCEPT 2: Encapsulation -> private fields
    private int _bookId;
    private string _title = "";
    private string _author = "";
    private decimal _price;

    public int BookId
    {
        get => _bookId;
        private set => _bookId = value;
    }

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidInputException("Book title cannot be empty.");
            _title = value.Trim();
        }
    }

    public string Author
    {
        get => _author;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidInputException("Book author cannot be empty.");
            _author = value.Trim();
        }
    }

    public decimal Price
    {
        get => _price;
        set
        {
            if (value <= 0)
                throw new InvalidInputException("Book price must be greater than 0.");
            _price = value;
        }
    }

    public bool IsIssued { get; set; }

    // CONCEPT 6: Constructor Overloading
    // Version 1: khali constructor. ID automatically milti hai, baqi fields baad me set hongi.
    public Book()
    {
        BookId = _nextBookId++;
    }

    // Version 2: sab values ke saath. Loading ke waqt ya direct creation me use hota hai.
    public Book(int id, string title, string author, decimal price)
    {
        if (id <= 0)
            throw new InvalidInputException("Book ID must be positive.");

        BookId = id;
        Title = title;     // setters validation chalayenge
        Author = author;
        Price = price;

        // Counter ko hamesha max ID se aage rakho, taake Load ke baad duplicate ID na bane (Test 19)
        if (id >= _nextBookId) _nextBookId = id + 1;
    }

    // Sirf padhne ke liye: agla ID kya hoga (ID "kharch" kiye baghair)
    public static int NextId => _nextBookId;

    public static void ResetCounter() => _nextBookId = 1;

    public override string ToString() =>
        $"[{BookId}] {Title} by {Author} | Price: {Price:F2} | {(IsIssued ? "Issued" : "Available")}";
}