using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class BooksWithCollaboratingAuthor
{
    public string Isbn13 { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Collaborators { get; set; }
}
