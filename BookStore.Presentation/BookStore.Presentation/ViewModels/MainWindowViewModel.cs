using Bookstore.Infrastructure.Data.Model;
using CompanyDemo.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Presentation.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{

	private ObservableCollection<Book> _books;

	public ObservableCollection<Book> Books
	{
		get { return _books; }
		set { 
			_books = value;
			RaisePropertyChanged();
		}
	}

	public MainWindowViewModel()
    {

		using var db = new BookstoreDBContext();
		
		Books = new ObservableCollection<Book>(db.Books.ToList());
        
    }
}

