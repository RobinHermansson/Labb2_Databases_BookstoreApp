using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

public class StoresViewModel: ViewModelBase
{
    private readonly IDialogService _dialogService; 
	public AsyncDelegateCommand SaveChangesCommand { get; set; }
	public AsyncDelegateCommand CancelChangesCommand { get; set; }
	private List<StoreDetails> _deletedStores = new List<StoreDetails>();
    private List<StoreDetails> _newStores = new List<StoreDetails>();
	private List<StoreDetails> _changedStores = new List<StoreDetails>();
	private List<Store> OriginalListOfStores = new List<Store>();


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
		
	public StoresViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
		SaveChangesCommand = new AsyncDelegateCommand(SaveChangesAsync, CanSaveChanges);
		CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
    }

	private async Task SaveChangesAsync(object? sender)
	{
		try
        {

            using var db = new BookstoreDBContext();

            // Handle new authors
            foreach (var newStore in _newStores)
            {
                var dbStore = new Store
                {
                    StoreName = newStore.StoreName,
                    Address = newStore.Address,
                    City = newStore.City,
                    Country = newStore.Country,
                    PostalCode = newStore.PostalCode,
                    WebpageUrl = newStore.WebpageUrl,
                    PhoneNumber = newStore.PhoneNumber
                };
                
                db.Stores.Add(dbStore);
            }

            // Handle deleted authors
            foreach (var deletedStore in _deletedStores)
            {
                var dbStore = await db.Stores
                .FirstOrDefaultAsync(a => a.Id == deletedStore.StoreId);
                if (dbStore != null)
                {
                    db.Stores.Remove(dbStore);
                }
            }
            
            //Handling modified existing authors
            var changedStores = new List<StoreDetails>();
            foreach (var displayStore in DisplayStoreDetails)
            {
                var originalStore = OriginalListOfStores.FirstOrDefault(a => a.Id == displayStore.StoreId);
                if (originalStore != null)
                {
                    if (originalStore.StoreName != displayStore.StoreName ||
                        originalStore.Address != displayStore.Address ||
                        originalStore.Country != displayStore.Country ||
                        originalStore.WebpageUrl != displayStore.WebpageUrl ||
                        originalStore.PhoneNumber != displayStore.PhoneNumber ||
                        originalStore.PostalCode != displayStore.PostalCode ||
                        originalStore.City != displayStore.City)
                    {
                        changedStores.Add(displayStore);
                    }
                }
            }
            foreach (var changedStore in changedStores)
            {
                var dbStore = await db.Stores.FindAsync(changedStore.StoreId);

                if (dbStore != null)
                {
                    dbStore.StoreName = changedStore.StoreName;
                    dbStore.Address = changedStore.Address;
                    dbStore.Country = changedStore.Country;
                    dbStore.City = changedStore.City;
                    dbStore.WebpageUrl = changedStore.WebpageUrl;
                    dbStore.PhoneNumber = changedStore.PhoneNumber;
                    dbStore.PostalCode = changedStore.PostalCode;
                }
            }

            await db.SaveChangesAsync();

            // Refresh states and the list according to new changes.
            HasChanges = false;
            try
            {
                await LoadStoresAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when loading stores after saving: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving changes {ex}");
        }
    }


	private bool CanSaveChanges(object? sender)
	{
		return HasChanges;
	}

	private async Task CancelChanges(object? sender)
	{
        try
        {
            await LoadStoresAsync();
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error when loading Stores after cancelling. {ex.Message}");
        }
		_deletedStores.Clear();
		_changedStores.Clear();
		HasChanges = false;
	}
	private bool CanCancelChanges(object? sender)
	{
		return HasChanges;
	}

	private ObservableCollection<StoreDetails> _displayStoreDetails;

	public ObservableCollection<StoreDetails> DisplayStoreDetails
	{
		get => _displayStoreDetails;
		set 
		{ 
			// Unsubscribe from old collection
            if (_displayStoreDetails != null)
            {
                _displayStoreDetails.CollectionChanged -= DisplayStoreDetails_CollectionChanged;
                foreach (var store in _displayStoreDetails)
                {
                    store.PropertyChanged -= StoreDetails_PropertyChanged;
                }
            }

            _displayStoreDetails = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_displayStoreDetails != null)
            {
                _displayStoreDetails.CollectionChanged += DisplayStoreDetails_CollectionChanged;
                foreach (var store in _displayStoreDetails)
                {
                    store.PropertyChanged += StoreDetails_PropertyChanged;
                }
            }

		}
	}
	private void DisplayStoreDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (StoreDetails newStore in e.NewItems)
            {
                newStore.PropertyChanged += StoreDetails_PropertyChanged;
                _newStores.Add(newStore);
                
            }
        }

        if (e.OldItems != null)
        {
            foreach (StoreDetails removedStore in e.OldItems)
            {
                removedStore.PropertyChanged -= StoreDetails_PropertyChanged;
                if (_newStores.Contains(removedStore))
                {
                    _newStores.Remove(removedStore);
                } 
                else if (removedStore.StoreId > 0)
                {
                    _deletedStores.Add(removedStore);
                    Debug.WriteLine($"Store marked for deletion: {removedStore.StoreId}");
                }
            }
        }

        CheckForChanges();
    }
    private void StoreDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (
            e.PropertyName == nameof(StoreDetails.StoreName) ||
            e.PropertyName == nameof(StoreDetails.Address) ||
            e.PropertyName == nameof(StoreDetails.City) ||
            e.PropertyName == nameof(StoreDetails.Country) ||
            e.PropertyName == nameof(StoreDetails.PostalCode) ||
            e.PropertyName == nameof(StoreDetails.WebpageUrl) ||
            e.PropertyName == nameof(StoreDetails.PhoneNumber))
        {
            CheckForChanges();
        }
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_deletedStores.Any())
        {
            hasAnyChanges = true;
        }
        if (_newStores.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var displayStore in DisplayStoreDetails)
        {
            var originalStore = OriginalListOfStores.FirstOrDefault(o => o.Id == displayStore.StoreId);
            if (originalStore != null)
            {
                if (
					originalStore.StoreName != displayStore.StoreName ||
					originalStore.City != displayStore.City ||
                    originalStore.Country != displayStore.Country ||
                    originalStore.Address != displayStore.Address ||
                    originalStore.PostalCode != displayStore.PostalCode ||
                    originalStore.WebpageUrl != displayStore.WebpageUrl ||
                    originalStore.PhoneNumber != displayStore.PhoneNumber)
                {
					_changedStores.Add(displayStore);
                    hasAnyChanges = true;
                    break;
                }
            }
        }

		Debug.WriteLine($"Any changes? : {hasAnyChanges}");
        HasChanges = hasAnyChanges;
    }

	public async Task LoadStoresAsync()
	{
        try
        {
            using var db = new BookstoreDBContext();

            var tempStoreDetails = await db.Stores
                .Select(o => new StoreDetails()
                {
                    StoreId = o.Id, 
                    StoreName = o.StoreName,
                    City = o.City,
                    Country = o.Country,
                    Address = o.Address,
                    WebpageUrl = o.WebpageUrl,
                    PostalCode = o.PostalCode,
                    PhoneNumber = o.PhoneNumber
                }).ToListAsync();

            OriginalListOfStores = await db.Stores.ToListAsync();
            DisplayStoreDetails = new ObservableCollection<StoreDetails>(tempStoreDetails);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading stores directly: {ex.Message}");
        }
	}
}

