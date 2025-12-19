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

	public BooksViewModel _booksViewModel;
	private object _currentView;

	public object CurrentView
	{
		get { return _currentView; }
		set { _currentView = value;
			RaisePropertyChanged();
		}
	}

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
            _ = _booksViewModel.LoadBooksForSelectedStore(SelectedStore.Id); // awaits but discards it
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
		
		Stores = new ObservableCollection<Store>(db.Stores.ToList());


		_booksViewModel = new BooksViewModel();

		SelectedStore = Stores.FirstOrDefault();
		CurrentView = _booksViewModel;
        
    }
}

