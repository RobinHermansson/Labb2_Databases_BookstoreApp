using Accessibility;
using Bookstore.Infrastructure.Data.Model;
using CompanyDemo.Presentation.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace BookStore.Presentation.ViewModels;

internal class BooksViewModel : ViewModelBase
{
	private ObservableCollection<BookDetails> _books;
    private readonly MainWindowViewModel _mainViewModel;
    public List<Book> OriginalListOfBooks;

    private List<BookDetails> _newBooks = new List<BookDetails>();
    private List<BookDetails> _deletedBooks = new List<BookDetails>();
    private List<BookDetails> _changedBooks = new List<BookDetails>();

    

    public DelegateCommand SaveChangesCommand { get; set; }
    public DelegateCommand CancelChangesCommand { get; set; }


    private BookDetails _selectedBook;

    public BookDetails SelectedBook
    {
        get { return _selectedBook; }
        set 
        { 
            _selectedBook = value;
            RaisePropertyChanged();
        }
    }


    private bool _hasChanges;

    public bool HasChanges
    {
        get { return _hasChanges; }
        set 
        { 
            _hasChanges = value;
            RaisePropertyChanged();
            SaveChangesCommand?.RaiseCanExecuteChanged();
            CancelChangesCommand?.RaiseCanExecuteChanged();
        }
    }




