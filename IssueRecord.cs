namespace CityLibrary;

public class IssueRecord
{
   
    private static int _nextRecordId = 1;

    public int RecordId { get; private set; }
    public int BookId { get; private set; }
    public int MemberId { get; private set; }
    public DateTime IssueDate { get; private set; }


    public DateTime? ReturnDate { get; set; }

   
    public IssueRecord(int bookId, int memberId)
    {
        RecordId = _nextRecordId++;
        BookId = bookId;
        MemberId = memberId;
        IssueDate = DateTime.Now;
        ReturnDate = null;
    }

    
    public IssueRecord(int recordId, int bookId, int memberId, DateTime issueDate, DateTime? returnDate)
    {
        if (recordId <= 0 || bookId <= 0 || memberId <= 0)
            throw new InvalidInputException("Issue record IDs must be positive.");

        RecordId = recordId;
        BookId = bookId;
        MemberId = memberId;
        IssueDate = issueDate;
        ReturnDate = returnDate;

        if (recordId >= _nextRecordId) _nextRecordId = recordId + 1;
    }

    public static void ResetCounter() => _nextRecordId = 1;

    public bool IsActive => ReturnDate == null; // abhi tak return nahi hui
}