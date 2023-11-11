using System;
using System.Collections.Generic;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Data;

public partial class ComputerStoreContext : DbContext
{
    public ComputerStoreContext()
    {
    }

    public ComputerStoreContext(DbContextOptions<ComputerStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; } = default!;

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Product> Products { get; set; } = default!;

    public virtual DbSet<Subcategory> Subcategories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=ComputerStore;Username=postgres;Password=root");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cart_pkey");

            entity.ToTable("cart");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("userId");

            entity.HasOne(d => d.Product).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productIdKey");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userIdKey");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("order_pkey");

            entity.ToTable("order");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeliveryDate).HasColumnName("deliveryDate");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(15)
                .HasColumnName("orderStatus");
            entity.Property(e => e.OnlinePayment)
                .HasMaxLength(15)
                .HasColumnName("onlinePayment");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(15)
                .HasColumnName("paymentStatus");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasColumnName("userId");
            entity.Property(e => e.CreationDate).HasColumnName("creationDate");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("userIdKey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("orderItems_pkey");

            entity.ToTable("orderItems");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('items_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.OrderNumber).HasColumnName("orderNumber");
            entity.Property(e => e.ProductId).HasColumnName("productId");

            entity.HasOne(d => d.OrderNumberNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orderIdKey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("productIdKey");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("product_pkey");

            entity.ToTable("product");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Availability).HasColumnName("availability");
            entity.Property(e => e.CategoryId)
                .HasColumnName("categoryId");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.SubcategoryId).HasColumnName("subcategoryId");
            entity.Property(e => e.IsOnSale).HasColumnName("isOnSale");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("categoryIdKey");
            entity.HasOne(d => d.Subcategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.SubcategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subcategoryIdKey");
        });

        modelBuilder.Entity<Subcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subcategory_pkey");

            entity.ToTable("subcategory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.ToTable("user");

            entity.Property(e => e.Id)
                .HasMaxLength(450)
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.UserName)
                .HasMaxLength(256)
                .HasColumnName("userName");
            entity.Property(e => e.RoleId)
                .HasColumnName("roleId");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("roleIdKey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
        entity.HasKey(e => e.Id).HasName("role_pkey");

        entity.ToTable("role");

        entity.Property(e => e.Id).HasColumnName("id");
        entity.Property(e => e.RoleName)
            .HasMaxLength(50)
            .HasColumnName("roleName");
        });

        modelBuilder.HasSequence("cart_id_seq");
        modelBuilder.HasSequence("category_id_seq");
        modelBuilder.HasSequence("items_id_seq");
        modelBuilder.HasSequence("order_id_seq");
        modelBuilder.HasSequence("subcategory_id_seq");

        
        modelBuilder.HasSequence("cart_id_seq");
        modelBuilder.HasSequence("category_id_seq");
        modelBuilder.HasSequence("items_id_seq");
        modelBuilder.HasSequence("order_id_seq");
        modelBuilder.HasSequence("subcategory_id_seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
