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

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
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


    private PublisherMode _currentPublisherMode = PublisherMode.SelectExisting;
    private string _publisherName;
    private string _publisherAddress;
    private string _publisherCountry;
    private string _publisherEmail;

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


    private bool _isPublisherEditEnabled;

    public AsyncDelegateCommand SaveChangesCommand { get; set; }
    public AsyncDelegateCommand CancelChangesCommand { get; set; }
    public AsyncDelegateCommand BackToBooksCommand { get; set; }
    private bool CanSaveChanges(object parameter) => HasChanges && !IsLoading && IsISBN13Valid;

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

    /*
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
    */
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
                ValidationErrorText = "ISBN13 must be exactly 13 letters long.";
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
        catch(Exception ex)
        {
            Debug.WriteLine($"Error when loading Related data: {ex.Message}");
            await _dialogService.ShowMessageDialogAsync("Error when loading the Book's related data", "ERROR");
        }

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
        switch (CurrentPublisherMode)
        {
            case PublisherMode.SelectExisting:
                // Check if selected Publisher changed from original
                int? currentPublisherId = SelectedPublisher?.Id;
                int? originalPublisherId = _originalPublisher?.Id;
                if (currentPublisherId != originalPublisherId)
                {
                    hasAnyChanges = true;
                }
                break;
                
            case PublisherMode.CreateNew:
                // Any data in Publisher fields indicates a change
                if (!string.IsNullOrEmpty(PublisherName) || 
                    !string.IsNullOrEmpty(PublisherAddress) || 
                    !string.IsNullOrEmpty(PublisherCountry) ||
                    !string.IsNullOrEmpty(PublisherEmail)
                    )
                {
                    hasAnyChanges = true;
                }
                break;
                
            case PublisherMode.EditExisting:
                // Check if the Publisher's properties have been modified
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
        /*
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
        */

        HasChanges = hasAnyChanges;
    }
    private async Task SaveChanges(object parameter)
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
                       //Might need to also update the actual publisher with db.Publishers...? 
                    }
                    break;
                }
            /*
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
            */
            await db.SaveChangesAsync();

            StatusText = existingBook != null ? "Updated successfully!" : "Created successfully!";
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
                SelectedAuthor = AvailableAuthors.FirstOrDefault(a => a.Id == _originalAuthor.Id);
                
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
                SelectedPublisher = AvailablePublishers.FirstOrDefault(a => a.Id == _originalPublisher.Id);

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

public enum AuthorMode
{
    SelectExisting,
    CreateNew,
    EditExisting
}

public enum PublisherMode
{
    SelectExisting,
    CreateNew,
    EditExisting
}