    public ObservableCollection<BookDetails> Books
	{
		get => _books; 
		set 
		{
			// Unsubscribe from old collection
            if (_books != null)
            {
                _books.CollectionChanged -= Books_CollectionChanged;
                foreach (var book in _books)
                {
                    book.PropertyChanged -= Books_PropertyChanged;
                }
            }

            _books = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_books != null)
            {
                _books.CollectionChanged += Books_CollectionChanged;
                foreach (var book in _books)
                {
                    book.PropertyChanged += Books_PropertyChanged;
                }
            }

			_books = value;
			RaisePropertyChanged();
		}
	}
	 private void Books_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (BookDetails newBook in e.NewItems)
            {
                newBook.PropertyChanged += Books_PropertyChanged;
                _newBooks.Add(newBook);
                Debug.WriteLine($"New Book added: {newBook.ISBN13} {newBook.Title}");
            }
        }

        if (e.OldItems != null)
        {
            foreach (BookDetails removedBook in e.OldItems)
            {
                removedBook.PropertyChanged -= Books_PropertyChanged;
                
                if (_newBooks.Contains(removedBook))
                {
                    _newBooks.Remove(removedBook);
                }
                
                _deletedBooks.Add(removedBook);
                Debug.WriteLine($"Book marked for deletion: {removedBook.ISBN13} {removedBook.Title}");
            }
        }

        CheckForChanges();
    }
    private void Books_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (e.PropertyName == nameof(BookDetails.ISBN13) ||
            e.PropertyName == nameof(BookDetails.Title) ||
            e.PropertyName == nameof(BookDetails.PublicationDate) ||
            e.PropertyName == nameof(BookDetails.PriceInSek))
        {
            CheckForChanges();
        }
    }
    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_newBooks.Any())
        {
            hasAnyChanges = true;
        }

        if (_deletedBooks.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var book in Books)
        {
            var originalBook = OriginalListOfBooks.FirstOrDefault(b => b.Isbn13 == book.ISBN13);
            if (originalBook != null)
            {
                if (originalBook.Isbn13 != book.ISBN13 ||
                    originalBook.Title != book.Title ||
                    originalBook.PublicationDate != book.PublicationDate ||
                    originalBook.PriceInSek != book.PriceInSek)
                {
                    _changedBooks.Add(book);
                    hasAnyChanges = true;
                    break;
                }
            }
        }

        HasChanges = hasAnyChanges;
    }




	public string StoreAtInstantiation { get; set; }

    public BooksViewModel(MainWindowViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        Debug.WriteLine("Empty init called.");
        SaveChangesCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
        CancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);

 
    }

    /*public BooksViewModel(string storeName)
    {
        Debug.WriteLine($"Storename called.");
		StoreAtInstantiation = storeName;
                
    }*/

    private bool CanCancelChanges(object? sender)
    {
        return HasChanges;
    }
    private bool CanSaveChanges(object? sender)
    {
        return HasChanges;
    }
    private void SaveChanges(object? sender)
    {
        Debug.WriteLine("Save changes called.");
        using var db = new BookstoreDBContext();
        foreach (BookDetails newBook in _newBooks)
        {
            Debug.WriteLine($"Attempting to save new book with isbn: {newBook.ISBN13}");
            db.Books.Add(
            new Book()
            {
                Isbn13 = newBook.ISBN13,
                Title = newBook.Title,
                PriceInSek = newBook.PriceInSek,
                PublicationDate = newBook.PublicationDate,
                Language = newBook.Language
            }
            );

            db.InventoryBalances.Add(new InventoryBalance()
            {
                StoreId = _mainViewModel.SelectedStore.Id,
                Isbn13 = newBook.ISBN13,
                Quantity = newBook.Quantity
            });
        }
        foreach (BookDetails deletedBook in _deletedBooks)
        {
            Debug.WriteLine($"Attempting to delete book. {deletedBook.ISBN13}");
            var bookToDelete = db.Books.FirstOrDefault(b => b.Isbn13 == deletedBook.ISBN13);
            var relatedInventoryBalance = db.InventoryBalances.FirstOrDefault(ib => ib.Isbn13 == bookToDelete.Isbn13);
            if (relatedInventoryBalance is not null)
            {
                if (bookToDelete is not null)
                {
                    db.InventoryBalances.Remove(relatedInventoryBalance);
                    db.Books.Remove(bookToDelete);
                }
            }


        } 
        
        foreach (BookDetails changedBook in _changedBooks)
        {
            Debug.WriteLine($"Attempting to save changed book with isbn13: {changedBook.ISBN13}");
            var bookToChange = db.Books.FirstOrDefault(b => b.Isbn13 == changedBook.ISBN13);
            if (bookToChange is not null)
            {
                bookToChange.Isbn13 = changedBook.ISBN13;
                bookToChange.Title = changedBook.Title;
                bookToChange.PublicationDate = changedBook.PublicationDate;
                bookToChange.PriceInSek = changedBook.PriceInSek;
                bookToChange.Language = changedBook.Language;
            }

        }

        db.SaveChanges();
        _newBooks.Clear();
        _deletedBooks.Clear();
        _changedBooks.Clear();
        _ = LoadBooksForSelectedStore(_mainViewModel.SelectedStore.Id);

        HasChanges = false;
    }



    private void CancelChanges(object obj)
    {
        Debug.WriteLine("Cancel.");
        _ = LoadBooksForSelectedStore(_mainViewModel.SelectedStore.Id);
    }


	public async Task LoadBooksForSelectedStore(int storeId = 1)
	{
        Debug.WriteLine("Load books called.");
		using var db = new BookstoreDBContext();

		var bookDetailsList = await db.InventoryBalances
			.Where(s => s.StoreId == storeId)
			.Select(s => new BookDetails() { ISBN13 = s.Isbn13, Title = s.Isbn13Navigation.Title, PriceInSek = s.Isbn13Navigation.PriceInSek, PublicationDate = s.Isbn13Navigation.PublicationDate, Quantity = s.Quantity, Language = s.Isbn13Navigation.Language })
			.ToListAsync();
        OriginalListOfBooks = await db.Books.ToListAsync();
        Books = new ObservableCollection<BookDetails>(bookDetailsList);
	}
}

public class BookDetails : INotifyPropertyChanged
{
	private string _isbn13;
    public string ISBN13 
	{ 
		get => _isbn13;
		set 
		{
			_isbn13 = value;
			OnPropertyChanged();
		}
	}

	private string _title;

	public string Title { 

		get { return _title; }
		set 
		{ 
			_title = value;
			OnPropertyChanged();
		}
	}
	private decimal _priceInSek;

	public decimal PriceInSek
	{
		get { return _priceInSek; }
		set 
		{ 
			_priceInSek = value;
			OnPropertyChanged();
		}
	}
    private string _language;

    public string Language
    {
        get { return _language; }
        set 
        { 
            _language = value;
            OnPropertyChanged();
        }
    }


    private DateOnly _publicationDate;

	public DateOnly PublicationDate
	{
		get { return _publicationDate; }
		set 
		{ 
			_publicationDate = value;
			OnPropertyChanged();
		}
	}

	private int _quantity;

	public int Quantity
	{
		get { return _quantity; }
		set
		{ 
			_quantity = value;
			OnPropertyChanged();
		}
	}

    public event PropertyChangedEventHandler? PropertyChanged;
	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
