using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;
public class CustomersViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private ObservableCollection<CustomerDetails> _displayCustomerDetails;
    private bool _hasChanges;
    private List<CustomerDetails> _newCustomers = new List<CustomerDetails>();
    private List<CustomerDetails> _deletedCustomers = new List<CustomerDetails>();
    private List<CustomerDetails> _changedCustomers = new List<CustomerDetails>();

    public List<Customer> OriginalListOfCustomers;

    private CustomerDetails _selectedCustomer;

    public CustomerDetails SelectedCustomer
    {
        get => _selectedCustomer;
        set 
        { 
            _selectedCustomer = value;
            RaisePropertyChanged();
        }
    }

    public AsyncDelegateCommand CancelChangesCommand { get; set; }
    public AsyncDelegateCommand SaveChangesCommand { get; set; }

    public bool HasChanges
    {
        get => _hasChanges;
        set
        {
            _hasChanges = value;
            RaisePropertyChanged();
            CancelChangesCommand?.RaiseCanExecuteChanged();
            SaveChangesCommand?.RaiseCanExecuteChanged();
        }
    }

    public CustomersViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
        SaveChangesCommand = new AsyncDelegateCommand(SaveChanges, CanSaveChanges);
    }
    public ObservableCollection<CustomerDetails> DisplayCustomerDetails
    {
        get => _displayCustomerDetails;
        set
        {

            // Unsubscribe from old collection
            if (_displayCustomerDetails != null)
            {
                _displayCustomerDetails.CollectionChanged -= DisplayCustomerDetails_CollectionChanged;
                foreach (var customer in _displayCustomerDetails)
                {
                    customer.PropertyChanged -= CustomerDetails_PropertyChanged;
                }
            }

            _displayCustomerDetails = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_displayCustomerDetails != null)
            {
                _displayCustomerDetails.CollectionChanged += DisplayCustomerDetails_CollectionChanged;
                foreach (var customer in _displayCustomerDetails)
                {
                    customer.PropertyChanged += CustomerDetails_PropertyChanged;
                }
            }
        }
    }
    private void DisplayCustomerDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CustomerDetails newCustomer in e.NewItems)
            {
                newCustomer.PropertyChanged += CustomerDetails_PropertyChanged;

                if (newCustomer.Id <= 0)
                {
                    _newCustomers.Add(newCustomer);
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (CustomerDetails removedCustomer in e.OldItems)
            {
                removedCustomer.PropertyChanged -= CustomerDetails_PropertyChanged;

                if (_newCustomers.Contains(removedCustomer))
                {
                    _newCustomers.Remove(removedCustomer);
                }
                else if (removedCustomer.Id > 0)
                {
                    _deletedCustomers.Add(removedCustomer);
                }
            }
        }

        CheckForChanges();
    }
    private void CustomerDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (e.PropertyName == nameof(CustomerDetails.FirstName) ||
            e.PropertyName == nameof(CustomerDetails.LastName) ||
            e.PropertyName == nameof(CustomerDetails.Email) ||
            e.PropertyName == nameof(CustomerDetails.Phone))
        {
            CheckForChanges();
        }
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_newCustomers.Any())
        {
            hasAnyChanges = true;
        }

        if (_deletedCustomers.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var displayCustomer in DisplayCustomerDetails)
        {
            var originalCustomer = OriginalListOfCustomers.FirstOrDefault(a => a.Id == displayCustomer.Id);
            if (originalCustomer != null)
            {
                if (originalCustomer.FirstName != displayCustomer.FirstName ||
                    originalCustomer.LastName != displayCustomer.LastName ||
                    originalCustomer.Email != displayCustomer.Email ||
                    originalCustomer.Phone != displayCustomer.Phone)
                {
                    hasAnyChanges = true;
                    _changedCustomers.Add(displayCustomer);
                    break;
                }
            }
        }

        HasChanges = hasAnyChanges;
    }
    public async Task LoadAllCustomersAsync()
    {
        using var db = new BookstoreDBContext();

        var customerDetails = await db.Customers.Select(c =>
            new CustomerDetails()
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone
            }
        ).ToListAsync();

        OriginalListOfCustomers = await db.Customers.ToListAsync();
        DisplayCustomerDetails = new ObservableCollection<CustomerDetails>(customerDetails);
    }

    public bool CanCancelChanges(object? sender) => HasChanges;
    public async Task CancelChanges(object? sender)
    {
        try
        {
            await LoadAllCustomersAsync();

        } catch(Exception ex)
        {
            Debug.WriteLine($"Error when cancelling changes and loading customers: {ex.Message}");
        }
        _newCustomers.Clear();
        _changedCustomers.Clear();
        _deletedCustomers.Clear();
    }
    public bool CanSaveChanges(object? sender) => HasChanges;

    public async Task SaveChanges(object? sender)
    {
        try
        {
            using var db = new BookstoreDBContext();
            if (_newCustomers.Count != 0)
            {
                foreach (var newCustomer in _newCustomers)
                {
                    db.Customers.Add(new Customer() 
                    {
                        FirstName = newCustomer.FirstName,
                        LastName = newCustomer.LastName,
                        Email = newCustomer.Email,
                        Phone = newCustomer.Phone
                    });
                }
            }
            if (_deletedCustomers.Count != 0)
            {
                foreach (var deletedCustomer in _deletedCustomers)
                {
                    var toBeDeleted = db.Customers.FirstOrDefault(c => c.Id == deletedCustomer.Id);
                    if (toBeDeleted is not null)
                    {
                        db.Customers.Remove(toBeDeleted);
                    }
                 }
            }
            if (_changedCustomers.Count != 0)
            {
                foreach(var changedCustomer in _changedCustomers)
                {
                    var toBeUpdated = db.Customers.FirstOrDefault(c => c.Id == changedCustomer.Id);
                    if (toBeUpdated is not null)
                    {
                        toBeUpdated.FirstName = changedCustomer.FirstName;
                        toBeUpdated.LastName = changedCustomer.LastName;
                        toBeUpdated.Email = changedCustomer.Email;
                        toBeUpdated.Phone = changedCustomer.Phone;
                    }
                }
            }
            
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when saving changes: {ex.Message}");
        }

        _newCustomers.Clear();
        _deletedCustomers.Clear();
        _changedCustomers.Clear();
        HasChanges = false;
        try
        {
            await LoadAllCustomersAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading customers after saving: {ex.Message}");
        }
    }
    public void ClearState()
    {
        _newCustomers.Clear();
        _deletedCustomers.Clear();
        _changedCustomers.Clear();
        HasChanges = false;
    }


}

public class CustomerDetails : INotifyPropertyChanged
{
    private int _id;
    private string _firstName;
    private string _lastName;
    private string _email;
    private string _phone;



    public int Id
    {
        get => _id;
        set
        {
            _id = value;
        }
    }
    public string Email
    {
        get => _email;
        set
        {
            _email = value;
            OnPropertyChanged();
        }
    }



    public string Phone
    {
        get => _phone;
        set
        {
            _phone = value;
            OnPropertyChanged();
        }
    }

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
