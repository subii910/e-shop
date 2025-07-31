using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace e_shop.Models;

public partial class Customerwebsite1Context : DbContext
{
    public Customerwebsite1Context()
    {
    }

    public Customerwebsite1Context(DbContextOptions<Customerwebsite1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=customerwebsite1;Integrated Security=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admins__719FE4E8A65A31EA");

            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Passwor).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B017C8021");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8A3586673");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6B209DCEB");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.CustomerFid).HasColumnName("CustomerFID");
            entity.Property(e => e.FeedbackDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductFid).HasColumnName("ProductFID");

            entity.HasOne(d => d.CustomerF).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CustomerFid)
                .HasConstraintName("FK__Feedbacks__Custo__32E0915F");

            entity.HasOne(d => d.ProductF).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ProductFid)
                .HasConstraintName("FK__Feedbacks__Produ__33D4B598");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFDBE1F820");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CustomerFid).HasColumnName("CustomerFID");
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.CustomerF).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerFid)
                .HasConstraintName("FK__Orders__Customer__36B12243");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C1706113F");

            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
            entity.Property(e => e.OrderFid).HasColumnName("OrderFID");
            entity.Property(e => e.ProductFid).HasColumnName("ProductFID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.OrderF).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderFid)
                .HasConstraintName("FK__OrderDeta__Order__34C8D9D1");

            entity.HasOne(d => d.ProductF).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductFid)
                .HasConstraintName("FK__OrderDeta__Produ__35BCFE0A");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED84E2BDD3");

            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.BrandFid).HasColumnName("BrandFID");
            entity.Property(e => e.CategoryFid).HasColumnName("CategoryFID");
            entity.Property(e => e.Pprice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PPrice");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.Srice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.CategoryF).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryFid)
                .HasConstraintName("FK__Products__Catego__38996AB5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
