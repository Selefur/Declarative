using System;
using System.Collections.Generic;
using System.Data.Entity; // Required for Include and ToListAsync
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplProject
{
    /// <summary>
    /// Interaction logic for PageClients.xaml
    /// </summary>
    public partial class PageClients : Page
    {
        private TaroDBEntities _context = new TaroDBEntities(); // The EF context
        private List<Client> _clientsList; // To hold the data for the grid

        public PageClients()
        {
            InitializeComponent();
        }

        // Load data when the page becomes visible
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClientDataAsync();
        }

        private async Task LoadClientDataAsync()
        {
            StatusTextBlock.Text = "Завантаження даних...";
            RecordCountTextBlock.Text = "Записів: ...";
            try
            {
                // Load clients and include related Question data
                _clientsList = await _context.Client
                                          .Include(c => c.Question) // Eager load Question
                                          .OrderByDescending(c => c.Id)
                                          .ToListAsync();

                ClientsDataGrid.ItemsSource = _clientsList; // Bind data to the grid
                StatusTextBlock.Text = "Готово";
                RecordCountTextBlock.Text = $"Записів: {_clientsList.Count}";
            }
            catch (Exception ex)
            {
                // Log the exception details (using a logging framework is better)
                Console.WriteLine($"Error loading client data: {ex.ToString()}");
                StatusTextBlock.Text = "Помилка завантаження даних!";
                RecordCountTextBlock.Text = "Записів: 0";
                MessageBox.Show($"Не вдалося завантажити дані клієнтів: {ex.Message}\n\nПеревірте підключення до бази даних та її структуру.",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally disable controls if loading fails
                ClientsDataGrid.ItemsSource = null;
            }
        }

        // --- Action Method Stubs ---

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Logic to cancel current editing action (if any)
            // For now, maybe just reload data if changes were possible
            StatusTextBlock.Text = "Дію скасовано";
            // Example: Reload data if editing was possible
            // await LoadClientDataAsync();
            MessageBox.Show("Функція 'Скасувати' ще не реалізована.", "Інформація");
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Logic to open a new window/panel to create a new client
            StatusTextBlock.Text = "Створення нового запису...";
            MessageBox.Show("Функція 'Створити' ще не реалізована.", "Інформація");
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client selectedClient)
            {
                // Logic to open a window/panel to edit the selectedClient
                StatusTextBlock.Text = $"Редагування клієнта ID: {selectedClient.Id}...";
                MessageBox.Show($"Функція 'Редагувати' для клієнта {selectedClient.Name} ще не реалізована.", "Інформація");
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть клієнта для редагування.", "Редагування", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusTextBlock.Text = "Оберіть запис для редагування";
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Logic to save changes made (likely after edit/create)
            // Example: await _context.SaveChangesAsync();
            StatusTextBlock.Text = "Збереження змін...";
            MessageBox.Show("Функція 'Зберегти' ще не реалізована.", "Інформація");
            // Remember to reload data after save:
            // await LoadClientDataAsync();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            // Logic to open a search/filter panel
            StatusTextBlock.Text = "Пошук записів...";
            MessageBox.Show("Функція 'Знайти' ще не реалізована.", "Інформація");
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsDataGrid.SelectedItem is Client selectedClient)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити клієнта '{selectedClient.Name}' (ID: {selectedClient.Id})?",
                                             "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    StatusTextBlock.Text = $"Видалення клієнта ID: {selectedClient.Id}...";
                    try
                    {
                        // Find the entity in the context before removing
                        var clientToRemove = await _context.Client.FindAsync(selectedClient.Id);
                        if (clientToRemove != null)
                        {
                            _context.Client.Remove(clientToRemove);
                            await _context.SaveChangesAsync();
                            StatusTextBlock.Text = $"Клієнта ID: {selectedClient.Id} видалено.";
                            await LoadClientDataAsync(); // Refresh the grid
                        }
                        else
                        {
                            StatusTextBlock.Text = "Помилка: Клієнта не знайдено для видалення.";
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting client data: {ex.ToString()}");
                        StatusTextBlock.Text = "Помилка видалення!";
                        MessageBox.Show($"Не вдалося видалити клієнта: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    StatusTextBlock.Text = "Видалення скасовано.";
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть клієнта для видалення.", "Видалення", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusTextBlock.Text = "Оберіть запис для видалення";
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Друк...";
            MessageBox.Show("Функція 'Друк' ще не реалізована.", "Інформація");
        }

        // Optional: Dispose the context when the page is unloaded
        // Usually handled by the scope (e.g. if context is created per operation),
        // but if it's long-lived like this, dispose it.
        // Note: Page_Unloaded might fire during navigation within the frame too.
        // Consider managing context lifetime more carefully (e.g., Dependency Injection).
        // protected override void OnClosed(EventArgs e) // This doesn't exist on Page
        // {
        //     _context?.Dispose();
        //     base.OnClosed(e);
        // }

        // A better place might be if the parent Window is closing,
        // or using a more robust lifetime management strategy.
        // For simplicity here, we'll let the garbage collector handle it,
        // but be aware this isn't ideal for long-running apps.

    }
}