using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BookStore.Presentation.ViewModels;

public class AuthorsViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    private ObservableCollection<AuthorDetails> _displayAuthorDetails;
    public List<Author> OriginalListOfAuthors;
    private AuthorDetails _selectedAuthor;
    public AsyncDelegateCommand SaveChangesCommand { get; set; }
    public AsyncDelegateCommand CancelChangesCommand { get; set; }

    private List<AuthorDetails> _newAuthors = new List<AuthorDetails>();
    private List<AuthorDetails> _deletedAuthors = new List<AuthorDetails>();
    private List<AuthorDetails> _changedAuthors = new();

    private bool _hasChanges = false;

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

    public AuthorDetails SelectedAuthor
    {
        get { return _selectedAuthor; }
        set
        {
            _selectedAuthor = value;
            RaisePropertyChanged();
        }
    }


    public AuthorsViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        SaveChangesCommand = new AsyncDelegateCommand(SaveChangesAsync, CanSaveChanges);
        CancelChangesCommand = new AsyncDelegateCommand(CancelChanges, CanCancelChanges);
    }

    private bool CanCancelChanges(object? sender)
    {
        return HasChanges;
    }
    private async Task CancelChanges(object obj)
    {
        try
        {
            await LoadAuthorDetailsAsync();
            await _dialogService.ShowMessageDialogAsync("Successfully cancelled all changes.");
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageDialogAsync("Error when loading authors details after cancelling changes.", "ERROR");
            Debug.WriteLine($"Error when loading authors details after cancelling changes. {ex.Message}");
        }
        finally
        {
            _deletedAuthors.Clear();
            _newAuthors.Clear();
        }
    }

    public ObservableCollection<AuthorDetails> DisplayAuthorDetails
    {
        get { return _displayAuthorDetails; }
        set
        {

            // Unsubscribe from old collection
            if (_displayAuthorDetails != null)
            {
                _displayAuthorDetails.CollectionChanged -= DisplayAuthorDetails_CollectionChanged;
                foreach (var author in _displayAuthorDetails)
                {
                    author.PropertyChanged -= AuthorDetails_PropertyChanged;
                }
            }

            _displayAuthorDetails = value;
            RaisePropertyChanged();

            // Subscribe to new collection
            if (_displayAuthorDetails != null)
            {
                _displayAuthorDetails.CollectionChanged += DisplayAuthorDetails_CollectionChanged;
                foreach (var author in _displayAuthorDetails)
                {
                    author.PropertyChanged += AuthorDetails_PropertyChanged;
                }
            }
        }
    }
    private void DisplayAuthorDetails_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (AuthorDetails newAuthor in e.NewItems)
            {
                newAuthor.PropertyChanged += AuthorDetails_PropertyChanged;
                
                if (newAuthor.Id <= 0)
                {
                    _newAuthors.Add(newAuthor);
                }
            }
        }

        if (e.OldItems != null)
        {
            foreach (AuthorDetails removedAuthor in e.OldItems)
            {
                removedAuthor.PropertyChanged -= AuthorDetails_PropertyChanged;
                
                if (_newAuthors.Contains(removedAuthor))
                {
                    _newAuthors.Remove(removedAuthor);
                }
                else if (removedAuthor.Id > 0)
                {
                    _deletedAuthors.Add(removedAuthor);
                }
            }
        }

        CheckForChanges();
    }
    private void AuthorDetails_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // Only check for changes on properties that matter
        if (e.PropertyName == nameof(AuthorDetails.FirstName) ||
            e.PropertyName == nameof(AuthorDetails.LastName) ||
            e.PropertyName == nameof(AuthorDetails.BirthDate) ||
            e.PropertyName == nameof(AuthorDetails.DeathDate))
        {
            CheckForChanges();
        }
    }

    private void CheckForChanges()
    {
        bool hasAnyChanges = false;

        if (_newAuthors.Any())
        {
            hasAnyChanges = true;
        }

        if (_deletedAuthors.Any())
        {
            hasAnyChanges = true;
        }

        foreach (var displayAuthor in DisplayAuthorDetails)
        {
            var originalAuthor = OriginalListOfAuthors.FirstOrDefault(a => a.Id == displayAuthor.Id);
            if (originalAuthor != null)
            {
                if (originalAuthor.FirstName != displayAuthor.FirstName ||
                    originalAuthor.LastName != displayAuthor.LastName ||
                    originalAuthor.BirthDate != displayAuthor.BirthDate ||
                    originalAuthor.DeathDate != displayAuthor.DeathDate)
                {
                    _changedAuthors.Add(displayAuthor); // For clear state tracking...
                    hasAnyChanges = true;
                    break;
                }
            }
        }

        HasChanges = hasAnyChanges;
    }

    public async Task LoadAuthorDetailsAsync()
    {
        try
        {
            using var db = new BookstoreDBContext();

            var tempList = await db.Authors
                .Include(b => b.BookIsbn13s)
                .Select(a => new AuthorDetails()
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    BirthDate = a.BirthDate ?? new DateOnly(),
                    DeathDate = a.DeathDate ?? new DateOnly(),
                    BooksIsbn13 = string.Join(", ", a.BookIsbn13s.Select(b => b.Isbn13)),
                    BookTitles = string.Join(", ", a.BookIsbn13s.Select(b => b.Title))
                }
                ).ToListAsync();
            OriginalListOfAuthors = await db.Authors.ToListAsync();
            DisplayAuthorDetails = new ObservableCollection<AuthorDetails>(tempList);
            _newAuthors.Clear();
            _deletedAuthors.Clear();
            HasChanges = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error when loading author details directly: {ex.Message}");
            await _dialogService.ShowMessageDialogAsync("Error when loading authors details.", "ERROR");
        }
    }

    private bool CanSaveChanges(object? sender)
    {
        return HasChanges;
    }

    private async Task SaveChangesAsync(object sender)
    {
        try
        {

            using var db = new BookstoreDBContext();

            // Handle new authors
            foreach (var newAuthor in _newAuthors)
            {
                var dbAuthor = new Author
                {
                    FirstName = newAuthor.FirstName,
                    LastName = newAuthor.LastName,
                    BirthDate = newAuthor.BirthDate,
                    DeathDate = newAuthor.DeathDate
                };
                
                db.Authors.Add(dbAuthor);
            }

            // Handle deleted authors
            foreach (var deletedAuthor in _deletedAuthors)
            {
                var dbAuthor = await db.Authors
                .Include(a => a.BookIsbn13s) 
                .FirstOrDefaultAsync(a => a.Id == deletedAuthor.Id);
                if (dbAuthor != null)
                {
                    dbAuthor.BookIsbn13s.Clear();
                    db.Authors.Remove(dbAuthor);
                }
            }
            
            foreach (var changedAuthor in _changedAuthors)
            {
                var dbAuthor = await db.Authors.FindAsync(changedAuthor.Id);

                if (dbAuthor != null)
                {
                    dbAuthor.FirstName = changedAuthor.FirstName;
                    dbAuthor.LastName = changedAuthor.LastName;
                    dbAuthor.BirthDate = changedAuthor.BirthDate;
                    dbAuthor.DeathDate = changedAuthor.DeathDate;
                }
            }

            await db.SaveChangesAsync();
            await _dialogService.ShowMessageDialogAsync("Successfully saved changes.");
            // Refresh states and the list according to new changes.
            HasChanges = false;
            await LoadAuthorDetailsAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving changes {ex}");
            await _dialogService.ShowMessageDialogAsync("Error saving saving changes.", "ERROR");
        }
    }

    public void ClearState()
    {
        SelectedAuthor = null;
        _changedAuthors.Clear();
        _newAuthors.Clear();
        _deletedAuthors.Clear();
        HasChanges = false;
    }
}

public class AuthorDetails : INotifyPropertyChanged
{
    private string _firstName;
    private string _lastName;
    private DateOnly _birthDate;
    private DateOnly? _deathDate;

    public int Id { get; set; }

    public string FirstName
    {
        get => _firstName;
        set
        {
            _firstName = value;
            OnPropertyChanged();
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            OnPropertyChanged();
        }
    }

    public DateOnly BirthDate
    {
        get => _birthDate;
        set
        {
            _birthDate = value;
            OnPropertyChanged();
        }
    }

    public DateOnly? DeathDate
    {
        get => _deathDate;
        set
        {
            _deathDate = value;
            OnPropertyChanged();
        }
    }

    public string BooksIsbn13 { get; set; }
    public string BookTitles { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
