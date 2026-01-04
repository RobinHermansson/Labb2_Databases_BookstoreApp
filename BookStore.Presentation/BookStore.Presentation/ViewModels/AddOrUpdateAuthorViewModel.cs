using Bookstore.Infrastructure.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Presentation.ViewModels;

internal class AddOrUpdateAuthorViewModel : ViewModelBase
{
    private Author _affectedAuthor;

    public Author AffectedAuthor
    {
        get { return _affectedAuthor; }
        set { _affectedAuthor = value;
            RaisePropertyChanged();
        }
    }

    public AddOrUpdateAuthorViewModel(Author authorToAddOrUpdate)
    {
        AffectedAuthor = authorToAddOrUpdate;
    }
}
