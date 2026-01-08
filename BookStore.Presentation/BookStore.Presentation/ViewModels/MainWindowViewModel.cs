using Bookstore.Infrastructure.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BookStore.Presentation.ViewModels;

internal class MainWindowViewModel : ViewModelBase
{

	public BooksViewModel _booksViewModel;
    public AuthorsViewModel _authorsViewModel;
	private object _currentView;

	public object CurrentView
	{
		get { return _currentView; }
		set { _currentView = value;
			RaisePropertyChanged();
		}
	}
	private bool _isBooksSelected = true;
    public bool IsBooksSelected
    {
        get => _isBooksSelected;
        set
        {
            _isBooksSelected = value;
            RaisePropertyChanged();
            if (value) CurrentView = _booksViewModel;
        }
    }

    private bool _isAuthorsSelected;
    public bool IsAuthorsSelected
    {
        get => _isAuthorsSelected;
        set
        {
            _isAuthorsSelected = value;
            if (value) CurrentView = _authorsViewModel;
            _ = _authorsViewModel.LoadAuthorDetailsAsync();
            RaisePropertyChanged();

        }
    }

    private bool _isCustomersSelected;
    public bool IsCustomersSelected
    {
        get => _isCustomersSelected;
        set
        {
            _isCustomersSelected = value;
            RaisePropertyChanged();
            if (value) 
            {
                // TODO: Create CustomersViewModel and set it
                // CurrentView = _customersViewModel;
            }
        }
    }

    private bool _isOrdersSelected;
    public bool IsOrdersSelected
    {
        get => _isOrdersSelected;
        set
        {
            _isOrdersSelected = value;
            RaisePropertyChanged();
            if (value) 
            {
                // TODO: Create OrdersViewModel and set it
                // CurrentView = _ordersViewModel;
            }
        }
    }

    private bool _isPublishersSelected;
    public bool IsPublishersSelected
    {
        get => _isPublishersSelected;
        set
        {
            _isPublishersSelected = value;
            RaisePropertyChanged();
            if (value) 
            {
                // TODO: Create PublishersViewModel and set it
                // CurrentView = _publishersViewModel;
            }
        }
    }

    private bool _isStoresSelected;
    public bool IsStoresSelected
    {
        get => _isStoresSelected;
        set
        {
            _isStoresSelected = value;
            RaisePropertyChanged();
            if (value) 
            {
                // TODO: Create StoresViewModel and set it
                // CurrentView = _storesViewModel;
            }
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            RaisePropertyChanged();
        }
    }
    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
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

        _ = InitializeAsync();

		using var db = new BookstoreDBContext();
		
		_booksViewModel = new BooksViewModel(this);
        _authorsViewModel = new AuthorsViewModel();

        // Books is selected by default
        IsBooksSelected = true;
        
    }
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Load stores
            await LoadStoresAsync();

            // Set default selection and load initial books
            if (Stores.Count > 0)
            {
                SelectedStore = Stores.First();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to initialize: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStoresAsync()
    {
        using var db = new BookstoreDBContext();
        var storesList = await db.Stores.ToListAsync();
        Stores = new ObservableCollection<Store>(storesList);
    }
    /*
    private async Task LoadAuthorsDataAsync()
    {
        try
        {
            await _authorsViewModel.LoadAuthorDetailsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load authors: {ex.Message}";
        }
    }
    */
}

