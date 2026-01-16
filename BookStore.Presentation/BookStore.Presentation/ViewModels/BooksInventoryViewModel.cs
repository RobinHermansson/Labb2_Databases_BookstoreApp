
using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BookStore.Presentation.ViewModels;

public class BooksInventoryViewModel : ViewModelBase
{

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<InventoryBalanceDetail> _availableBooks;
    private InventoryBalanceDetail? _selectedAvailable;
    private ObservableCollection<InventoryBalanceDetail> _booksAtStore;
    private InventoryBalanceDetail? _selectedBookAtStore;

    public DelegateCommand AddBookToStoreListCommand { get; set; }
    public DelegateCommand RemoveBookFromStoreListCommand { get; set; }


    public ObservableCollection<InventoryBalanceDetail> AvailableBooks
    {
        get => _availableBooks;
        set
        {
            _availableBooks = value;
            RaisePropertyChanged();
        }
    }
    public InventoryBalanceDetail? SelectedAvailable
    {
        get => _selectedAvailable;
        set { _selectedAvailable = value; RaisePropertyChanged(); }
    }
    public ObservableCollection<InventoryBalanceDetail> BooksAtStore
    {
        get => _booksAtStore;
        set { _booksAtStore = value; RaisePropertyChanged(); }
    }
    public InventoryBalanceDetail? SelectedBookAtStore
    {
        get => _selectedBookAtStore;
        set { _selectedBookAtStore = value; RaisePropertyChanged(); }
    }

    public BooksInventoryViewModel(INavigationService navigationService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _ = LoadBookComparisonAtStore(2);
        AddBookToStoreListCommand = new DelegateCommand(AddBookToStoreList, CanAddBookToStoreList);
        RemoveBookFromStoreListCommand = new DelegateCommand(RemoveBookFromStoreList, CanRemoveBookFromStoreList);
    }


    private async Task LoadBookComparisonAtStore(int storeId)
    {
        using var db = new BookstoreDBContext();

        var inventoryBalances = await db.InventoryBalances.Include(ib => ib.Isbn13Navigation).ToListAsync();
        var books = await db.Books.ToListAsync();
        var stores = await db.Stores.ToListAsync();

        var inventoryAvailableAtStore = inventoryBalances
            .Where(ib => ib.StoreId == storeId)
            .Select(ib => new InventoryBalanceDetail()
            {
                BookISBN13 = ib.Isbn13,
                BookTitle = ib.Isbn13Navigation.Title
            });

        BooksAtStore = new ObservableCollection<InventoryBalanceDetail>(inventoryAvailableAtStore);
        AvailableBooks = new ObservableCollection<InventoryBalanceDetail>();
        foreach (Book book in books)
        {
            var bookExists = BooksAtStore.FirstOrDefault(b => b.BookISBN13 == book.Isbn13);
            if (bookExists is null)
            {
                AvailableBooks.Add(new InventoryBalanceDetail() { BookISBN13 = book.Isbn13, BookTitle = book.Title });
            }
        }
    }

    private bool CanRemoveBookFromStoreList(object? sender)
    {
        return SelectedBookAtStore is not null;
    }
    private void RemoveBookFromStoreList(object? sender)
    {
        var bookToRemove = SelectedBookAtStore;

        if (bookToRemove != null)
        {
            BooksAtStore.Remove(bookToRemove);
            AvailableBooks.Add(bookToRemove);
        }
    }
    private bool CanAddBookToStoreList(object? sender)
    {
        return SelectedAvailable is not null;
    }
    private void AddBookToStoreList(object? sender)
    {

        var bookToAdd = SelectedAvailable;

        if (bookToAdd != null)
        {
            BooksAtStore.Add(bookToAdd);
            AvailableBooks.Remove(bookToAdd);
        }
    }
    public class InventoryBalanceDetail() : ViewModelBase
    {
        private string _bookIsbn13;
        private string _bookTitle;
        public string BookISBN13
        {
            get => _bookIsbn13;
            set { _bookIsbn13 = value; RaisePropertyChanged(); }
        }
        public string BookTitle
        {
            get { return _bookTitle; }
            set { _bookTitle = value; RaisePropertyChanged(); }
        }


    }
}

