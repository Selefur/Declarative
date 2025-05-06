using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject; 
using System.Windows.Input; 

namespace FortuneTeller
{
    public partial class SearchClientsWindow : Window
    {
        public SearchClientsWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) => SearchTextBox.Focus();
        }

        // Обробник кнопки "Знайти"
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                ResultsDataGrid.ItemsSource = null;
                return;
            }

            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    var searchResults = await dbContext.Client
                        .Include(c => c.Question)
                        .Where(c => c.Name.Contains(searchText))
                        .OrderBy(c => c.Name)
                        .ToListAsync();

                    ResultsDataGrid.ItemsSource = searchResults;

                    if (!searchResults.Any())
                    {
                        MessageBox.Show("Клієнтів з таким іменем не знайдено.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час пошуку: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                ResultsDataGrid.ItemsSource = null;
            }
        }

        // Обробник кнопки "Очистити"
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            ResultsDataGrid.ItemsSource = null;
        }

        // --- Обробники команди Undo (Скасувати/Закрити) ---
        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
            e.Handled = true;
        }
    }
}