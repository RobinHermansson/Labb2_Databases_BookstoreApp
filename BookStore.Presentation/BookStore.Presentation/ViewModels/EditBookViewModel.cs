using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using BookStore.Presentation.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace BookStore.Presentation.ViewModels;

public class EditBookViewModel : ViewModelBase
{
    private BookDetails _bookToAdmin;

    private Book _originalBook;

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private string _isbn13;
    private string _title;
    private decimal _priceInSek;
    private int _quantity;
    private string _statusText;

    // Author properties
    private AuthorMode _currentAuthorMode = AuthorMode.SelectExisting;
    private string _authorFirstName;
    private string _authorLastName;
    private DateOnly? _authorBirthDate;
    private DateOnly? _authorDeathDate;

    // Publisher properties
    private PublisherMode _currentPublisherMode = PublisherMode.SelectExisting;
    private string _publisherName;
    private string _publisherAddress;
    private string _publisherCountry;
    private string _publisherEmail;


    public string TitleText { get; set; } = "Edit book:";

    private bool _hasChanges;
    private bool _isLoading;

    private BookDetails _originalBookDetails;
    private Author _selectedAuthor;
    private Author _originalAuthor;
    private Publisher _selectedPublisher;
    private Publisher _originalPublisher;

    public AsyncDelegateCommand SaveChangesCommand { get; set; }
    public AsyncDelegateCommand CancelChangesCommand { get; set; }
    public AsyncDelegateCommand BackToBooksCommand { get; set; }
    private bool CanSaveChanges(object parameter) => HasChanges && !IsLoading && IsISBN13Valid;
    private bool CanCancel(object parameter) => HasChanges && !IsLoading;

    private string _validationErrorText = string.Empty;
    public string ValidationErrorText
    {
        get => _validationErrorText;
        set
        {
            _validationErrorText = value;
            RaisePropertyChanged();
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
            _isLoading = value;
            RaisePropertyChanged();
            SaveChangesCommand?.RaiseCanExecuteChanged();
            CancelChangesCommand?.RaiseCanExecuteChanged();
            BackToBooksCommand?.RaiseCanExecuteChanged();
        }
    }

    public bool IsISBN13Valid { get; private set; } = true;

