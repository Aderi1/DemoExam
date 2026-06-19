using System;

namespace DemoExam.Models;

public class Order
{
    public int OrderId { get; set; }

    public string ProductArticle { get; set; } = "";

    public DateTime OrderDate { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string Address { get; set; } = "";

    public string FullName { get; set; } = "";

    public int ReceiveCode { get; set; }

    public string Status { get; set; } = "";
}