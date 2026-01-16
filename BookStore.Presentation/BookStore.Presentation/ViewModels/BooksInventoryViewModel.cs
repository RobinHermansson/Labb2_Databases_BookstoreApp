
using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace BookStore.Presentation.ViewModels;

public class BooksInventoryViewModel : ViewModelBase
{

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<InventoryBalanceDetail> _availableBooks;
    private Book _selectedAvailable;


    public ObservableCollection<InventoryBalanceDetail> AvailableBooks
    {
        get => _availableBooks;
        set { _availableBooks = value; RaisePropertyChanged(); }
    }
    public Book SelectedAvailable
    {
        get => _selectedAvailable;
        set { _selectedAvailable = value; RaisePropertyChanged(); }
    }

    public BooksInventoryViewModel(INavigationService navigationService, IDialogService dialogService )
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _ = LoadAvailableBooksAtStore(2);
    }


    private async Task LoadAvailableBooksAtStore(int storeId)
    {
        using var db = new BookstoreDBContext();

        var inventoryBalances = await db.InventoryBalances.Include(ib => ib.Isbn13Navigation).ToListAsync();
        var books = await db.Books.ToListAsync();
        var stores = await db.Stores.ToListAsync();

        var inventoryAvailableAtStore = inventoryBalances
            .Where(ib => ib.StoreId == storeId)
            .Select(ib => new InventoryBalanceDetail() 
            { 
                BookISBN13 = ib.Isbn13, BookTitle = ib.Isbn13Navigation.Title
            });

        AvailableBooks = new ObservableCollection<InventoryBalanceDetail>(inventoryAvailableAtStore);
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

