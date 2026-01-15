using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace BookStore.Presentation.ViewModels;

public class BookAdministrationViewModel : ViewModelBase
{

    private BookDetails _bookToAdmin;

    private INavigationService _navigationService;
    private IDialogService _dialogService;
    private string _isbn13;
    private string _title;
    private decimal _priceInSek;
    private int _quantity;

    private string _statusText;

    private AuthorMode _currentAuthorMode = AuthorMode.SelectExisting;
    private string _authorFirstName;
    private string _authorLastName;
    private DateOnly? _authorBirthDate;
    private DateOnly? _authorDeathDate;

    public string TitleText { get; set; } = "Edit book:";

    private bool _hasChanges;
    private bool _isLoading;

    private bool _isNewBook;

    public bool IsNewBook
    {
        get => _isNewBook;
        set
        {
            _isNewBook = value;
            RaisePropertyChanged();
        }
    }

    private BookDetails _originalBookDetails;
    private Author _selectedAuthor;
    private Author _originalAuthor;
    private Publisher _originalPublisher;


    private bool _isAuthorEditEnabled;
    private bool _isPublisherEditEnabled;

    public DelegateCommand SaveChangesCommand { get; set; }
    public DelegateCommand CancelChangesCommand { get; set; }
    public DelegateCommand BackToBooksCommand { get; set; }
    private bool CanSaveChanges(object parameter) => HasChanges && !IsLoading;
    private bool CanCancel(object parameter) => HasChanges && !IsLoading;

    public AuthorMode CurrentAuthorMode
    {
        get => _currentAuthorMode;
        set
        {
            _currentAuthorMode = value;
            RaisePropertyChanged();
            OnAuthorModeChanged();
        }
    }

