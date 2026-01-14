using System;
using System.Collections.Generic;
namespace Bookstore.Infrastructure.Data.Model;

public partial class Book
{
    public string Isbn13 { get; set; } = null!;

    public string Title { get; set; } = null!;

    public Language Language { get; set; } = Language.English;

    public decimal PriceInSek { get; set; }

    public DateOnly PublicationDate { get; set; }

    public int? PublisherId { get; set; }

    public virtual ICollection<InventoryBalance> InventoryBalances { get; set; } = new List<InventoryBalance>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
}

public enum Language
{
    Swedish,
    English,
    Finnish,
    Danish
}
