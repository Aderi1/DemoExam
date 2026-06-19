namespace DemoExam.Models;

public class PickupPoint
{
    public int Id { get; set; }

    public string Address { get; set; } = "";

    public override string ToString()
    {
        return Address;
    }
}