using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Price { get; set; }

    public int? CategoryId { get; set; }

    public int? SubcategoryId { get; set; }

    public bool Availability { get; set; }

    public int Amount { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Category? Category { get; set; } = null!;

    public virtual Subcategory? Subcategory { get; set; }

}