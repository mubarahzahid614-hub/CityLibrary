using System.Globalization;

namespace CityLibrary;

// Result of Load: kitni lines load hui aur kitni corrupted thi (skip hui)
public record LoadResult(bool FilesFound, int Books, int Members, int Records, int Skipped);

// Library = poore system ka "manager". Saari rules (business rules) yahin enforce hoti hain.
public class Library
{
    private const string BooksFile = "books.txt";
    private const string MembersFile = "members.txt";
    private const string IssuesFile = "issues.txt";
    private const string LogFile = "library.log";

    // CONCEPT 12: Collections -> List<T>
    private readonly List<Book> _books = new();
    private readonly List<Member> _members = new();
    private readonly List<IssueRecord> _records = new();

    public Librarian Librarian { get; } = new Librarian(1, "Head Librarian", "LIB-001");

    public IReadOnlyList<Book> Books => _books;
    public IReadOnlyList<Member> Members => _members;

    // ---------------------------------------------------------------- BOOKS

    public Book AddBook(string title, string author, decimal price)
    {
        try
        {
            int id = Book.NextId;
            if (_books.Any(b => b.BookId == id))   // Rule: Book ID must be unique
                throw new InvalidInputException($"Book ID {id} already exists.");

            // Book(id, title, author, price) -> setters validation chalate hain.
            // Agar yahan exception aayi to counter aage NAHI barhta (isliye ID skip nahi hoti).
            var book = new Book(id, title, author, price);
            _books.Add(book);
            Log($"Added Book #{book.BookId}: {book.Title}");
            return book;
        }
        catch (LibraryException ex)
        {
            Log($"FAILED Add Book: {ex.Message}");
            throw; // upar Program tak pohnchao taake user ko message dikhe
        }
    }

    // CONCEPT 7: Method Overloading -> same naam, alag parameters

    // Version 1: ID se search
    public Book SearchBook(int id)
    {
        var book = _books.FirstOrDefault(b => b.BookId == id);
        if (book == null)
        {
            Log($"FAILED Search: Book #{id} not found");
            throw new BookNotFoundException($"Book with ID {id} was not found.");
        }
        return book;
    }

