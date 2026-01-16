
namespace BookStore.Presentation.Services;

public interface INavigationService
{
    void NavigateTo(string viewName, string navigatedFrom, object parameter = null);
    Task NavigateBack();
}
