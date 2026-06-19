namespace DemoExam.Models;

public class OrderStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public override string ToString()
    {
        return Name;
    }
}