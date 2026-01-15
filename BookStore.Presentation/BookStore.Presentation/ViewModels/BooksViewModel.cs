using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

public class BooksViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<BookDetails> _books;
    private ObservableCollection<Store> _stores;
    private Store _selectedStore;
    public List<Book> OriginalListOfBooks;

    private List<BookDetails> _deletedBooks = new List<BookDetails>();
    private List<BookDetails> _changedBooks = new List<BookDetails>();



    public AsyncDelegateCommand SaveChangesCommand { get; set; }
    public DelegateCommand EditBookCommand { get; set; }
    public AsyncDelegateCommand CancelChangesCommand { get; set; }
    public DelegateCommand RemoveBookCommand { get; set; }
    public DelegateCommand AddBookCommand { get; set; }


    private BookDetails _selectedBook;

    public BookDetails SelectedBook
    {
        get { return _selectedBook; }
        set
        {
            _selectedBook = value;
            RaisePropertyChanged();
            EditBookCommand?.RaiseCanExecuteChanged();

        }
    }

    public Store SelectedStore
    {
        get => _selectedStore;
        set
        {
            _selectedStore = value;
            RaisePropertyChanged();
            if (value != null)
                _ = LoadBooksForSelectedStore(value.Id);
        }
    }

    public ObservableCollection<Store> Stores
    {
        get => _stores;
        set
        {
            _stores = value;
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
        }
    }
    private void Books_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (BookDetails removedBook in e.OldItems)
            {
                removedBook.PropertyChanged -= Books_PropertyChanged;

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
                }
            }
        }

        HasChanges = hasAnyChanges;
    }

    public string StoreAtInstantiation { get; set; }

    public BooksViewModel(INavigationService navigationService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        SaveChangesCommand = new AsyncDelegateCommand(SaveChanges, CanSaveChanges);
        CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
        EditBookCommand = new DelegateCommand(EditBook, CanEditBook);
        AddBookCommand = new DelegateCommand(AddBook, CanAddBook);
    }

    private bool CanCancelChanges(object? sender)
    {
        return HasChanges;
    }
    private bool CanEditBook(object? sender)
    {
        return SelectedBook is not null;
    }
    private void EditBook(object? sender)
    {
        _navigationService.NavigateTo("BookAdministration", "BooksView", SelectedBook);
    }

    private bool CanSaveChanges(object? sender)
    {
        return HasChanges;
    }
    private async Task SaveChanges(object? sender)
    {
        try
        {
            using var db = new BookstoreDBContext();
            bool hasChangesToSave = false;

            // Handle deletions with confirmation
            if (_deletedBooks.Any())
            {
                bool proceedWithDeletion = await _dialogService.ShowConfirmationDialogAsync(
                    "You are about to delete one or more books! Are you certain you want to delete these books and their relations?",
                    "Confirm Deletion");

                if (proceedWithDeletion)
                {
                    foreach (BookDetails deletedBook in _deletedBooks)
                    {
                        var bookToDelete = db.Books.FirstOrDefault(b => b.Isbn13 == deletedBook.ISBN13);
                        if (bookToDelete is not null)
                        {
                            var relatedInventoryBalance = db.InventoryBalances.FirstOrDefault(ib => ib.Isbn13 == bookToDelete.Isbn13);
                            if (relatedInventoryBalance is not null)
                            {
                                db.InventoryBalances.Remove(relatedInventoryBalance);
                            }

                            var relatedOrderItems = db.OrderItems
                                .Where(oi => oi.Isbn13 == bookToDelete.Isbn13);
                            db.OrderItems.RemoveRange(relatedOrderItems);

                            var bookWithAuthors = db.Books
                                .Include(b => b.Authors)
                                .FirstOrDefault(b => b.Isbn13 == bookToDelete.Isbn13);
                            if (bookWithAuthors != null)
                            {
                                bookWithAuthors.Authors.Clear();
                            }

                            db.Books.Remove(bookToDelete);
                            hasChangesToSave = true;
                        }
                    }
                }
                else
                {
                    _deletedBooks.Clear();
                }
            }

            if (_changedBooks.Any())
            {
                foreach (BookDetails changedBook in _changedBooks)
                {
                    var bookToChange = db.Books.FirstOrDefault(b => b.Isbn13 == changedBook.ISBN13);
                    if (bookToChange is not null)
                    {
                        bookToChange.Isbn13 = changedBook.ISBN13;
                        bookToChange.Title = changedBook.Title;
                        bookToChange.PublicationDate = changedBook.PublicationDate;
                        bookToChange.PriceInSek = changedBook.PriceInSek;
                        bookToChange.Language = changedBook.Language;
                        hasChangesToSave = true;
                    }
                }
            }

            if (hasChangesToSave)
            {
                await db.SaveChangesAsync();
            }

            _deletedBooks.Clear();
            _changedBooks.Clear();

            await LoadBooksForSelectedStore(SelectedStore.Id);

            HasChanges = false;
            await _dialogService.ShowMessageDialogAsync($"Successfully saved books.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync($"Error saving changes: {ex.Message}", "ERROR");
            Debug.WriteLine($"Error in SaveChanges: {ex}");
        }

    }



    private async Task CancelChanges(object? obj)
    {
        try
        {
            Task result = LoadBooksForSelectedStore(SelectedStore.Id);
            HasChanges = false;
            _deletedBooks.Clear();
            _changedBooks.Clear();
            await _dialogService.ShowMessageDialogAsync("Reverted the pending changes.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when cancelling changes and reloading. {ex.Message}");
        }
    }

    private void AddBook(object? sender)
    {
        _navigationService.NavigateTo("BookAdministration", "BooksView", new BookDetails() { BookStoreId = SelectedStore.Id });
    }

    private bool CanAddBook(object? sender)
    {
        return true; //can always add a book
    }


    public async Task LoadBooksForSelectedStore(int storeId = 1)
    {
        using var db = new BookstoreDBContext();

        try
        {

            var bookDetailsList = await db.InventoryBalances
                .Where(s => s.StoreId == storeId)
                .Select(s => new BookDetails() { ISBN13 = s.Isbn13, BookStoreId = storeId, Title = s.Isbn13Navigation.Title, PriceInSek = s.Isbn13Navigation.PriceInSek, PublicationDate = s.Isbn13Navigation.PublicationDate, Quantity = s.Quantity, Language = s.Isbn13Navigation.Language })
                .ToListAsync();
            OriginalListOfBooks = await db.Books.ToListAsync();
            Books = new ObservableCollection<BookDetails>(bookDetailsList);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when running LoadBooksForSelectedStore. {ex.Message}");
        }
    }

    public async Task LoadStoresAsync()
    {
        int? selectedStoreId = SelectedStore?.Id;

        using var db = new BookstoreDBContext();
        try
        {
            var tempList = await db.Stores.ToListAsync();
            Stores = new ObservableCollection<Store>(tempList);

            if (selectedStoreId.HasValue)
            {
                SelectedStore = Stores.FirstOrDefault(s => s.Id == selectedStoreId.Value);
            }

            if (SelectedStore is null)
            {
                SelectedStore = Stores.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when running 'LoadStoresAsync'. {ex.Message}");
        }
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

    private int _bookStoreId;
    public int BookStoreId
    {
        get => _bookStoreId;
        set
        {
            _bookStoreId = value;
            OnPropertyChanged();
        }
    }

    private string _title;

    public string Title
    {

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
    private Language _language;

    public Language Language
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
