using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject; // Ваш простір імен моделі та контексту
using System.Windows.Input; // Потрібно для команд

namespace FortuneTeller
{
    public partial class SearchClientsWindow : Window
    {
        public SearchClientsWindow()
        {
            InitializeComponent();
            // Можна встановити фокус на поле пошуку при відкритті
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
                        // Розгляньте використання IndexOf для пошуку без урахування регістру:
                        // .Where(c => c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
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
            // Завжди можна закрити вікно пошуку
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Дія - просто закрити вікно
            this.Close();
            e.Handled = true;
        }
    }
}