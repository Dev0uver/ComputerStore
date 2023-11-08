using System;
using System.Collections.Generic;

namespace ComputerStore.Models;

public partial class Subcategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
