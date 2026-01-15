
namespace BookStore.Presentation.Services;

public interface IDialogService
    {
        Task<bool> ShowConfirmationDialogAsync(string message, string title = "Confirm");
        Task ShowMessageDialogAsync(string message, string title = "Information");
        //void ShowErrorDialog(string message, string title = "Error");
    }