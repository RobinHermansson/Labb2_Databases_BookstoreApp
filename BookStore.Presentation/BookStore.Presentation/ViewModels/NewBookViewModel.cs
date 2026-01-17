using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using BookStore.Presentation.Shared;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace BookStore.Presentation.ViewModels;

public class NewBookViewModel : ViewModelBase
{
    private BookDetails _bookToAdmin;
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

    public string TitleText { get; set; } = "Create a new book:";

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
    private bool CanSaveChanges(object parameter) => HasChanges && !IsLoading && IsISBN13Valid && IsPriceValid && IsTitleValid && IsAuthorFirstNameValid && IsAuthorLastNameValid && IsPublisherNameValid && IsPublisherAddressValid && IsPublisherCountryValid && IsPublisherEmailValid;
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

            IsISBN13Valid = !string.IsNullOrEmpty(value) && value.Length == 13;
            if (!IsISBN13Valid)
            {
                ValidationErrorText = "ISBN13 must be exactly 13 characters.";
            }
            RaisePropertyChanged(nameof(IsISBN13Valid));
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public bool IsTitleValid { get; private set; } = true;


    public string Title
    {
        get { return _title; }
        set
        {
            _title = value;
            IsTitleValid = !string.IsNullOrEmpty(value);
            if (!IsTitleValid)
            {
                ValidationErrorText = "Title can't be empty.";
            }
            RaisePropertyChanged(nameof(IsTitleValid));
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

    public bool IsPriceValid { get; private set; } = true;

    public decimal PriceInSek
    {
        get { return _priceInSek; }
        set
        {
            _priceInSek = value;
            IsPriceValid = value > 0;
            if (!IsPriceValid)
            {
                ValidationErrorText = "Price must be higher than 0.";
            }
            RaisePropertyChanged(nameof(IsPriceValid));
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

    public bool IsAuthorFirstNameValid { get; set; } = true;
    public string AuthorFirstName
    {
        get => _authorFirstName;
        set
        {
            _authorFirstName = value;
            if (CurrentAuthorMode == AuthorMode.CreateNew || CurrentAuthorMode == AuthorMode.EditExisting)
            {
                IsAuthorFirstNameValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsAuthorFirstNameValid = true;
            }
            RaisePropertyChanged(nameof(IsAuthorFirstNameValid));
            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public bool IsAuthorLastNameValid { get; set; } = true;
    public string AuthorLastName
    {
        get => _authorLastName;
        set
        {
            _authorLastName = value;
            if (CurrentAuthorMode == AuthorMode.CreateNew || CurrentAuthorMode == AuthorMode.EditExisting)
            {
                IsAuthorLastNameValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsAuthorLastNameValid = true;
            }
            RaisePropertyChanged(nameof(IsAuthorLastNameValid));

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

    public bool IsPublisherNameValid { get; set; } = true;
    public string PublisherName
    {
        get => _publisherName;
        set
        {
            _publisherName = value;
            if (CurrentPublisherMode == PublisherMode.CreateNew || CurrentPublisherMode == PublisherMode.EditExisting)
            {
                IsPublisherNameValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsPublisherNameValid = true;
            }
            RaisePropertyChanged(nameof(IsPublisherNameValid));
            RaisePropertyChanged();
            CheckForChanges();
        }
    }
    public bool IsPublisherAddressValid { get; set; } = true;
    public string PublisherAddress
    {
        get => _publisherAddress;
        set
        {
            _publisherAddress = value;
            if (CurrentPublisherMode == PublisherMode.CreateNew || CurrentPublisherMode == PublisherMode.EditExisting)
            {
                IsPublisherAddressValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsPublisherAddressValid = true;
            }
            RaisePropertyChanged(nameof(IsPublisherAddressValid));

            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public bool IsPublisherCountryValid { get; set; } = true;
    public string PublisherCountry
    {
        get => _publisherCountry;
        set
        {
            _publisherCountry = value;
            if (CurrentPublisherMode == PublisherMode.CreateNew || CurrentPublisherMode == PublisherMode.EditExisting)
            {
                IsPublisherCountryValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsPublisherCountryValid = true;
            }
            RaisePropertyChanged(nameof(IsPublisherCountryValid));

            RaisePropertyChanged();
            CheckForChanges();
        }
    }

    public bool IsPublisherEmailValid { get; set; } = true;
    public string PublisherEmail
    {
        get => _publisherEmail;
        set
        {
            _publisherEmail = value;
            if (CurrentPublisherMode == PublisherMode.CreateNew || CurrentPublisherMode == PublisherMode.EditExisting)
            {
                IsPublisherEmailValid = !string.IsNullOrEmpty(value);
            }
            else
            {
                IsPublisherEmailValid = true;
            }
            RaisePropertyChanged(nameof(IsPublisherEmailValid));

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
            if (value != null && CurrentPublisherMode == PublisherMode.EditExisting)
            {
                PublisherName = value.Name;
                PublisherAddress = value.Address;
                PublisherCountry = value.Country;
                PublisherEmail = value.Email;
            }
            CheckForChanges();

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

    public NewBookViewModel(BookDetails bookToAdmin, INavigationService navigationService, IDialogService dialogService)
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

            AvailablePublishers = new ObservableCollection<Publisher>(publishers);
            AvailableAuthors = new ObservableCollection<Author>(authors);

            // Initialize original values (all empty for new book)
            _originalBookDetails = new BookDetails
            {
                ISBN13 = string.Empty,
                Title = string.Empty,
                Language = Language.English,
                PriceInSek = 0,
                PublicationDate = DateOnly.FromDateTime(DateTime.Now),
                Quantity = 0
            };

            // Initialize properties
            ISBN13 = string.Empty;
            Title = string.Empty;
            PriceInSek = 0;
            SelectedLanguage = Language.English;
            PublicationDate = DateOnly.FromDateTime(DateTime.Now);
            Quantity = 0;

            _originalAuthor = null;
            _originalPublisher = null;

            HasChanges = false;
        }
        catch (Exception ex)
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

        // Check book fields
        if (!string.IsNullOrEmpty(ISBN13) ||
            !string.IsNullOrEmpty(Title) ||
            PriceInSek > 0 ||
            Quantity > 0 ||
            SelectedPublisher != null ||
            SelectedAuthor != null)
        {
            hasAnyChanges = true;
        }

        // Check author changes
        switch (CurrentAuthorMode)
        {
            case AuthorMode.SelectExisting:
                if (SelectedAuthor != null)
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
                if (SelectedPublisher != null)
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

            if (string.IsNullOrEmpty(ISBN13) || ISBN13.Length != 13)
            {
                await _dialogService.ShowMessageDialogAsync("ISBN13 must be exactly 13 characters.", "ERROR");
                return;
            }

            var checkExisting = await db.Books.FirstOrDefaultAsync(b => b.Isbn13 == ISBN13);
            if (checkExisting != null)
            {
                await _dialogService.ShowMessageDialogAsync("A book with this ISBN13 already exists.", "ERROR");
                return;
            }

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

            // Handle Author
            switch (CurrentAuthorMode)
            {
                case AuthorMode.SelectExisting:
                    if (SelectedAuthor != null)
                    {
                        newBook.Authors.Clear();
                        var authorToAdd = await db.Authors.FindAsync(SelectedAuthor.Id);
                        if (authorToAdd != null)
                        {
                            newBook.Authors.Add(authorToAdd);
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
                        newBook.Authors.Clear();
                        newBook.Authors.Add(newAuthor);
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
                        newBook.Authors.Clear();
                        newBook.Authors.Add(existingAuthor);
                    }
                    break;
            }

            // Handle Publisher
            switch (CurrentPublisherMode)
            {
                case PublisherMode.SelectExisting:
                    if (SelectedPublisher != null)
                    {
                        var publisherToSwitch = await db.Publishers.FindAsync(SelectedPublisher.Id);
                        if (publisherToSwitch != null)
                        {
                            newBook.Publisher = publisherToSwitch;
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
                        newBook.Publisher = newPublisher;
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

            StatusText = "Book created successfully!";
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
                if (SelectedAuthor is null && AvailableAuthors.Count >= 1 && _originalAuthor is null)
                {
                    SelectedAuthor = AvailableAuthors[0];
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
                if (SelectedPublisher is null && AvailablePublishers.Count >= 1 && _originalPublisher is null)
                {
                    SelectedPublisher = AvailablePublishers[0];
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

