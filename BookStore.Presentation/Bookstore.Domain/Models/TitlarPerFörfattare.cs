using System;
using System.Collections.Generic;

namespace Bookstore.Infrastructure.Data.Model;

public partial class TitlarPerFörfattare
{
    public string Namn { get; set; } = null!;

    public string Ålder { get; set; } = null!;

    public string Titlar { get; set; } = null!;

    public string LagerSaldo { get; set; } = null!;
}
