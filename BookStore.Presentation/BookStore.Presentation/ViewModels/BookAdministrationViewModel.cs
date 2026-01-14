using Bookstore.Infrastructure.Data.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BookStore.Presentation.ViewModels;

public class BookAdministrationViewModel: ViewModelBase
{

	public string TitleText { get; set; } = "Edit book:";
	private string _isbn13;
	private bool _isLoading;

	public bool IsLoading
	{
		get { return _isLoading; }
		set { _isLoading = value; RaisePropertyChanged(); }
	}


	public string ISBN13
	{
		get { return _isbn13; }
		set 
		{ 
			_isbn13 = value;
			RaisePropertyChanged();
		}
	}
	private string _title;

	public string Title
	{
		get { return _title; }
		set 
		{ 
			_title = value;
			RaisePropertyChanged();
		}
	}
	public IEnumerable<Language> AvailableLanguages => Enum.GetValues<Language>();

	public Language SelectedLanguage
	{
		get => _bookToAdmin?.Language ?? Language.English;
		set 
		{
			if (_bookToAdmin is not null)
			{
                _bookToAdmin.Language = value;
                RaisePropertyChanged();
			}
		}
	}
	
	private decimal _priceInSek;

	public decimal PriceInSek
	{
		get { return _priceInSek; }
		set 
		{ 
			_priceInSek = value;
			RaisePropertyChanged();
		}
	}

	private int _quantity;

	public int Quantity
	{
		get { return _quantity; }
		set { _quantity = value; RaisePropertyChanged(); }
	}

	private DateOnly _publicationDate;

	public DateOnly PublicationDate
	{
		get { return _publicationDate; }
		set 
		{ 
			_publicationDate = value;
			RaisePropertyChanged();
		}
	}
	private ObservableCollection<Publisher> _publisher;

	private Publisher _selectedPublisher;

	public Publisher SelectedPublisher
	{
		get { return _selectedPublisher; }
		set { _selectedPublisher = value; RaisePropertyChanged(); }
	}


	public ObservableCollection<Publisher> AvailablePublishers
	{
		get { return _publisher; }
		set 
		{ 
			_publisher = value;
			RaisePropertyChanged();
		}
	}

	private Author _selectedAuthor;

	public Author SelectedAuthor
	{
		get { return _selectedAuthor; }
		set { _selectedAuthor = value; RaisePropertyChanged(); }
	}


	private ObservableCollection<Author> _availableAuthors;

	public ObservableCollection<Author> AvailableAuthors
	{
		get { return _availableAuthors; }
		set 
		{ 
			_availableAuthors = value;
			RaisePropertyChanged();
		}
	}

	private BookDetails _bookToAdmin;

	public BookDetails BookToAdmin
	{
		get => _bookToAdmin; 
		set 
		{ 
			_bookToAdmin = value;
			RaisePropertyChanged();
		}
	}

	public BookAdministrationViewModel(BookDetails bookToAdmin)
    {
		_bookToAdmin = bookToAdmin;
		
		if (bookToAdmin.ISBN13 is null)
		{
			TitleText = "Create a new book:";
		}
		_isbn13 = bookToAdmin.ISBN13;
		_title = bookToAdmin.Title;
		_publicationDate = bookToAdmin.PublicationDate;
		_priceInSek = bookToAdmin.PriceInSek;
    }
	public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            await LoadRelatedDataAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
	private async Task LoadRelatedDataAsync()
    {
        using var db = new BookstoreDBContext();
        
		var publishers= await db.Publishers.ToListAsync();
        var authors = await db.Authors.ToListAsync();
        
        AvailablePublishers = new ObservableCollection<Publisher>(publishers);
        AvailableAuthors = new ObservableCollection<Author>(authors);
        
		if (!string.IsNullOrEmpty(_bookToAdmin.ISBN13))       
		{

            var existingBook = await db.Books
                .Include(b => b.Publisher)
                .Include(b => b.Authors)
				.Include(b => b.InventoryBalances)
                .FirstOrDefaultAsync(b => b.Isbn13 == _bookToAdmin.ISBN13);
                
            if (existingBook != null)
            {
                SelectedPublisher = AvailablePublishers.FirstOrDefault(p => p.Id == existingBook.PublisherId);
                
                var bookAuthor = existingBook.Authors.FirstOrDefault();
                if (bookAuthor != null)
                {
                    SelectedAuthor = AvailableAuthors.FirstOrDefault(a => a.Id == bookAuthor.Id);
                }

				var bookQuantity = existingBook.InventoryBalances.FirstOrDefault();
				if (bookQuantity != null)
				{
					var existingBooksInventoryBalance = existingBook.InventoryBalances.FirstOrDefault(ib => ib.Isbn13 == existingBook.Isbn13);
					if (existingBooksInventoryBalance is not null)
					{
						Quantity = existingBooksInventoryBalance.Quantity;
					}
				}
            }			
        }
    }
}