    public string ISBN13
    {
        get { return _isbn13; }
        set
        {
            _isbn13 = value;
            RaisePropertyChanged();
            CheckForChanges();

            IsISBN13Valid = !string.IsNullOrEmpty(value) && value.Length == 13;
            if (!IsISBN13Valid)
            {
                ValidationErrorText = "ISBN13 must be exactly 13 characters.";
            }
            RaisePropertyChanged("IsISBN13Valid");
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

    // Author properties
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

    // Publisher properties
    public PublisherMode CurrentPublisherMode
    {
        get => _currentPublisherMode;
        set
        {
            _currentPublisherMode = value;
            RaisePropertyChanged();
            OnPublisherModeChanged();
        }
    }

    public string PublisherName
    {
        get => _publisherName;
        set
        {
            _publisherName = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string PublisherAddress
    {
        get => _publisherAddress;
        set
        {
            _publisherAddress = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string PublisherCountry
    {
        get => _publisherCountry;
        set
        {
            _publisherCountry = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public string PublisherEmail
    {
        get => _publisherEmail;
        set
        {
            _publisherEmail = value;
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

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
    private ObservableCollection<Publisher> _availablePublishers;
    public ObservableCollection<Publisher> AvailablePublishers
    {
        get { return _availablePublishers; }
        set
        {
            _availablePublishers = value;
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

    public ObservableCollection<Book> AvailableBooks { get; set; }

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

    public EditBookViewModel(BookDetails bookToAdmin, INavigationService navigationService, IDialogService dialogService)
    {
        _bookToAdmin = bookToAdmin;
        _navigationService = navigationService;
        _dialogService = dialogService;

        SaveChangesCommand = new AsyncDelegateCommand(SaveChanges, CanSaveChanges);
        CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancel);
        BackToBooksCommand = new AsyncDelegateCommand(GoBack, CanGoBack);
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
        try
        {
            using var db = new BookstoreDBContext();

            var publishers = await db.Publishers.ToListAsync();
            var authors = await db.Authors.ToListAsync();
            var books = await db.Books.ToListAsync();

            AvailablePublishers = new ObservableCollection<Publisher>(publishers);
            AvailableAuthors = new ObservableCollection<Author>(authors);
            AvailableBooks = new ObservableCollection<Book>(books);

            // Set original values FIRST before populating properties to avoid false change detection
            _originalBookDetails = new BookDetails
            {
                ISBN13 = BookToAdmin.ISBN13,
                Title = BookToAdmin.Title,
                Language = BookToAdmin.Language,
                PriceInSek = BookToAdmin.PriceInSek,
                PublicationDate = BookToAdmin.PublicationDate,
                Quantity = BookToAdmin.Quantity
            };

            if (!string.IsNullOrEmpty(_bookToAdmin.ISBN13))
            {
                var existingBook = await db.Books
                    .Include(b => b.Publisher)
                    .Include(b => b.Authors)
                    .Include(b => b.InventoryBalances)
                    .FirstOrDefaultAsync(b => b.Isbn13 == _bookToAdmin.ISBN13);

                if (existingBook != null)
                {
                    // Set original book before loading data to avoid false change detection
                    _originalBook = new Book
                    {
                        Isbn13 = existingBook.Isbn13,
                        Title = existingBook.Title,
                        Language = existingBook.Language,
                        PriceInSek = existingBook.PriceInSek,
                        PublicationDate = existingBook.PublicationDate,
                        PublisherId = existingBook.PublisherId
                    };

                    await LoadBookDataAsync(existingBook);
                    
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
                }
            }

            // Reset HasChanges to false after all initialization is complete
            HasChanges = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading Related data: {ex.Message}");
            await _dialogService.ShowMessageDialogAsync("Error when loading the Book's related data", "ERROR");
        }
    }

    private async Task LoadBookDataAsync(Book book)
    {
        ISBN13 = book.Isbn13;
        Title = book.Title;
        SelectedLanguage = book.Language;
        PriceInSek = book.PriceInSek;
        PublicationDate = book.PublicationDate;

        if (book.PublisherId.HasValue)
        {
            SelectedPublisher = AvailablePublishers.FirstOrDefault(p => p.Id == book.PublisherId);
        }

        using var db = new BookstoreDBContext();
        var bookWithAuthors = await db.Books
            .Include(b => b.Authors)
            .Include(b => b.InventoryBalances)
            .FirstOrDefaultAsync(b => b.Isbn13 == book.Isbn13);

        if (bookWithAuthors != null)
        {
            var bookAuthor = bookWithAuthors.Authors.FirstOrDefault();
            if (bookAuthor != null)
            {
                SelectedAuthor = AvailableAuthors.FirstOrDefault(a => a.Id == bookAuthor.Id);
            }

            var inventoryBalance = bookWithAuthors.InventoryBalances.FirstOrDefault(ib => ib.StoreId == BookToAdmin.BookStoreId);
            if (inventoryBalance != null)
            {
                Quantity = inventoryBalance.Quantity;
            }
            else
            {
                Quantity = 0;
            }
        }

        CheckForChanges();
    }

    private void BookToAdmin_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        CheckForChanges();
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        // Check book changes
        if (_originalBook != null)
        {
            if (_originalBook.Isbn13 != ISBN13 ||
                _originalBook.Title != Title ||
                _originalBook.Language != SelectedLanguage ||
                _originalBook.PriceInSek != PriceInSek ||
                _originalBook.PublicationDate != PublicationDate)
            {
                hasAnyChanges = true;
            }
        }
        if (_originalBookDetails != null && _originalBookDetails.Quantity != Quantity)
        {
            hasAnyChanges = true;
        }

        // Check author changes
        switch (CurrentAuthorMode)
        {
            case AuthorMode.SelectExisting:
                int? currentAuthorId = SelectedAuthor?.Id;
                int? originalAuthorId = _originalAuthor?.Id;
                if (currentAuthorId != originalAuthorId)
                {
                    hasAnyChanges = true;
                }
                break;

            case AuthorMode.CreateNew:
                if (!string.IsNullOrEmpty(AuthorFirstName) ||
                    !string.IsNullOrEmpty(AuthorLastName) ||
                    AuthorBirthDate.HasValue ||
                    AuthorDeathDate.HasValue)
                {
                    hasAnyChanges = true;
                }
                break;

            case AuthorMode.EditExisting:
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

        // Check publisher changes
        switch (CurrentPublisherMode)
        {
            case PublisherMode.SelectExisting:
                int? currentPublisherId = SelectedPublisher?.Id;
                int? originalPublisherId = _originalPublisher?.Id;
                if (currentPublisherId != originalPublisherId)
                {
                    hasAnyChanges = true;
                }
                break;

            case PublisherMode.CreateNew:
                if (!string.IsNullOrEmpty(PublisherName) ||
                    !string.IsNullOrEmpty(PublisherAddress) ||
                    !string.IsNullOrEmpty(PublisherCountry) ||
                    !string.IsNullOrEmpty(PublisherEmail))
                {
                    hasAnyChanges = true;
                }
                break;

            case PublisherMode.EditExisting:
                if (_originalPublisher != null && SelectedPublisher != null &&
                    (_originalPublisher.Name != PublisherName ||
                     _originalPublisher.Address != PublisherAddress ||
                     _originalPublisher.Country != PublisherCountry ||
                     _originalPublisher.Email != PublisherEmail))
                {
                    hasAnyChanges = true;
                }
                break;
        }

        HasChanges = hasAnyChanges;
    }

    private async Task SaveChanges(object parameter)
    {
        IsLoading = true;
        try
        {
            using var db = new BookstoreDBContext();

            if (string.IsNullOrEmpty(ISBN13))
            {
                await _dialogService.ShowMessageDialogAsync("ISBN13 is required.", "ERROR");
                return;
            }

            var bookToWorkWith = await db.Books
                .Include(b => b.Authors)
                .Include(b => b.InventoryBalances)
                .FirstOrDefaultAsync(b => b.Isbn13 == ISBN13);

            if (bookToWorkWith == null)
            {
                await _dialogService.ShowMessageDialogAsync("Book not found in database.", "ERROR");
                return;
            }

            // Update book properties (but NOT ISBN13 as it's the primary key)
            bookToWorkWith.Title = Title;
            bookToWorkWith.Language = SelectedLanguage;
            bookToWorkWith.PriceInSek = PriceInSek;
            bookToWorkWith.PublicationDate = PublicationDate;
            bookToWorkWith.PublisherId = SelectedPublisher?.Id;

            var editInventoryBalance = bookToWorkWith.InventoryBalances.FirstOrDefault(ib => ib.StoreId == BookToAdmin.BookStoreId);
            if (editInventoryBalance != null)
            {
                editInventoryBalance.Quantity = Quantity;
            }
            else if (BookToAdmin.BookStoreId > 0)
            {
                var newInventoryBalance = new InventoryBalance()
                {
                    Isbn13 = bookToWorkWith.Isbn13,
                    StoreId = BookToAdmin.BookStoreId,
                    Quantity = Quantity
                };
                db.InventoryBalances.Add(newInventoryBalance);
            }

            // Handle Author
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

                        if (!bookToWorkWith.Authors.Any(a => a.Id == SelectedAuthor.Id))
                        {
                            bookToWorkWith.Authors.Clear();
                            bookToWorkWith.Authors.Add(existingAuthor);
                        }
                    }
                    break;
            }

            // Handle Publisher
            switch (CurrentPublisherMode)
            {
                case PublisherMode.SelectExisting:
                    if (SelectedPublisher != null)
                    {
                        bookToWorkWith.Publisher = null;
                        var publisherToSwitch = await db.Publishers.FindAsync(SelectedPublisher.Id);
                        if (publisherToSwitch != null)
                        {
                            bookToWorkWith.Publisher = publisherToSwitch;
                        }
                    }
                    break;

                case PublisherMode.CreateNew:
                    if (!string.IsNullOrEmpty(PublisherName))
                    {
                        var newPublisher = new Publisher
                        {
                            Name = PublisherName,
                            Address = PublisherAddress,
                            Country = PublisherCountry,
                            Email = PublisherEmail
                        };
                        db.Publishers.Add(newPublisher);
                        bookToWorkWith.Publisher = newPublisher;
                    }
                    break;

                case PublisherMode.EditExisting:
                    if (SelectedPublisher != null)
                    {
                        var existingPublisher = await db.Publishers.FindAsync(SelectedPublisher.Id);
                        if (existingPublisher != null)
                        {
                            existingPublisher.Name = PublisherName;
                            existingPublisher.Address = PublisherAddress;
                            existingPublisher.Country = PublisherCountry;
                            existingPublisher.Email = PublisherEmail;
                        }
                    }
                    break;
            }

            await db.SaveChangesAsync();

            StatusText = "Book updated successfully!";
            await _dialogService.ShowMessageDialogAsync(StatusText);
            HasChanges = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving: {ex.Message}");
            StatusText = $"Save failed: {ex.Message}";
            await _dialogService.ShowMessageDialogAsync(StatusText, "ERROR!");
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
                if (_originalAuthor != null)
                {
                    SelectedAuthor = AvailableAuthors.FirstOrDefault(a => a.Id == _originalAuthor.Id);
                }
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

    private void OnPublisherModeChanged()
    {
        switch (CurrentPublisherMode)
        {
            case PublisherMode.SelectExisting:
                if (_originalPublisher != null)
                {
                    SelectedPublisher = AvailablePublishers.FirstOrDefault(a => a.Id == _originalPublisher.Id);
                }
                PublisherName = SelectedPublisher?.Name ?? string.Empty;
                PublisherAddress = SelectedPublisher?.Address ?? string.Empty;
                PublisherCountry = SelectedPublisher?.Country ?? string.Empty;
                PublisherEmail = SelectedPublisher?.Email ?? string.Empty;
                break;

            case PublisherMode.CreateNew:
                SelectedPublisher = null;
                PublisherName = string.Empty;
                PublisherAddress = string.Empty;
                PublisherCountry = string.Empty;
                PublisherEmail = string.Empty;
                break;

            case PublisherMode.EditExisting:
                if (SelectedPublisher != null)
                {
                    PublisherName = SelectedPublisher.Name;
                    PublisherAddress = SelectedPublisher.Address;
                    PublisherCountry = SelectedPublisher.Country;
                    PublisherEmail = SelectedPublisher.Email;
                }
                break;
        }

        CheckForChanges();
    }

    public async Task CancelChanges(object? parameter)
    {
        try
        {
            await LoadRelatedDataAsync();
            StatusText = "Cancelled whatever you were doing.";
            await _dialogService.ShowMessageDialogAsync(StatusText);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading related data after cancelling changes: {ex.Message}");
            await _dialogService.ShowMessageDialogAsync("Error when loading the book's related data after cancelling changes.", "ERROR");
        }
        finally
        {
            HasChanges = false;
        }
    }

    public async Task GoBack(object? sender)
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
        await _navigationService.NavigateBack();
    }

    public bool CanGoBack(object? sender)
    {
        return !IsLoading;
    }
}
