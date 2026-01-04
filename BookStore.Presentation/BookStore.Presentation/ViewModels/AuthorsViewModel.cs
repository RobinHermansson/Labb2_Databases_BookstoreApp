using Bookstore.Infrastructure.Data.Model;
using BookStore.Presentation.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
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

namespace BookStore.Presentation.ViewModels;

internal class AuthorsViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
	private ObservableCollection<AuthorDetails> _authors;
	private ObservableCollection<AuthorDetails> _originalAuthors;

    private Author _affectedAuthor;

    public Author AffectedAuthor
    {
        get 
        { 
            return _affectedAuthor;
        }
        set 
        { 
            _affectedAuthor = value;
            RaisePropertyChanged();
        }
    }


    public DelegateCommand SaveChangesCommand { get; }
	public DelegateCommand ChangeSelectedAuthorCommand { get; }
    public DelegateCommand CancelChangesCommand { get; }

	private bool _hasChanges = false;
	public bool HasChanges 
	{ 
		get => _hasChanges;
		set
		{
			_hasChanges = value;
			RaisePropertyChanged();
            SaveChangesCommand.RaiseCanExecuteChanged();
            CancelChangesCommand.RaiseCanExecuteChanged();
		}
	}

	public ObservableCollection<AuthorDetails> Authors
	{
		get => _authors; 
		set 
		{	
			_authors = value;
			RaisePropertyChanged();
			
		}
	}
    private bool _authorIsSelected = false;
    public bool AuthorIsSelected
    {
        get => _authorIsSelected;
        set
        {
            _authorIsSelected = value;
            RaisePropertyChanged();
        }
    }

	private AuthorDetails? _selectedAuthor;
    public AuthorDetails? SelectedAuthor
    {
        get => _selectedAuthor;
        set
        {
            _selectedAuthor = value;
            RaisePropertyChanged();
            AuthorIsSelected = true;
        }
    }

    public AuthorsViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

		SaveChangesCommand = new DelegateCommand(
            async _ => await SaveChangesAsync(),
            _ => HasChanges
        );
		ChangeSelectedAuthorCommand = new DelegateCommand(ChangeSelectedAuthor);

        CancelChangesCommand = new DelegateCommand(
            _ => CancelChanges(),
            _ => HasChanges
        );
        
    }

    public void ChangeSelectedAuthor(object sender)
    {
        if (SelectedAuthor is null)
        {
            Debug.WriteLine("The selected author was null in 'ChangeSelectedAuthor' call.");
            return;
        }

        AffectedAuthor = new Author() { Id = SelectedAuthor.Id, FirstName = SelectedAuthor.FirstName, LastName = SelectedAuthor.FirstName, BirthDate = SelectedAuthor.BirthDate, DeathDate = SelectedAuthor.DeathDate };
        _mainWindowViewModel.CurrentView = new AddOrUpdateAuthorViewModel(AffectedAuthor);

        
    }

    public async Task LoadAllAuthors()
	{
		using var db = new BookstoreDBContext();

		var tempList = await db.Authors
            .Include(a => a.BookIsbn13s)
            .Select(a => new AuthorDetails()
			{
                Id = a.Id,
				FirstName = a.FirstName,
				LastName = a.LastName,
				BirthDate = a.BirthDate ?? new DateOnly(),
				DeathDate = a.DeathDate ?? new DateOnly(),
				BooksIsbn13 = string.Join(", ", a.BookIsbn13s.Select(b => b.Isbn13)),
				BookTitles = string.Join(", ", a.BookIsbn13s.Select(b => b.Title))
			}).ToListAsync();
		Authors = new ObservableCollection<AuthorDetails>(tempList);

        _originalAuthors = new ObservableCollection<AuthorDetails>(
                tempList.Select(a => new AuthorDetails
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    BirthDate = a.BirthDate,
                    DeathDate = a.DeathDate,
                }).ToList()
                );
        HasChanges = false;

        Debug.WriteLine($"Original authorlist count: {_originalAuthors.Count()}");


	}
    private async Task SaveChangesAsync()
    {
        Debug.WriteLine("AuthorsViewModel: Saving changes to database");

        if (!ValidateChanges())
        {
            Debug.WriteLine("AuthorsViewModel: Validation failed");
            return;
        }

        try
        {
            using var db = new BookstoreDBContext();

            foreach (var authorDetails in Authors)
            {
                if (authorDetails.Id == 0) // New author
                {
                    var newAuthor = new Author
                    {
                        FirstName = authorDetails.FirstName,
                        LastName = authorDetails.LastName,
                        BirthDate = authorDetails.BirthDate == default ? null : authorDetails.BirthDate,
                        DeathDate = authorDetails.DeathDate == default ? null : authorDetails.DeathDate
                    };
                    db.Authors.Add(newAuthor);
                    Debug.WriteLine($"AuthorsViewModel: Adding new author: {newAuthor.FirstName} {newAuthor.LastName}");
                }
                else // Existing author
                {
                    var existingAuthor = await db.Authors.FindAsync(authorDetails.Id);
                    if (existingAuthor != null)
                    {
                        existingAuthor.FirstName = authorDetails.FirstName;
                        existingAuthor.LastName = authorDetails.LastName;
                        existingAuthor.BirthDate = authorDetails.BirthDate == default ? null : authorDetails.BirthDate;
                        existingAuthor.DeathDate = authorDetails.DeathDate == default ? null : authorDetails.DeathDate;
                        Debug.WriteLine($"AuthorsViewModel: Updating author: {existingAuthor.FirstName} {existingAuthor.LastName}");
                    }
                }
            }

            // Handle deletions (authors in original but not in current)
            var deletedIds = _originalAuthors
                .Where(orig => !Authors.Any(curr => curr.Id == orig.Id && orig.Id != 0))
                .Select(a => a.Id)
                .ToList();

            foreach (var deletedId in deletedIds)
            {
                var authorToDelete = await db.Authors.FindAsync(deletedId);
                if (authorToDelete != null)
                {
                    db.Authors.Remove(authorToDelete);
                    Debug.WriteLine($"AuthorsViewModel: Deleting author: {authorToDelete.FirstName} {authorToDelete.LastName}");
                }
            }

            await db.SaveChangesAsync();
            Debug.WriteLine("AuthorsViewModel: Changes saved successfully");

            await LoadAllAuthors();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AuthorsViewModel: Error saving changes - {ex.Message}");
            throw;
        }
    }
    private void CancelChanges()
    {
        Debug.WriteLine("AuthorsViewModel: Cancelling changes");
        _ = LoadAllAuthors();
    }
	private bool ValidateChanges()
    {
        foreach (var author in Authors)
        {
            if (string.IsNullOrWhiteSpace(author.FirstName))
            {
                Debug.WriteLine($"Validation error: First name is required for author ID {author.Id}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(author.LastName))
            {
                Debug.WriteLine($"Validation error: Last name is required for author ID {author.Id}");
                return false;
            }

            if (author.DeathDate != default && author.BirthDate != default && author.DeathDate < author.BirthDate)
            {
                Debug.WriteLine($"Validation error: Death date cannot be before birth date for Author with id: {author.Id} ({author.FirstName} {author.LastName})");
                return false;
            }
        }

        return true;
    }
    private void OnAuthorsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasChanges = true;
        Debug.WriteLine("AuthorsViewModel: Collection changed detected");
    }

    private void OnAuthorPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        HasChanges = true;
        Debug.WriteLine($"AuthorsViewModel: Property {e.PropertyName} changed");
    }
    public class AuthorDetails : INotifyPropertyChanged
    {
        private int _id;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private DateOnly _birthDate;
        private DateOnly _deathDate;
        private string _booksIsbn13 = string.Empty;
        private string _bookTitles = string.Empty;

        public int Id 
        { 
            get => _id; 
            set { _id = value; OnPropertyChanged(); } 
        }
        
        public string FirstName 
        { 
            get => _firstName; 
            set { _firstName = value; OnPropertyChanged(); } 
        }
        
        public string LastName 
        { 
            get => _lastName; 
            set { _lastName = value; OnPropertyChanged(); } 
        }
        
        public DateOnly BirthDate 
        { 
            get => _birthDate; 
            set { _birthDate = value; OnPropertyChanged(); } 
        }
        
        public DateOnly DeathDate 
        { 
            get => _deathDate; 
            set { _deathDate = value; OnPropertyChanged(); } 
        }
        
        public string BooksIsbn13 
        { 
            get => _booksIsbn13; 
            set { _booksIsbn13 = value; OnPropertyChanged(); } 
        }

        public string BookTitles
        {
            get => _bookTitles;
            set { _bookTitles = value; OnPropertyChanged(); }
        }
       
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}

