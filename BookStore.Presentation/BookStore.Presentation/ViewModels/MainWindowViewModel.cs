using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Navigation;
using NavigationService = BookStore.Presentation.Services.NavigationService;

namespace BookStore.Presentation.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private INavigationService _navigationService;

	public BooksViewModel _booksViewModel;
    public AuthorsViewModel _authorsViewModel;
    public CustomersViewModel _customersViewModel;
    public OrdersViewModel _ordersViewModel;
    public PublishersViewModel _publisherViewModel;
    public StoresViewModel _storesViewModel;
	private object _currentView;

    private readonly IDialogService _dialogService;
    private bool _isDialogOpen;
    private UserControl _dialogContent;

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set
        {
            _isDialogOpen = value;
            RaisePropertyChanged();
        }
    }

    public UserControl DialogContent
    {
        get => _dialogContent;
        set
        {
            _dialogContent = value;
            RaisePropertyChanged();
        }
    }

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
            if (value) CurrentView = _booksViewModel;
            _ = _booksViewModel.LoadStoresAsync();
            RaisePropertyChanged();
            //_booksViewModel?.RaisePropertyChanged("Stores");
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
            _ = _customersViewModel.LoadAllCustomersAsync();
            if (value) CurrentView = _customersViewModel;
            RaisePropertyChanged();
            
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
                CurrentView = _ordersViewModel;
                _ = _ordersViewModel.LoadOrdersAsync();
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
            if (value) CurrentView = _publisherViewModel;
            _ = _publisherViewModel.LoadPublishersAsync();
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
            if (value) CurrentView = _storesViewModel;
            _ = _storesViewModel.LoadStoresAsync();
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

	public MainWindowViewModel()
    {

		using var db = new BookstoreDBContext();
        _dialogService = new DialogService(this);
        _navigationService = new NavigationService(this, _dialogService);
		
		_booksViewModel = new BooksViewModel(_navigationService);
        _authorsViewModel = new AuthorsViewModel();
        _customersViewModel = new CustomersViewModel();
        _ordersViewModel = new OrdersViewModel(this);
        _publisherViewModel = new PublishersViewModel();
        _storesViewModel = new StoresViewModel();

        // Books is selected by default
        IsBooksSelected = true;
        
    }
}