    public bool IsPublisherEditEnabled
    {
        get => _isPublisherEditEnabled;
        set
        {
            _isPublisherEditEnabled = value;
            RaisePropertyChanged();
            CheckForChanges();
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



    public bool IsLoading
    {
        get { return _isLoading; }
        set
        {
            _isLoading = value; RaisePropertyChanged();
            SaveChangesCommand?.RaiseCanExecuteChanged();
            CancelChangesCommand?.RaiseCanExecuteChanged();
            BackToBooksCommand?.RaiseCanExecuteChanged();
        }
    }


    public string ISBN13
    {
        get { return _isbn13; }
        set
        {
            _isbn13 = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string Title
    {
        get { return _title; }
        set
        {
            _title = value;
            RaisePropertyChanged();
            CheckForChanges();
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
                CheckForChanges();
            }
        }
    }


    public decimal PriceInSek
    {
        get { return _priceInSek; }
        set
        {
            _priceInSek = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }


    public int Quantity
    {
        get { return _quantity; }
        set
        {
            _quantity = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    private DateOnly _publicationDate;

    public DateOnly PublicationDate
    {
        get { return _publicationDate; }
        set
        {
            _publicationDate = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string AuthorFirstName
    {
        get => _authorFirstName;
        set
        {
            _authorFirstName = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string AuthorLastName
    {
        get => _authorLastName;
        set
        {
            _authorLastName = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public DateOnly? AuthorBirthDate
    {
        get => _authorBirthDate;
        set
        {
            _authorBirthDate = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public DateOnly? AuthorDeathDate
    {
        get => _authorDeathDate;
        set
        {
            _authorDeathDate = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }
    private ObservableCollection<Publisher> _publisher;

    private Publisher _selectedPublisher;

    public Publisher SelectedPublisher
    {
        get { return _selectedPublisher; }
        set
        {
            _selectedPublisher = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
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

    public Author SelectedAuthor
    {
        get { return _selectedAuthor; }
        set
        {
            _selectedAuthor = value;
            RaisePropertyChanged();
            if (value != null && CurrentAuthorMode == AuthorMode.EditExisting)
            {
                AuthorFirstName = value.FirstName;
                AuthorLastName = value.LastName;
                AuthorBirthDate = value.BirthDate;
                AuthorDeathDate = value.DeathDate;
            }
            CheckForChanges();
        }
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

    public BookDetails BookToAdmin
    {
        get => _bookToAdmin;
        set
        {
            if (_bookToAdmin != null)
            {
                _bookToAdmin.PropertyChanged -= BookToAdmin_PropertyChanged;
            }

            _bookToAdmin = value;
            if (_bookToAdmin != null)
            {
                _bookToAdmin.PropertyChanged += BookToAdmin_PropertyChanged;
            }
            RaisePropertyChanged();
        }
    }
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            RaisePropertyChanged();
        }
    }

    public BookAdministrationViewModel(BookDetails bookToAdmin, INavigationService navigationService, IDialogService dialogService)
    {
        _bookToAdmin = bookToAdmin;
        _navigationService = navigationService;
        _dialogService = dialogService;

        IsNewBook = string.IsNullOrEmpty(bookToAdmin.ISBN13);
        if (IsNewBook)
        {
            TitleText = "Create a new book:";
        }

        SaveChangesCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
        CancelChangesCommand = new DelegateCommand(CancelChanges, CanCancel);
        BackToBooksCommand = new DelegateCommand(GoBack, CanGoBack);

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

        var publishers = await db.Publishers.ToListAsync();
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
                    var existingBooksInventoryBalance = existingBook.InventoryBalances.FirstOrDefault(ib => ib.StoreId == BookToAdmin.BookStoreId);
                    if (existingBooksInventoryBalance is not null)
                    {
                        Quantity = existingBooksInventoryBalance.Quantity;
                    }
                }
            }
        }
        _originalBookDetails = new BookDetails
        {
            ISBN13 = BookToAdmin.ISBN13,
            Title = BookToAdmin.Title,
            Language = BookToAdmin.Language,
            PriceInSek = BookToAdmin.PriceInSek,
            PublicationDate = BookToAdmin.PublicationDate,
            Quantity = BookToAdmin.Quantity
        };

        if (SelectedAuthor != null)
        {
            _originalAuthor = new Author
            {
                Id = SelectedAuthor.Id,
                FirstName = SelectedAuthor.FirstName,
                LastName = SelectedAuthor.LastName,
                BirthDate = SelectedAuthor.BirthDate,
                DeathDate = SelectedAuthor.DeathDate
            };
        }

        if (SelectedPublisher != null)
        {
            _originalPublisher = new Publisher
            {
                Id = SelectedPublisher.Id,
                Name = SelectedPublisher.Name,
                Address = SelectedPublisher.Address,
                Country = SelectedPublisher.Country,
                Email = SelectedPublisher.Email
            };
        }

        ISBN13 = BookToAdmin.ISBN13;
        Title = BookToAdmin.Title;
        PriceInSek = BookToAdmin.PriceInSek;
        SelectedLanguage = BookToAdmin.Language;
        PublicationDate = BookToAdmin.PublicationDate;
        Quantity = BookToAdmin.Quantity;

    }
    private void BookToAdmin_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        CheckForChanges();
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_originalBookDetails != null)
        {
            // Compare against individual ViewModel properties, not BookToAdmin
            if (_originalBookDetails.ISBN13 != ISBN13 ||
                _originalBookDetails.Title != Title ||
                _originalBookDetails.Language != SelectedLanguage ||
                _originalBookDetails.PriceInSek != PriceInSek ||
                _originalBookDetails.PublicationDate != PublicationDate ||
                _originalBookDetails.Quantity != Quantity)
            {
                hasAnyChanges = true;
            }
        }
        switch (CurrentAuthorMode)
        {
            case AuthorMode.SelectExisting:
                // Check if selected author changed from original
                int? currentAuthorId = SelectedAuthor?.Id;
                int? originalAuthorId = _originalAuthor?.Id;
                if (currentAuthorId != originalAuthorId)
                {
                    hasAnyChanges = true;
                }
                break;
                
            case AuthorMode.CreateNew:
                // Any data in author fields indicates a change
                if (!string.IsNullOrEmpty(AuthorFirstName) || 
                    !string.IsNullOrEmpty(AuthorLastName) ||
                    AuthorBirthDate.HasValue ||
                    AuthorDeathDate.HasValue)
                {
                    hasAnyChanges = true;
                }
                break;
                
            case AuthorMode.EditExisting:
                // Check if the author's properties have been modified
                if (_originalAuthor != null && SelectedAuthor != null &&
                    (_originalAuthor.FirstName != AuthorFirstName ||
                     _originalAuthor.LastName != AuthorLastName ||
                     _originalAuthor.BirthDate != AuthorBirthDate ||
                     _originalAuthor.DeathDate != AuthorDeathDate))
                {
                    hasAnyChanges = true;
                }
                break;
        }

        if (IsPublisherEditEnabled && _originalPublisher != null && SelectedPublisher != null)
        {
            if (_originalPublisher.Name != SelectedPublisher.Name ||
                _originalPublisher.Address != SelectedPublisher.Address ||
                _originalPublisher.Country != SelectedPublisher.Country ||
                _originalPublisher.Email != SelectedPublisher.Email)
            {
                hasAnyChanges = true;
            }
        }

        HasChanges = hasAnyChanges;
    }
    private async void SaveChanges(object parameter)
    {
        IsLoading = true;
        try
        {
            using var db = new BookstoreDBContext();
            Book bookToWorkWith; 

            var existingBook = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.InventoryBalances)
                .FirstOrDefaultAsync(b => b.Isbn13 == ISBN13);


            if (existingBook != null)
            {
                existingBook.Title = Title;
                existingBook.Language = SelectedLanguage;
                existingBook.PriceInSek = PriceInSek;
                existingBook.PublicationDate = PublicationDate;
                existingBook.PublisherId = SelectedPublisher?.Id;

                var inventoryBalance = existingBook.InventoryBalances.FirstOrDefault(ib => ib.StoreId == BookToAdmin.BookStoreId);
                if (inventoryBalance != null)
                {
                    inventoryBalance.Quantity = Quantity;
                }

                bookToWorkWith = existingBook;
            }
            else
            {
                var newBook = new Book()
                {
                    Isbn13 = ISBN13,
                    Title = Title,
                    Language = SelectedLanguage,
                    PriceInSek = PriceInSek,
                    PublicationDate = PublicationDate,
                    PublisherId = SelectedPublisher?.Id
                };

                db.Books.Add(newBook);

                if (BookToAdmin.BookStoreId > 0)
                {
                    var inventoryBalance = new InventoryBalance()
                    {
                        Isbn13 = ISBN13,
                        StoreId = BookToAdmin.BookStoreId,
                        Quantity = Quantity
                    };
                    db.InventoryBalances.Add(inventoryBalance);
                }

                bookToWorkWith = newBook;
            }

            switch (CurrentAuthorMode)
            {
                case AuthorMode.SelectExisting:
                    if (SelectedAuthor != null)
                    {
                        bookToWorkWith.Authors.Clear();

                        var authorToAdd = await db.Authors.FindAsync(SelectedAuthor.Id);
                        if (authorToAdd != null)
                        {
                            bookToWorkWith.Authors.Add(authorToAdd);
                        }
                    }
                    break;

                case AuthorMode.CreateNew:
                    if (!string.IsNullOrEmpty(AuthorFirstName) && !string.IsNullOrEmpty(AuthorLastName))
                    {
                        var newAuthor = new Author
                        {
                            FirstName = AuthorFirstName,
                            LastName = AuthorLastName,
                            BirthDate = AuthorBirthDate,
                            DeathDate = AuthorDeathDate
                        };

                        db.Authors.Add(newAuthor);

                        bookToWorkWith.Authors.Clear();
                        bookToWorkWith.Authors.Add(newAuthor);
                    }
                    break;

                case AuthorMode.EditExisting:
                    if (SelectedAuthor != null)
                    {
                        var existingAuthor = await db.Authors.FindAsync(SelectedAuthor.Id);
                        if (existingAuthor != null)
                        {
                            existingAuthor.FirstName = AuthorFirstName;
                            existingAuthor.LastName = AuthorLastName;
                            existingAuthor.BirthDate = AuthorBirthDate;
                            existingAuthor.DeathDate = AuthorDeathDate;
                        }

                        // Make sure the book is associated with this author
                        if (!bookToWorkWith.Authors.Any(a => a.Id == SelectedAuthor.Id))
                        {
                            bookToWorkWith.Authors.Clear();
                            bookToWorkWith.Authors.Add(existingAuthor);
                        }
                    }
                    break;
            }

            if (IsPublisherEditEnabled && SelectedPublisher != null)
            {
                var existingPublisher = await db.Publishers.FindAsync(SelectedPublisher.Id);
                if (existingPublisher != null)
                {
                    existingPublisher.Name = SelectedPublisher.Name;
                    existingPublisher.Address = SelectedPublisher.Address;
                    existingPublisher.Country = SelectedPublisher.Country;
                    existingPublisher.Email = SelectedPublisher.Email;
                }
            }

            await db.SaveChangesAsync();

            StatusText = existingBook != null ? "Updated successfully!" : "Created successfully!";
            HasChanges = false;

            // _navigationService?.NavigateBack();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving: {ex.Message}");
            StatusText = $"Save failed: {ex.Message}";

            // await _dialogService.ShowErrorDialogAsync($"Failed to save: {ex.Message}", "Save Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnAuthorModeChanged()
    {
        switch (CurrentAuthorMode)
        {
            case AuthorMode.SelectExisting:
                AuthorFirstName = SelectedAuthor?.FirstName ?? string.Empty;
                AuthorLastName = SelectedAuthor?.LastName ?? string.Empty;
                AuthorBirthDate = SelectedAuthor?.BirthDate;
                AuthorDeathDate = SelectedAuthor?.DeathDate;
                break;

            case AuthorMode.CreateNew:
                SelectedAuthor = null;
                AuthorFirstName = string.Empty;
                AuthorLastName = string.Empty;
                AuthorBirthDate = null;
                AuthorDeathDate = null;
                break;

            case AuthorMode.EditExisting:
                if (SelectedAuthor != null)
                {
                    AuthorFirstName = SelectedAuthor.FirstName;
                    AuthorLastName = SelectedAuthor.LastName;
                    AuthorBirthDate = SelectedAuthor.BirthDate;
                    AuthorDeathDate = SelectedAuthor.DeathDate;
                }
                break;
        }

        CheckForChanges();
    }
    public async void CancelChanges(object? parameter)
    {
        await LoadRelatedDataAsync();
        HasChanges = false;
        StatusText = "Cancelled whatever you were doing.";
    }

    public async void GoBack(object? sender)
    {
        if (HasChanges)
        {
            bool shouldContinue = await _dialogService.ShowConfirmationDialogAsync("You have unsaved changes. Are you sure you want to go back without saving?",
            "Unsaved Changes");

            if (!shouldContinue)
            {
                return;
            }
        }
        _navigationService.NavigateBack();
    }
    public bool CanGoBack(object? sender)
    {
        return !IsLoading;
    }
}

public enum AuthorMode
{
    SelectExisting,
    CreateNew,
    EditExisting
}
