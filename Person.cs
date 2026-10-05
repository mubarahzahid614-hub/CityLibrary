namespace CityLibrary;

// CONCEPT 5: Abstraction -> Person abstract class hai, direct object nahi ban sakta.
public abstract class Person
{
    // CONCEPT 2: Encapsulation -> private field + public property with validation
    private string _name = "";

    public int Id { get; protected set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidInputException("Name cannot be empty.");
            _name = value.Trim();
        }
    }

    protected Person(int id, string name)
    {
        Id = id;
        Name = name; // setter chalega, isliye validation yahan bhi lagegi
    }

    // CONCEPT 4: Polymorphism ke liye abstract method.
    // Har child class apna version (override) likhegi.
    public abstract string DisplayInfo();
}

// CONCEPT 3: Inheritance -> Member : Person
public class Member : Person
{
    // CONCEPT 8: Static member -> sab members ke liye ek shared counter
    private static int _nextMemberId = 1;

    private string _phone = "";

    public string Phone
    {
        get => _phone;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidInputException("Phone cannot be empty.");
            _phone = value.Trim();
        }
    }

    // Naya member: ID automatically milti hai
    public Member(string name, string phone) : this(_nextMemberId, name, phone) { }

    // File se load karte waqt: ID pehle se pata hoti hai
    public Member(int id, string name, string phone) : base(id, name)
    {
        Phone = phone;
        // Agar loaded ID bari hai to counter aage barha do (duplicate ID se bachne ke liye)
        if (id >= _nextMemberId) _nextMemberId = id + 1;
    }

    // Load Data se pehle counter reset karne ke liye
    public static void ResetCounter() => _nextMemberId = 1;

    // CONCEPT 4: Polymorphism -> override
    public override string DisplayInfo() =>
        $"Member #{Id} | Name: {Name} | Phone: {Phone}";
}

// CONCEPT 3 + 4: Librarian bhi Person se inherit karta hai, apna DisplayInfo() likhta hai
public class Librarian : Person
{
    private string _employeeCode = "";

    public string EmployeeCode
    {
        get => _employeeCode;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidInputException("Employee code cannot be empty.");
            _employeeCode = value.Trim();
        }
    }

    public Librarian(int id, string name, string employeeCode) : base(id, name)
    {
        EmployeeCode = employeeCode;
    }

    public override string DisplayInfo() =>
        $"Librarian #{Id} | Name: {Name} | Employee Code: {EmployeeCode}";
}