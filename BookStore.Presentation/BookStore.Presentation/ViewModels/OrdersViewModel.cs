using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

public class OrdersViewModel: ViewModelBase
{
	private readonly IDialogService _dialogService;
	
	private OrderDetails _selectedOrder;
	public AsyncDelegateCommand SaveChangesCommand { get; set; }
	public AsyncDelegateCommand CancelChangesCommand { get; set; }
	private List<OrderDetails> _deletedOrders = new List<OrderDetails>();
	private List<OrderDetails> _changedOrders = new List<OrderDetails>();
	private List<OrderDetails> OriginalListOfOrderDetails = new List<OrderDetails>();


	private bool _hasChanges = false;

	public bool HasChanges
	{
		get => _hasChanges;
		set 
		{ 
			_hasChanges = value;
			RaisePropertyChanged();
			SaveChangesCommand?.RaiseCanExecuteChanged();
			CancelChangesCommand?.RaiseCanExecuteChanged();
		}
	}
		
	public OrdersViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
		SaveChangesCommand = new AsyncDelegateCommand(SaveChangesAsync, CanSaveChanges);
		CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
    }

	private async Task SaveChangesAsync(object? sender)
	{
		using var db = new BookstoreDBContext();
		Debug.WriteLine("Not in use, as we currently do not permit altering the Orders.");
	}
	private bool CanSaveChanges(object? sender)
	{
		return HasChanges;
	}

	private async Task CancelChanges(object? sender)
	{
		try
		{
			await LoadOrdersAsync();
		}
		catch(Exception ex)
		{
			Debug.WriteLine($"Error when cancelling changes and awaiting the LoadOrdersAsync call. {ex.Message}");
		}
		_deletedOrders.Clear();
		_changedOrders.Clear();
		HasChanges = false;
	}
	private bool CanCancelChanges(object? sender)
	{
		return HasChanges;
	}
    public OrderDetails SelectedOrder
	{
		get => _selectedOrder; 
		set 
		{ 
			_selectedOrder = value;
			RaisePropertyChanged();
		}
	}


	private ObservableCollection<OrderDetails> _displayOrderDetails;

	public ObservableCollection<OrderDetails> DisplayOrderDetails
	{
		get => _displayOrderDetails;
		set 
		{ 
			// Unsubscribe from old collection
            if (_displayOrderDetails != null)
            {
                _displayOrderDetails.CollectionChanged -= DisplayOrderDetails_CollectionChanged;
                foreach (var order in _displayOrderDetails)
                {
                    order.PropertyChanged -= OrderDetails_PropertyChanged;
                }
            }

            _displayOrderDetails = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_displayOrderDetails != null)
            {
                _displayOrderDetails.CollectionChanged += DisplayOrderDetails_CollectionChanged;
                foreach (var order in _displayOrderDetails)
                {
                    order.PropertyChanged += OrderDetails_PropertyChanged;
                }
            }

		}
	}
	private void DisplayOrderDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (OrderDetails newOrder in e.NewItems)
            {
                newOrder.PropertyChanged += OrderDetails_PropertyChanged;
                
            }
        }

        if (e.OldItems != null)
        {
            foreach (OrderDetails removedOrder in e.OldItems)
            {
                removedOrder.PropertyChanged -= OrderDetails_PropertyChanged;
                
                if (removedOrder.OrderId > 0)
                {
                    _deletedOrders.Add(removedOrder);
                    Debug.WriteLine($"Order marked for deletion: {removedOrder.OrderId}");
                }
            }
        }

        CheckForChanges();
    }
    private void OrderDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (
			e.PropertyName == nameof(OrderDetails.CustomerFirstName) ||
			e.PropertyName == nameof(OrderDetails.CustomerLastName) ||
            e.PropertyName == nameof(OrderDetails.StoreName) ||
            e.PropertyName == nameof(OrderDetails.OrderDate))
        {
            CheckForChanges();
        }
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_deletedOrders.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var displayOrder in DisplayOrderDetails)
        {
            var originalOrder = OriginalListOfOrderDetails.FirstOrDefault(o => o.OrderId == displayOrder.OrderId);
            if (originalOrder != null)
            {
                if (
					originalOrder.CustomerFirstName != displayOrder.CustomerFirstName ||
					originalOrder.CustomerLastName != displayOrder.CustomerLastName ||
                    originalOrder.BooksInOrder != displayOrder.BooksInOrder ||
                    originalOrder.OrderDate != displayOrder.OrderDate ||
                    originalOrder.StoreName != displayOrder.StoreName)
                {
					_changedOrders.Add(displayOrder);
                    hasAnyChanges = true;
                    break;
                }
            }
        }

        HasChanges = hasAnyChanges;
    }

	public async Task LoadOrdersAsync()
	{
		try
		{
            using var db = new BookstoreDBContext();

            var tempOrderDetails = await db.Orders
                .Where(o => o.Customer != null && o.Store != null)
                .Select(o => new OrderDetails()
                {
                    CustomerId = o.CustomerId, 
                    CustomerFirstName = o.Customer.FirstName,
                    CustomerLastName = o.Customer.LastName,
                    BooksInOrder = string.Join(", ", o.OrderItems.Select(o => o.Isbn13)),
                    OrderDate = o.OrderDate ?? new DateTime(),
                    StoreName = o.Store.StoreName,
                    TotalOrderPrice = o.OrderItems.Sum(oi => oi.UnitPrice)
                }).ToListAsync();

            OriginalListOfOrderDetails = tempOrderDetails;
            DisplayOrderDetails = new ObservableCollection<OrderDetails>(tempOrderDetails);
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"Error when loading orders: {ex.Message}");
		}
	}
}

public class OrderDetails : INotifyPropertyChanged
{
	private int _orderId;
	public int OrderId
	{
		get => _orderId;
		set
		{
			_orderId = value;
			OnPropertyChanged();
		}
	}
	public int CustomerId { get; set; }

	private string _customerFirstName;

	public string CustomerFirstName
	{
		get => _customerFirstName; 
		set 
		{
			_customerFirstName = value;
			OnPropertyChanged();
			
		}
	}
	private string _customerLastName;

	public string CustomerLastName
	{
		get => _customerLastName; 
		set 
		{
			_customerLastName = value;
			OnPropertyChanged();
			
		}
	}
	private string _booksInOrder;

	public string BooksInOrder
	{
		get => _booksInOrder;
		set 
		{
			_booksInOrder = value;
			OnPropertyChanged();
		}
	}
    private DateTime _orderDate;
	public DateTime OrderDate
	{

        get => _orderDate;
        set
		{
			_orderDate = value;
			OnPropertyChanged();
		}
	}
	private string _storeName;

	public string StoreName
	{
		get => _storeName; 
		set 
		{ 
			_storeName = value;
			OnPropertyChanged();
		}
	}
	private decimal _totalOrderPrice;

	public decimal TotalOrderPrice
	{
		get => _totalOrderPrice;
		set 
		{
			_totalOrderPrice = value;
		}
	}
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	}
}