public class StoreDetails : INotifyPropertyChanged
{
	private int _publisherId;
	public int StoreId
	{
		get => _publisherId;
		set
		{
			_publisherId = value;
			OnPropertyChanged();
		}
	}

	private string _name;
	public string StoreName
	{
		get => _name; 
		set 
		{
			_name = value;
			OnPropertyChanged();
			
		}
	}
	private string _email;

	public string City
	{
		get => _email; 
		set 
		{
			_email = value;
			OnPropertyChanged();
			
		}
	}
	private string _country;

	public string Country
	{
		get => _country;
		set 
		{
			_country = value;
			OnPropertyChanged();
		}
	}
    private string _address;
	public string Address
	{

        get => _address;
        set
		{
			_address = value;
			OnPropertyChanged();
		}
	}
    private string _postalCode;
	public string PostalCode
	{

        get => _postalCode;
        set
		{
			_postalCode = value;
			OnPropertyChanged();
		}
	}
    private string _webpageUrl;
	public string WebpageUrl
	{

        get => _webpageUrl;
        set
		{
			_webpageUrl = value;
			OnPropertyChanged();
		}
	}
    private string _phoneNumber;
	public string PhoneNumber
	{

        get => _phoneNumber;
        set
		{
			_phoneNumber = value;
			OnPropertyChanged();
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	}
}
