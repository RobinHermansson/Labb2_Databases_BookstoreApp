using BookStore.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BookStore.Presentation.View
{
    /// <summary>
    /// Interaction logic for BooksInventoryView.xaml
    /// </summary>
    public partial class BooksInventoryView : UserControl
    {
        public BooksInventoryView()
        {
            InitializeComponent();
            Loaded += BooksInventoryViewModel_Loaded;
        }

        private async void BooksInventoryViewModel_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is BooksInventoryViewModel bivm)
                await bivm.InitializeAsync();
        }
    }
}
