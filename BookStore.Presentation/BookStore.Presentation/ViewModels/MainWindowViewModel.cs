using Bookstore.Infrastructure.Data.Model;
using CompanyDemo.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

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

	private Store? _selectedStore;
    public Store? SelectedStore 
	{
		get => _selectedStore;

		set
		{
			_selectedStore = value;
			RaisePropertyChanged();
		}
	}

    private ObservableCollection<Store> _stores;

	public ObservableCollection<Store> Stores
	{
		get { return _stores; }
		set 
		{ 
			_stores = value;
			RaisePropertyChanged();
		}
	}


	public MainWindowViewModel()
    {

		using var db = new BookstoreDBContext();
		
		Books = new ObservableCollection<Book>(db.Books.ToList());
		Stores = new ObservableCollection<Store>(db.Stores.ToList());

		SelectedStore = Stores.FirstOrDefault();
        
    }
}

