using Bookstore.Infrastructure.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

public class PublishersViewModel: ViewModelBase
{
	private PublisherDetails _selectedPublisher;
	public AsyncDelegateCommand SaveChangesCommand { get; set; }
	public AsyncDelegateCommand CancelChangesCommand { get; set; }
	private List<PublisherDetails> _deletedPublishers = new List<PublisherDetails>();
    private List<PublisherDetails> _newPublishers = new List<PublisherDetails>();
	private List<PublisherDetails> _changedPublishers = new List<PublisherDetails>();
	private List<Publisher> OriginalListOfPublishers = new List<Publisher>();


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
		
	public PublishersViewModel()
    {
		SaveChangesCommand = new AsyncDelegateCommand(SaveChangesAsync, CanSaveChanges);
		CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
    }

	private async Task SaveChangesAsync(object? sender)
	{
		try
        {

            using var db = new BookstoreDBContext();

            // Handle new authors
            foreach (var newPublisher in _newPublishers)
            {
                var dbPublisher = new Publisher
                {
                    Name = newPublisher.Name,
                    Address = newPublisher.Address,
                    Email = newPublisher.Email,
                    Country = newPublisher.Country
                };
                
                db.Publishers.Add(dbPublisher);
            }

            // Handle deleted authors
            foreach (var deletedPublisher in _deletedPublishers)
            {
                var dbPublisher = await db.Publishers
                .FirstOrDefaultAsync(a => a.Id == deletedPublisher.PublisherId);
                if (dbPublisher != null)
                {
                    db.Publishers.Remove(dbPublisher);
                }
            }
            
            //Handling modified existing authors
            var changedPublishers = new List<PublisherDetails>();
            foreach (var displayPublisher in DisplayPublisherDetails)
            {
                var originalPublisher = OriginalListOfPublishers.FirstOrDefault(a => a.Id == displayPublisher.PublisherId);
                if (originalPublisher != null)
                {
                    if (originalPublisher.Name != displayPublisher.Name ||
                        originalPublisher.Address != displayPublisher.Address ||
                        originalPublisher.Country != displayPublisher.Country ||
                        originalPublisher.Email != displayPublisher.Email)
                    {
                        changedPublishers.Add(displayPublisher);
                    }
                }
            }
            foreach (var changedPublisher in changedPublishers)
            {
                var dbPublisher = await db.Publishers.FindAsync(changedPublisher.PublisherId);

                if (dbPublisher != null)
                {
                    dbPublisher.Name = changedPublisher.Name;
                    dbPublisher.Address = changedPublisher.Address;
                    dbPublisher.Country = changedPublisher.Country;
                    dbPublisher.Email = changedPublisher.Email;
                }
            }

            await db.SaveChangesAsync();

            // Refresh states and the list according to new changes.
            HasChanges = false;
            try
            {
                await LoadPublishersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when loading publishers after saving: {ex.Message}");
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
            await LoadPublishersAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading publishers when cancelling: {ex.Message}");
        }
		_deletedPublishers.Clear();
		_changedPublishers.Clear();
		HasChanges = false;
	}
	private bool CanCancelChanges(object? sender)
	{
		return HasChanges;
	}
    public PublisherDetails SelectedPublisher
	{
		get => _selectedPublisher; 
		set 
		{ 
			_selectedPublisher = value;
			RaisePropertyChanged();
		}
	}


	private ObservableCollection<PublisherDetails> _displayPublisherDetails;

	public ObservableCollection<PublisherDetails> DisplayPublisherDetails
	{
		get => _displayPublisherDetails;
		set 
		{ 
			// Unsubscribe from old collection
            if (_displayPublisherDetails != null)
            {
                _displayPublisherDetails.CollectionChanged -= DisplayPublisherDetails_CollectionChanged;
                foreach (var publisher in _displayPublisherDetails)
                {
                    publisher.PropertyChanged -= PublisherDetails_PropertyChanged;
                }
            }

            _displayPublisherDetails = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_displayPublisherDetails != null)
            {
                _displayPublisherDetails.CollectionChanged += DisplayPublisherDetails_CollectionChanged;
                foreach (var publisher in _displayPublisherDetails)
                {
                    publisher.PropertyChanged += PublisherDetails_PropertyChanged;
                }
            }

		}
	}
	private void DisplayPublisherDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (PublisherDetails newPublisher in e.NewItems)
            {
                newPublisher.PropertyChanged += PublisherDetails_PropertyChanged;
                _newPublishers.Add(newPublisher);
                
            }
        }

        if (e.OldItems != null)
        {
            foreach (PublisherDetails removedPublisher in e.OldItems)
            {
                removedPublisher.PropertyChanged -= PublisherDetails_PropertyChanged;
                if (_newPublishers.Contains(removedPublisher))
                {
                    _newPublishers.Remove(removedPublisher);
                } 
                else if (removedPublisher.PublisherId > 0)
                {
                    _deletedPublishers.Add(removedPublisher);
                }
            }
        }

        CheckForChanges();
    }
    private void PublisherDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (
			e.PropertyName == nameof(PublisherDetails.Name) ||
			e.PropertyName == nameof(PublisherDetails.Email) ||
            e.PropertyName == nameof(PublisherDetails.Country) ||
            e.PropertyName == nameof(PublisherDetails.Address))
        {
            CheckForChanges();
        }
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_deletedPublishers.Any())
        {
            hasAnyChanges = true;
        }
        if (_newPublishers.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var displayPublisher in DisplayPublisherDetails)
        {
            var originalPublisher = OriginalListOfPublishers.FirstOrDefault(o => o.Id == displayPublisher.PublisherId);
            if (originalPublisher != null)
            {
                if (
					originalPublisher.Name != displayPublisher.Name ||
					originalPublisher.Email != displayPublisher.Email ||
                    originalPublisher.Country != displayPublisher.Country ||
                    originalPublisher.Address != displayPublisher.Address)
                {
					_changedPublishers.Add(displayPublisher);
                    hasAnyChanges = true;
                    break;
                }
            }
        }

        HasChanges = hasAnyChanges;
    }

	public async Task LoadPublishersAsync()
	{
        try
        {
            using var db = new BookstoreDBContext();

            var tempPublisherDetails = await db.Publishers
                .Select(o => new PublisherDetails()
                {
                    PublisherId = o.Id, 
                    Name = o.Name,
                    Email = o.Email,
                    Country = o.Country,
                    Address = o.Address
                }).ToListAsync();

            OriginalListOfPublishers = await db.Publishers.ToListAsync();
            DisplayPublisherDetails = new ObservableCollection<PublisherDetails>(tempPublisherDetails);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error when loading publishers directly: {ex.Message}");
        }
	}
}

public class PublisherDetails : INotifyPropertyChanged
{
	private int _publisherId;
	public int PublisherId
	{
		get => _publisherId;
		set
		{
			_publisherId = value;
			OnPropertyChanged();
		}
	}

	private string _name;
	public string Name
	{
		get => _name; 
		set 
		{
			_name = value;
			OnPropertyChanged();
			
		}
	}
	private string _email;

	public string Email
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
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

	}
}
