using System;
using System.Collections.Generic;

namespace ComputerStore.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int Amount { get; set; }

    public string UserId { get; set; } = null!;

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
