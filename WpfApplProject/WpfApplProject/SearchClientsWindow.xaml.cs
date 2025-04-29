using System;
using System.Linq;
using System.Windows;
using System.Data.Entity; // Потрібно для Include, Where, ToListAsync
using System.Threading.Tasks;
using WpfApplProject; // Ваш простір імен з моделлю Client та контекстом

namespace FortuneTeller
{
    public partial class SearchClientsWindow : Window
    {
        public SearchClientsWindow()
        {
            InitializeComponent();
        }

        // Обробник кнопки "Знайти"
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо текст пошуку з TextBox
            string searchText = SearchTextBox.Text.Trim();

            // Якщо текст порожній, можемо очистити результати або нічого не робити
            if (string.IsNullOrEmpty(searchText))
            {
                ResultsDataGrid.ItemsSource = null; // Очищаємо таблицю
                // Або можна показати повідомлення
                // MessageBox.Show("Введіть ім'я для пошуку.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Виконуємо пошук в базі даних
            try
            {
                using (var dbContext = new TaroDBEntities()) // Використовуємо ваш контекст
                {
                    // Виконуємо асинхронний запит до БД
                    var searchResults = await dbContext.Client // Наша таблиця/модель Client
                        .Include(c => c.Question) // Включаємо дані питання для відображення
                        .Where(c => c.Name.Contains(searchText)) // Фільтруємо за іменем (Name)
                                                                 // Примітка: Contains зазвичай чутливий до регістру в SQL Server.
                                                                 // Для нечутливого пошуку краще:
                                                                 // .Where(c => c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .OrderBy(c => c.Name) // Сортуємо результати (опціонально)
                        .ToListAsync(); // Отримуємо список клієнтів

                    // Встановлюємо результати як джерело даних для DataGrid
                    ResultsDataGrid.ItemsSource = searchResults;

                    // Якщо результатів немає, можна повідомити
                    if (!searchResults.Any())
                    {
                        MessageBox.Show("Клієнтів з таким іменем не знайдено.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час пошуку: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                ResultsDataGrid.ItemsSource = null; // Очищаємо у разі помилки
            }
        }

        // Обробник кнопки "Очистити"
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            ResultsDataGrid.ItemsSource = null; // Очищаємо таблицю результатів
        }
    }
}