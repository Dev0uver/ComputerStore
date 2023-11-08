using System;
using System.Collections.Generic;

namespace ComputerStore.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderNumber { get; set; }

    public int ProductId { get; set; }

    public int Amount { get; set; }

    public virtual Order OrderNumberNavigation { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
