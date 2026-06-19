namespace DemoExam.Models;

public class ProductShort
{
    public string Article { get; set; } = "";

    public string Name { get; set; } = "";

    public override string ToString()
    {
        return $"{Article} - {Name}";
    }
}