    // Version 2: title se (partial match, case-insensitive)
    public List<Book> SearchBook(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidInputException("Search text cannot be empty.");

        var results = _books
            .Where(b => b.Title.Contains(title.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (results.Count == 0)
        {
            Log($"FAILED Search: no book matching '{title}'");
            throw new BookNotFoundException($"No book found matching '{title}'.");
        }
        return results;
    }

    // -------------------------------------------------------------- MEMBERS

    public Member AddMember(string name, string phone)
    {
        try
        {
            var member = new Member(name, phone);
            _members.Add(member);
            Log($"Added Member #{member.Id}: {member.Name}");
            return member;
        }
        catch (LibraryException ex)
        {
            Log($"FAILED Add Member: {ex.Message}");
            throw;
        }
    }

    public Member FindMember(int id)
    {
        var member = _members.FirstOrDefault(m => m.Id == id);
        if (member == null)
            throw new MemberNotFoundException($"Member with ID {id} was not found.");
        return member;
    }

    // ---------------------------------------------------------- ISSUE/RETURN

    public IssueRecord IssueBook(int bookId, int memberId)
    {
        try
        {
            var book = SearchBook(bookId);       // BookNotFoundException agar nahi mili
            var member = FindMember(memberId);   // MemberNotFoundException agar nahi mila

            if (book.IsIssued)                   // Rule: already issued book dobara issue nahi ho sakti
                throw new BookNotAvailableException($"Book '{book.Title}' is already issued.");

            var record = new IssueRecord(bookId, memberId);
            _records.Add(record);
            book.IsIssued = true;

            Log($"Issued Book#{bookId} to Member#{memberId}");
            return record;
        }
        catch (LibraryException ex)
        {
            Log($"FAILED Issue (Book#{bookId}, Member#{memberId}): {ex.Message}");
            throw;
        }
    }

    public IssueRecord ReturnBook(int bookId)
    {
        try
        {
            var book = SearchBook(bookId);

            if (!book.IsIssued)                  // Rule: jo issue hi nahi hui wo return nahi ho sakti
                throw new BookNotAvailableException($"Book '{book.Title}' was not issued, so it cannot be returned.");

            var record = _records.LastOrDefault(r => r.BookId == bookId && r.IsActive);
            if (record == null)
                throw new InvalidInputException($"No active issue record found for Book#{bookId} (data mismatch).");

            record.ReturnDate = DateTime.Now;
            book.IsIssued = false;

            Log($"Returned Book#{bookId} from Member#{record.MemberId}");
            return record;
        }
        catch (LibraryException ex)
        {
            Log($"FAILED Return (Book#{bookId}): {ex.Message}");
            throw;
        }
    }

    public List<IssueRecord> GetActiveIssues() => _records.Where(r => r.IsActive).ToList();

    // Ek issue record ko readable line me convert karta hai
    public string DescribeIssue(IssueRecord r)
    {
        string title = _books.FirstOrDefault(b => b.BookId == r.BookId)?.Title ?? "(unknown book)";
        string member = _members.FirstOrDefault(m => m.Id == r.MemberId)?.Name ?? "(unknown member)";
        return $"Record #{r.RecordId} | Book#{r.BookId} '{title}' | Member#{r.MemberId} {member} | Issued: {r.IssueDate:yyyy-MM-dd HH:mm}";
    }

    // ------------------------------------------------------------ FILE SAVE

    // CONCEPT 11: File Handling (StreamWriter) + CONCEPT 10: try/catch/finally
    public void SaveData()
    {
        try
        {
            WriteLines(BooksFile, _books.Select(b =>
                $"{b.BookId}|{Clean(b.Title)}|{Clean(b.Author)}|{b.Price.ToString("F2", CultureInfo.InvariantCulture)}|{b.IsIssued}"));

            WriteLines(MembersFile, _members.Select(m =>
                $"{m.Id}|{Clean(m.Name)}|{Clean(m.Phone)}"));

            WriteLines(IssuesFile, _records.Select(r =>
                $"{r.RecordId}|{r.BookId}|{r.MemberId}|{r.IssueDate.ToString("s", CultureInfo.InvariantCulture)}|" +
                $"{(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("s", CultureInfo.InvariantCulture) : "")}"));

            Log($"Saved data: {_books.Count} books, {_members.Count} members, {_records.Count} records");
        }
        catch (IOException ex)
        {
            Log($"FAILED Save: {ex.Message}");
            throw;
        }
    }

    private static void WriteLines(string path, IEnumerable<string> lines)
    {
        StreamWriter? writer = null;
        try
        {
            writer = new StreamWriter(path, false);
            foreach (var line in lines)
                writer.WriteLine(line);
        }
        finally
        {
            // finally hamesha chalta hai (error aaye ya na aaye) -> file hamesha band hogi
            writer?.Dispose();
        }
    }

    // '|' text me aa jaye to file format kharab ho jata hai, isliye '/' se replace
    private static string Clean(string text) => text.Replace("|", "/");

    // ------------------------------------------------------------ FILE LOAD

    // CONCEPT 11: File Handling (StreamReader)
    public LoadResult LoadData()
    {
        if (!File.Exists(BooksFile) && !File.Exists(MembersFile) && !File.Exists(IssuesFile))
        {
            Log("Load skipped: no data files found");
            return new LoadResult(false, 0, 0, 0, 0);
        }

        // Purana data saaf + counters reset, phir file se dobara bharo
        _books.Clear();
        _members.Clear();
        _records.Clear();
        Book.ResetCounter();
        Member.ResetCounter();
        IssueRecord.ResetCounter();

        int skipped = 0;

        // ---- books ----
        foreach (var line in ReadLines(BooksFile))
        {
            try
            {
                var p = line.Split('|');
                if (p.Length != 5) throw new InvalidInputException("Wrong number of fields.");

                if (!int.TryParse(p[0], out int id)) throw new InvalidInputException("Bad book ID.");
                if (!decimal.TryParse(p[3], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price))
                    throw new InvalidInputException("Bad price.");
                if (!bool.TryParse(p[4], out bool issued)) throw new InvalidInputException("Bad IsIssued value.");
                if (_books.Any(b => b.BookId == id)) throw new InvalidInputException("Duplicate book ID.");

                var book = new Book(id, p[1], p[2], price) { IsIssued = issued };
                _books.Add(book);
            }
            catch (InvalidInputException)
            {
                skipped++; // corrupted line -> skip, crash nahi
            }
        }

        // ---- members ----
        foreach (var line in ReadLines(MembersFile))
        {
            try
            {
                var p = line.Split('|');
                if (p.Length != 3) throw new InvalidInputException("Wrong number of fields.");
                if (!int.TryParse(p[0], out int id)) throw new InvalidInputException("Bad member ID.");
                if (_members.Any(m => m.Id == id)) throw new InvalidInputException("Duplicate member ID.");

                _members.Add(new Member(id, p[1], p[2]));
            }
            catch (InvalidInputException)
            {
                skipped++;
            }
        }

        // ---- issue records ----
        foreach (var line in ReadLines(IssuesFile))
        {
            try
            {
                var p = line.Split('|');
                if (p.Length != 5) throw new InvalidInputException("Wrong number of fields.");

                if (!int.TryParse(p[0], out int recordId)) throw new InvalidInputException("Bad record ID.");
                if (!int.TryParse(p[1], out int bookId)) throw new InvalidInputException("Bad book ID.");
                if (!int.TryParse(p[2], out int memberId)) throw new InvalidInputException("Bad member ID.");
                if (!DateTime.TryParseExact(p[3], "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime issueDate))
                    throw new InvalidInputException("Bad issue date.");

                DateTime? returnDate = null;
                if (!string.IsNullOrWhiteSpace(p[4]))
                {
                    if (!DateTime.TryParseExact(p[4], "s", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rd))
                        throw new InvalidInputException("Bad return date.");
                    returnDate = rd;
                }

                // Record tabhi valid hai jab uski book aur member maujood hon
                if (!_books.Any(b => b.BookId == bookId) || !_members.Any(m => m.Id == memberId))
                    throw new InvalidInputException("Record refers to a missing book or member.");
                if (_records.Any(r => r.RecordId == recordId))
                    throw new InvalidInputException("Duplicate record ID.");

                _records.Add(new IssueRecord(recordId, bookId, memberId, issueDate, returnDate));
            }
            catch (InvalidInputException)
            {
                skipped++;
            }
        }

        Log($"Loaded data: {_books.Count} books, {_members.Count} members, {_records.Count} records, {skipped} corrupted line(s) skipped");
        return new LoadResult(true, _books.Count, _members.Count, _records.Count, skipped);
    }

    private static List<string> ReadLines(string path)
    {
        var lines = new List<string>();
        if (!File.Exists(path)) return lines;

        StreamReader? reader = null;
        try
        {
            reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line)) // khali lines ignore
                    lines.Add(line);
            }
        }
        finally
        {
            reader?.Dispose();
        }
        return lines;
    }

    // -------------------------------------------------------------- LOGGING

    // Har operation library.log me append hota hai
    public void Log(string message)
    {
        try
        {
            File.AppendAllText(LogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch (IOException)
        {
            // Log likhne me masla aaye to program band nahi hona chahiye
        }
    }
}