using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject;

namespace FortuneTeller
{
    public partial class PageClients : Page
    {
   
        public PageClients()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load history when the page loads
            await LoadClientHistoryAsync();
        }

        private async Task LoadClientHistoryAsync()
        {
            try
            {
                using (var dbContext = new TaroDBEntities()) 
                {
                    List<Client> history = await dbContext.Client
                                              .Include(c => c.Question) 
                                              .OrderByDescending(c => c.Id)
                                              .ToListAsync();

                    if (ClientHistoryGrid != null)
                    {
                        ClientHistoryGrid.ItemsSource = history; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                if (ClientHistoryGrid != null) ClientHistoryGrid.ItemsSource = null;
            }
        }

        private async void DeleteAllHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете видалити ВСЮ історію передбачень?\nЦю дію неможливо скасувати.",
                                         "Підтвердження видалення",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var deleteContext = new TaroDBEntities())
                    {
                        await deleteContext.Database.ExecuteSqlCommandAsync("DELETE FROM Client"); 
                    }

                    await LoadClientHistoryAsync();

                    MessageBox.Show("Історію передбачень було успішно видалено.",
                                    "Видалення завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час видалення історії: {ex.Message}"
                                  );
                }
            }
        }

      
    }
}