
using BookStore.Presentation.Services;

namespace BookStore.Presentation.ViewModels;

public class BooksInventoryViewModel : ViewModelBase
{

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    public BooksInventoryViewModel(INavigationService navigationService, IDialogService dialogService )
    {
        _navigationService = navigationService;
        _dialogService = dialogService;
    }
}
