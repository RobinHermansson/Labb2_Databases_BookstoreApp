
namespace BookStore.Presentation.Services;

public interface IDialogService
    {
        Task<bool> ShowConfirmationDialogAsync(string message, string title = "Confirm");
        //void ShowMessageDialog(string message, string title = "Information");
        //void ShowErrorDialog(string message, string title = "Error");
    }