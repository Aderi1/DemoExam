namespace DemoExam.Models;

public class UserShort
{
    public int Id { get; set; }

    public string FullName { get; set; } = "";

    public override string ToString()
    {
        return FullName;
    }
}