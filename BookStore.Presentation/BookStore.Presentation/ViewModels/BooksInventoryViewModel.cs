
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

    private bool _hasChanges;
    private ObservableCollection<InventoryBalanceDetail> _originalListAtStore = new();
    private ObservableCollection<InventoryBalanceDetail> _originalListWithAvailable = new();

    private ObservableCollection<InventoryBalanceDetail> _availableBooks;
    private InventoryBalanceDetail? _selectedAvailable;
    private ObservableCollection<InventoryBalanceDetail> _booksAtStore;
    private InventoryBalanceDetail? _selectedBookAtStore;

    private ObservableCollection<Store> _stores;
    private Store _selectedStore;

    public DelegateCommand AddBookToStoreListCommand { get; set; }
    public DelegateCommand RemoveBookFromStoreListCommand { get; set; }
    public AsyncDelegateCommand SaveChangesCommand { get; set; }
    public DelegateCommand CancelChangesCommand { get; set; }
    public AsyncDelegateCommand BackToBooksCommand { get; set; }

    public ObservableCollection<Store> Stores
    {
        get => _stores;
        set { _stores = value; RaisePropertyChanged(); }

    }
    public Store SelectedStore
    {
        get => _selectedStore;
        set 
        { 
            _selectedStore = value; RaisePropertyChanged();
            _ = LoadBookComparisonAtStore();
        }
    }

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

    public BooksInventoryViewModel(Store incomingStore, INavigationService navigationService, IDialogService dialogService)
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
        _ = LoadStores(incomingStore);

        _ = LoadBookComparisonAtStore();

        AddBookToStoreListCommand = new DelegateCommand(AddBookToStoreList, CanAddBookToStoreList);
        RemoveBookFromStoreListCommand = new DelegateCommand(RemoveBookFromStoreList, CanRemoveBookFromStoreList);
        CancelChangesCommand = new DelegateCommand(CancelChanges, CanCancelChanges);
        SaveChangesCommand = new AsyncDelegateCommand(SaveInventoryChangesAsync, CanSaveChanges);
        BackToBooksCommand = new AsyncDelegateCommand(GoBack, CanGoBack);

    }


    private async Task LoadStores(Store storeFromConstructor)
    {
        using var db = new BookstoreDBContext();
        var stores = await db.Stores.ToListAsync();
        Stores = new ObservableCollection<Store>(stores);
        SelectedStore = Stores.FirstOrDefault(s => s.Id == storeFromConstructor.Id);
    }
    private async Task LoadBookComparisonAtStore()
    {
        using var db = new BookstoreDBContext();

        var inventoryBalances = await db.InventoryBalances.Include(ib => ib.Isbn13Navigation).ToListAsync();
        var books = await db.Books.ToListAsync();

        var inventoryAvailableAtStore = inventoryBalances
            .Where(ib => ib.StoreId == SelectedStore.Id)
            .Select(ib => new InventoryBalanceDetail()
            {
                BookISBN13 = ib.Isbn13,
                BookTitle = ib.Isbn13Navigation.Title
            });

        foreach (var ib in inventoryAvailableAtStore)
        {
            _originalListAtStore.Add(ib);
        }

        BooksAtStore = new ObservableCollection<InventoryBalanceDetail>(inventoryAvailableAtStore);
        AvailableBooks = new ObservableCollection<InventoryBalanceDetail>();
        foreach (Book book in books)
        {
            var bookExists = BooksAtStore.FirstOrDefault(b => b.BookISBN13 == book.Isbn13);
            if (bookExists is null)
            {
                AvailableBooks.Add(new InventoryBalanceDetail() { BookISBN13 = book.Isbn13, BookTitle = book.Title });
                _originalListWithAvailable.Add(new InventoryBalanceDetail() { BookISBN13 = book.Isbn13, BookTitle = book.Title });
            }
        }
    }


    private void CheckForChanges()
    {
        bool hasAnyChanges = false;
        foreach (var ib in BooksAtStore)
        {
            var exists = _originalListAtStore.FirstOrDefault(b => b.BookISBN13 == ib.BookISBN13);
            if (exists is null)
            {
                hasAnyChanges = true;
                break;
            }
        }
        if (!hasAnyChanges)
            foreach (var ib in _originalListAtStore)
            {
                var exists = BooksAtStore.FirstOrDefault(b => b.BookISBN13 == ib.BookISBN13);
                if (exists is null)
                {
                    hasAnyChanges = true;
                    break;
                }
            }
        HasChanges = hasAnyChanges;
    }

    private bool CanCancelChanges(object? sender)
    {
        return HasChanges;
    }
    private void CancelChanges(object? sender)
    {
        _ = LoadBookComparisonAtStore();
        HasChanges = false;
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
        CheckForChanges();
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
        CheckForChanges();
    }
    private bool CanSaveChanges(object? sender)
    {
        return HasChanges;
    }
    public async Task SaveInventoryChangesAsync(object? sender)
    {
        try
        {
            using var db = new BookstoreDBContext();

            var currentStoreInventory = await db.InventoryBalances
                .Where(ib => ib.StoreId == SelectedStore.Id)
                .ToListAsync();

            var updatedIsbn13Set = BooksAtStore.Select(b => b.BookISBN13).ToHashSet();

            var recordsToDelete = currentStoreInventory
                .Where(ib => !updatedIsbn13Set.Contains(ib.Isbn13))
                .ToList();

            var currentIsbn13Set = currentStoreInventory.Select(ib => ib.Isbn13).ToHashSet();
            var recordsToAdd = BooksAtStore
                .Where(b => !currentIsbn13Set.Contains(b.BookISBN13))
                .Select(b => new InventoryBalance
                {
                    StoreId = 2,
                    Isbn13 = b.BookISBN13,
                    Quantity = 0
                })
                .ToList();

            if (recordsToDelete.Any())
            {
                db.InventoryBalances.RemoveRange(recordsToDelete);
            }

            if (recordsToAdd.Any())
            {
                await db.InventoryBalances.AddRangeAsync(recordsToAdd);
            }

            // Save changes
            int changesCount = await db.SaveChangesAsync();

            await _dialogService.ShowMessageDialogAsync(
                $"Successfully updated inventory! Added {recordsToAdd.Count} books and removed {recordsToDelete.Count} books.",
                "Inventory Updated");
            HasChanges = false;
            await _navigationService.NavigateBack();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving inventory changes: {ex.Message}");
            await _dialogService.ShowMessageDialogAsync(
                "An error occurred while saving inventory changes.",
                "ERROR");
        }
    }

    private bool CanGoBack(object? sender) => true; // Can always go back.
    public async Task GoBack(object? sender)
    {
        if (!HasChanges)
        {
            ClearState();
            await _navigationService.NavigateBack();
        }
        
            bool shouldStillGoBack = false;
        if (HasChanges)
        {
            shouldStillGoBack = await _dialogService.ShowConfirmationDialogAsync("You have unsaved changes, do you still want to go back without saving?", "Proceed without saving?");
        }
        if (shouldStillGoBack)
        {
            ClearState();
            await _navigationService.NavigateBack();
        }
        else
        {
            return;
        }
    }

    public void ClearState()
    {
        SelectedAvailable = null;
        SelectedBookAtStore = null;
        HasChanges = false;
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

