namespace CityLibrary;


public abstract class Person
{

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
        Name = name; 
    }

    
    public abstract string DisplayInfo();
}


public class Member : Person
{
    
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

    
    public Member(string name, string phone) : this(_nextMemberId, name, phone) { }

    
    public Member(int id, string name, string phone) : base(id, name)
    {
        Phone = phone;

        if (id >= _nextMemberId) _nextMemberId = id + 1;
    }

    
    public static void ResetCounter() => _nextMemberId = 1;


    public override string DisplayInfo() =>
        $"Member #{Id} | Name: {Name} | Phone: {Phone}";
}


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