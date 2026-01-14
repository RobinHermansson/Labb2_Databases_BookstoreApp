namespace BookStore.Presentation.ViewModels;

public class BookAdministrationViewModel: ViewModelBase
{
	private BookDetails _bookToAdmin;

	public BookDetails BookToAdmin
	{
		get => _bookToAdmin; 
		set 
		{ 
			_bookToAdmin = value;
			RaisePropertyChanged();
		}
	}

	public BookAdministrationViewModel(BookDetails bookToAdmin)
    {
		_bookToAdmin = bookToAdmin;
        
    }
}
