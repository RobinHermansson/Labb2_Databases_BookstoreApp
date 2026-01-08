using Bookstore.Infrastructure.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

internal class CustomersViewModel : ViewModelBase
{
    private ObservableCollection<CustomerDetails> _displayCustomerDetails;
    private bool _hasChanges;
    private List<CustomerDetails> _newCustomers = new List<CustomerDetails>();
    private List<CustomerDetails> _deletedCustomers = new List<CustomerDetails>();

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




    public DelegateCommand CancelChangesCommand { get; set; }
    public DelegateCommand SaveChangesCommand { get; set; }

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

    public CustomersViewModel()
    {
        CancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
        SaveChangesCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
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
                    Debug.WriteLine($"New customer added: {newCustomer.FirstName} {newCustomer.LastName}");
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
                    Debug.WriteLine($"Customer marked for deletion: {removedCustomer.FirstName} {removedCustomer.LastName}");
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
    public void CancelChanges(object? sender)
    {
        Debug.WriteLine("Cancelling changes");
        _ = LoadAllCustomersAsync();
    }
    public bool CanSaveChanges(object? sender) => HasChanges;

    public void SaveChanges(object? sender)
    {
        Debug.WriteLine("Not yet implemented.");
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
