using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;
using NavigationService = BookStore.Presentation.Services.NavigationService;

namespace BookStore.Presentation.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private INavigationService _navigationService;

    public BooksInventoryViewModel _booksInventoryViewModel;
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

    public AsyncDelegateCommand SwitchToBooksViewCommand { get; set; }
    public AsyncDelegateCommand SwitchToAuthorsViewCommand { get; set; }
    public AsyncDelegateCommand SwitchToCustomersViewCommand { get; set; }
    public AsyncDelegateCommand SwitchToOrdersViewCommand { get; set; }
    public AsyncDelegateCommand SwitchToPublishersViewCommand { get; set; }
    public AsyncDelegateCommand SwitchToStoresViewCommand { get; set; }

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
            RaisePropertyChanged();
        }
    }
    private async Task SwitchToBooksAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
            CurrentView = _booksViewModel;
            await _booksViewModel.LoadStoresAsync();
        }
    }

    private async Task SwitchToAuthorsAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
            CurrentView = _authorsViewModel;
            await _authorsViewModel.LoadAuthorDetailsAsync();
        }
    }

    private async Task SwitchToCustomersAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
            CurrentView = _customersViewModel;
            await _customersViewModel.LoadAllCustomersAsync();
        }
    }

    private async Task SwitchToOrdersAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
            CurrentView = _ordersViewModel;
            await _ordersViewModel.LoadOrdersAsync();
        }
    }

    private async Task SwitchToPublishersAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
             CurrentView = _publisherViewModel;
            await _publisherViewModel.LoadPublishersAsync();
        }
    }

    private async Task SwitchToStoresAsync(object? sender)
    {
        if (await ConfirmSwitchAsync())
        {
            CurrentView = _storesViewModel;
            await _storesViewModel.LoadStoresAsync();
        }
    }
    private bool _isAuthorsSelected;
    public bool IsAuthorsSelected
    {
        get => _isAuthorsSelected;
        set
        {
            _isAuthorsSelected = value;
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
    public bool HasChangesCheck()
    {
        bool hasChanges = false;

        if (_booksViewModel?.HasChanges == true)
            hasChanges = true;

        if (_booksInventoryViewModel?.HasChanges == true)
            hasChanges = true;

        if (_customersViewModel?.HasChanges == true)
            hasChanges = true;
        
        if (_authorsViewModel?.HasChanges == true)
            hasChanges = true;
        
        if (_publisherViewModel?.HasChanges == true)
            hasChanges = true;
        
        if (_ordersViewModel?.HasChanges == true)
            hasChanges = true;

        return hasChanges;
    }

    public void ResetAllViewStates()
    {
        if (_booksViewModel != null)
        {
            _booksViewModel.ClearState();
        }
        if (_authorsViewModel != null)
        {
            _authorsViewModel.ClearState();
        }
        if (_customersViewModel != null)
        {
            _customersViewModel.ClearState();
        }
        if (_booksInventoryViewModel != null)
        {
           _booksInventoryViewModel.ClearState();
        }
    }
    
    private async Task<bool> ConfirmSwitchAsync()
    {
        bool hasChanges = HasChangesCheck();
        if (hasChanges)
        {
            bool proceed = await _dialogService.ShowConfirmationDialogAsync(
                "There are unsaved changes, do you still want to proceed?", 
                "Proceed without saving?");
            if (proceed)
            {
                ResetAllViewStates();
                return true;
            }
            return false;
        }
        return true;
    }
	public MainWindowViewModel()
    {

		using var db = new BookstoreDBContext();
        _dialogService = new DialogService(this);
        _navigationService = new NavigationService(this, _dialogService);
		
		_booksViewModel = new BooksViewModel(_navigationService, _dialogService);
        _authorsViewModel = new AuthorsViewModel(_dialogService);
        _customersViewModel = new CustomersViewModel(_dialogService);
        _ordersViewModel = new OrdersViewModel(_dialogService);
        _publisherViewModel = new PublishersViewModel(_dialogService);
        _storesViewModel = new StoresViewModel(_dialogService);
        
        SwitchToBooksViewCommand = new AsyncDelegateCommand(SwitchToBooksAsync);
        SwitchToAuthorsViewCommand = new AsyncDelegateCommand(SwitchToAuthorsAsync);
        SwitchToCustomersViewCommand = new AsyncDelegateCommand(SwitchToCustomersAsync);
        SwitchToOrdersViewCommand = new AsyncDelegateCommand(SwitchToOrdersAsync);
        SwitchToPublishersViewCommand = new AsyncDelegateCommand(SwitchToPublishersAsync);
        SwitchToStoresViewCommand = new AsyncDelegateCommand(SwitchToStoresAsync);

        // Books is selected by default
        CurrentView = _booksViewModel;
        IsBooksSelected = true;
        _ = _booksViewModel.LoadStoresAsync(); 
    }
}

