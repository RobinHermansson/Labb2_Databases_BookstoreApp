using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookstore.Infrastructure.Data.Model;

public partial class BookstoreDBContext : DbContext
{
    public BookstoreDBContext()
    {
    }

    public BookstoreDBContext(DbContextOptions<BookstoreDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookSalesIntelligence> BookSalesIntelligences { get; set; }

    public virtual DbSet<BookTitlesPerAuthor> BookTitlesPerAuthors { get; set; }

    public virtual DbSet<BooksWithCollaboratingAuthor> BooksWithCollaboratingAuthors { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerValueAnalysis> CustomerValueAnalyses { get; set; }

    public virtual DbSet<InventoryBalance> InventoryBalances { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<TitlarPerFörfattare> TitlarPerFörfattares { get; set; }

    public virtual DbSet<TotalSalesPerPublisher> TotalSalesPerPublishers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddUserSecrets<BookstoreDBContext>().Build();
        var connectionString = config["ConnectionString"];
        optionsBuilder.UseSqlServer(connectionString)
            .LogTo(message => DatabaseLogger(message),
                new[] { DbLoggerCategory.Database.Name },
                LogLevel.Information,
                DbContextLoggerOptions.Level | DbContextLoggerOptions.LocalTime
            )
            .EnableSensitiveDataLogging();
    }

    private void DatabaseLogger(string message)
    {
        Console.WriteLine(message);
        Debug.WriteLine(message);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Authors__3214EC078AB4A5A7");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(75);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Isbn13).HasName("PK__Books__3BF79E03B8F1065A");

            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("ISBN13");
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.PriceInSek)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("PriceInSEK");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .HasConstraintName("FK_Books_Publishers");

            entity.HasMany(d => d.Authors).WithMany(p => p.BookIsbn13s)
                .UsingEntity<Dictionary<string, object>>(
                    "BookAuthorship",
                    r => r.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BookAuthorships_Authors"),
                    l => l.HasOne<Book>().WithMany()
                        .HasForeignKey("BookIsbn13")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_BookAuthorships_Books"),
                    j =>
                    {
                        j.HasKey("BookIsbn13", "AuthorId");
                        j.ToTable("BookAuthorships");
                        j.IndexerProperty<string>("BookIsbn13")
                            .HasMaxLength(13)
                            .IsFixedLength()
                            .HasColumnName("BookISBN13");
                    });
        });

        modelBuilder.Entity<BookSalesIntelligence>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BookSalesIntelligence");

            entity.Property(e => e.AuthorName).HasMaxLength(126);
            entity.Property(e => e.AverageSellingPrice).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("ISBN13");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<BookTitlesPerAuthor>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BookTitlesPerAuthor");

            entity.Property(e => e.Age)
                .HasMaxLength(18)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(126);
            entity.Property(e => e.InventoryValue)
                .HasMaxLength(44)
                .IsUnicode(false);
            entity.Property(e => e.Titles)
                .HasMaxLength(16)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BooksWithCollaboratingAuthor>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("BooksWithCollaboratingAuthors");

            entity.Property(e => e.Collaborators).HasMaxLength(4000);
            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("ISBN13");
            entity.Property(e => e.Title).HasMaxLength(150);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07B79CF827");

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D1053455ABBD0C").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<CustomerValueAnalysis>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CustomerValueAnalysis");

            entity.Property(e => e.AverageOrderValue).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.CustomerName).HasMaxLength(101);
            entity.Property(e => e.TotalSpent).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<InventoryBalance>(entity =>
        {
            entity.HasKey(e => new { e.StoreId, e.Isbn13 }).HasName("PK__Inventor__183D88E13B838F08");

            entity.ToTable("InventoryBalance");

            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("ISBN13");

            entity.HasOne(d => d.Isbn13Navigation).WithMany(p => p.InventoryBalances)
                .HasForeignKey(d => d.Isbn13)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBalance_Books");

            entity.HasOne(d => d.Store).WithMany(p => p.InventoryBalances)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InventoryBalance_Stores");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07611FA273");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Customers");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Stores");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderIte__3214EC0761183024");

            entity.Property(e => e.Isbn13)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("ISBN13");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Isbn13Navigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.Isbn13)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Books");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Orders");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Publishe__3214EC07C4C18C5A");

            entity.HasIndex(e => e.Email, "UQ__Publishe__A9D1053450F3080A").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stores__3214EC073A60F430");

            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsFixedLength();
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(50);
            entity.Property(e => e.StoreName).HasMaxLength(250);
            entity.Property(e => e.WebpageUrl).HasMaxLength(100);
        });

        modelBuilder.Entity<TitlarPerFörfattare>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TitlarPerFörfattare");

            entity.Property(e => e.LagerSaldo)
                .HasMaxLength(44)
                .IsUnicode(false);
            entity.Property(e => e.Namn).HasMaxLength(126);
            entity.Property(e => e.Titlar)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Ålder)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TotalSalesPerPublisher>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TotalSalesPerPublisher");

            entity.Property(e => e.AverageOrderValue).HasColumnType("decimal(38, 6)");
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.PublisherName).HasMaxLength(150);
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(38, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
