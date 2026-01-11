using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

internal class OrdersViewModel: ViewModelBase
{
	private DisplayOrderDetails _selectedOrder;

	public DisplayOrderDetails SelectedOrder
	{
		get => _selectedOrder; 
		set 
		{ 
			_selectedOrder = value;
			RaisePropertyChanged();
		}
	}


	private ObservableCollection<DisplayOrderDetails> _displayOrderDetails;

	public ObservableCollection<DisplayOrderDetails> DisplayOrderDetails
	{
		get => _displayOrderDetails;
		set 
		{ 
			_displayOrderDetails = value;
			RaisePropertyChanged();
		}
	}


}

public class DisplayOrderDetails : INotifyPropertyChanged
{
	private string _customerFullName;

	public string CustomerFullName
	{
		get => _customerFullName; 
		set 
		{
			CustomerFullName = value;
			OnPropertyChanged();
			
		}
	}
	private List<string> _booksInOrder;

	public List<string> BooksInOrder
	{
		get => _booksInOrder;
		set 
		{
			_booksInOrder = value;
			OnPropertyChanged();
		}
	}
    private DateOnly _orderDate;
	public DateOnly OrderDate
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
