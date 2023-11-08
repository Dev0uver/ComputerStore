using System;
using System.Collections.Generic;

namespace ComputerStore.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public bool OnlinePayment { get; set; }

    public int Total { get; set; }

    public string UserId { get; set; } = null!;
    
    public DateOnly CreationDate { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
