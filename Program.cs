using System.Globalization;
using System.Text;

namespace CityLibrary;

public class Program
{
    private static readonly Library library = new Library();

    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8; // ✅ ⚠️ jaise symbols sahi dikhane ke liye
        Console.WriteLine(library.Librarian.DisplayInfo()); // Librarian ka DisplayInfo (polymorphism)
        library.Log("Application started");

        bool running = true;

        // CONCEPT 13: Loops & Conditions -> menu tab tak chalta hai jab tak user 0 na dabaye
        while (running)
        {
            ShowMenu();
            int choice = ReadInt("Enter choice: ");

            // CONCEPT 10: try-catch-finally har operation ke liye
            try
            {
                switch (choice)
                {
                    case 1: AddBook(); break;
                    case 2: ViewAllBooks(); break;
                    case 3: SearchBook(); break;
                    case 4: AddMember(); break;
                    case 5: ViewAllMembers(); break;
                    case 6: IssueBook(); break;
                    case 7: ReturnBook(); break;
                    case 8: ViewIssuedBooks(); break;
                    case 9: SaveData(); break;
                    case 10: LoadData(); break;
                    case 0:
                        running = false;
                        Console.WriteLine("Goodbye! (Unsaved data save nahi hua, agar zaroorat ho to pehle option 9 use karein.)");
                        break;
                    default:
                        Console.WriteLine("⚠️  Invalid choice. Please select from the menu.");
                        break;
                }
            }
            catch (LibraryException ex)   // hamari custom exceptions (BookNotFound, InvalidInput, ...)
            {
                Console.WriteLine($"⚠️  {ex.Message}");
            }
            catch (IOException ex)        // file read/write ke masle
            {
                Console.WriteLine($"❌ File error: {ex.Message}");
                library.Log($"File error: {ex.Message}");
            }
            catch (Exception ex)          // koi unexpected error: crash nahi, message dikhao
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
                library.Log($"Unexpected error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine(); // chahe success ho ya error, har operation ke baad khali line
            }
        }

        library.Log("Application closed");
    }

    // ---------------------------------------------------------------- MENU

    private static void ShowMenu()
    {
        Console.WriteLine("======= CITY LIBRARY =======");
        Console.WriteLine("1.  Add Book");
        Console.WriteLine("2.  View All Books");
        Console.WriteLine("3.  Search Book");
        Console.WriteLine("4.  Add Member");
        Console.WriteLine("5.  View All Members");
        Console.WriteLine("6.  Issue Book");
        Console.WriteLine("7.  Return Book");
        Console.WriteLine("8.  View Issued Books");
        Console.WriteLine("9.  Save Data");
        Console.WriteLine("10. Load Data");
        Console.WriteLine("0.  Exit");
        Console.WriteLine("=============================");
    }

    // ------------------------------------------------------------- ACTIONS

    private static void AddBook()
    {
        string title = ReadText("Title: ");
        string author = ReadText("Author: ");
        decimal price = ReadDecimal("Price: ");

        var book = library.AddBook(title, author, price);
        Console.WriteLine($"✅ Book added with ID {book.BookId}");
    }

    private static void ViewAllBooks()
    {
        if (library.Books.Count == 0)
        {
            Console.WriteLine("No books in the library yet.");
            return;
        }
        foreach (var book in library.Books)
            Console.WriteLine(book);
    }

    private static void SearchBook()
    {
        Console.WriteLine("Search by: 1) ID  2) Title");
        int mode = ReadInt("Choice: ");

        if (mode == 1)
        {
            int id = ReadInt("Book ID: ");
            Console.WriteLine(library.SearchBook(id));            // SearchBook(int)
        }
        else if (mode == 2)
        {
            string title = ReadText("Title (or part of it): ");
            foreach (var book in library.SearchBook(title))       // SearchBook(string)
                Console.WriteLine(book);
        }
        else
        {
            Console.WriteLine("⚠️  Invalid search option.");
        }
    }

    private static void AddMember()
    {
        string name = ReadText("Name: ");
        string phone = ReadText("Phone: ");

        var member = library.AddMember(name, phone);
        Console.WriteLine($"✅ Member added with ID {member.Id}");
    }

    private static void ViewAllMembers()
    {
        // CONCEPT 4: Polymorphism -> List<Person> me Librarian + Members dono hain.
        // Har object par DisplayInfo() call hota hai, aur har class apna version chalati hai.
        var people = new List<Person>();
        people.Add(library.Librarian);
        people.AddRange(library.Members);

        foreach (Person p in people)
            Console.WriteLine(p.DisplayInfo());

        if (library.Members.Count == 0)
            Console.WriteLine("(No members added yet.)");
    }

    private static void IssueBook()
    {
        int bookId = ReadInt("Book ID: ");
        int memberId = ReadInt("Member ID: ");

        library.IssueBook(bookId, memberId);

        var book = library.SearchBook(bookId);
        var member = library.FindMember(memberId);
        Console.WriteLine($"✅ Book '{book.Title}' issued to {member.Name}.");
    }

    private static void ReturnBook()
    {
        int bookId = ReadInt("Book ID: ");

        library.ReturnBook(bookId);

        var book = library.SearchBook(bookId);
        Console.WriteLine($"✅ Book '{book.Title}' returned. It is available again.");
    }

    private static void ViewIssuedBooks()
    {
        var issues = library.GetActiveIssues();
        if (issues.Count == 0)
        {
            Console.WriteLine("No books are currently issued.");
            return;
        }
        foreach (var record in issues)
            Console.WriteLine(library.DescribeIssue(record));
    }

    private static void SaveData()
    {
        library.SaveData();
        Console.WriteLine("✅ Data saved to books.txt, members.txt and issues.txt");
    }

    private static void LoadData()
    {
        var result = library.LoadData();
        if (!result.FilesFound)
        {
            Console.WriteLine("⚠️  No saved data files found. Pehle Save Data (9) karein.");
            return;
        }
        Console.WriteLine($"✅ Loaded {result.Books} book(s), {result.Members} member(s), {result.Records} record(s).");
        if (result.Skipped > 0)
            Console.WriteLine($"⚠️  {result.Skipped} corrupted line(s) skipped.");
    }

    // ------------------------------------------------------- INPUT HELPERS

    // Console.ReadLine() null deta hai jab input khatam ho jaye (Ctrl+Z / piped input)
    private static string ReadRaw(string prompt)
    {
        Console.Write(prompt);
        string? input = Console.ReadLine();
        if (input == null)
        {
            Console.WriteLine();
            library.Log("Application closed (input ended)");
            Environment.Exit(0);
        }
        return input;
    }

    // Text: jaisa user likhe waisa wapas (empty ka faisla Library/setter karta hai -> InvalidInputException)
    private static string ReadText(string prompt) => ReadRaw(prompt);

    // Number prompt: "abc" likhne par dobara poochta hai, crash nahi hota (Test 17)
    private static int ReadInt(string prompt)
    {
        while (true)
        {
            string input = ReadRaw(prompt);
            if (int.TryParse(input.Trim(), out int value))
                return value;
            Console.WriteLine("⚠️  Please enter a valid whole number.");
        }
    }

    private static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            string input = ReadRaw(prompt);
            if (decimal.TryParse(input.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
                return value; // 0 ya negative bhi return hoga; "price > 0" ka rule Book.Price setter check karta hai
            Console.WriteLine("⚠️  Please enter a valid number.");
        }
    }
}