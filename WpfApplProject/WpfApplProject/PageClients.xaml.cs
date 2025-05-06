using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject;
using System.Windows.Input; 
using FortuneTeller.Commands;
using System.Data.Entity.Infrastructure; 

namespace FortuneTeller
{
    public partial class PageClients : Page
    {
        private bool isDirty = false; 

        public PageClients()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClientHistoryAsync();
            isDirty = false;
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
            isDirty = false;
        }


        // Створити (New)
        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private async void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var addClientWindow = new AddClientWindow();

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                addClientWindow.Owner = parentWindow;
            }

            bool? result = addClientWindow.ShowDialog();

            if (result == true)
            {
                await LoadClientHistoryAsync();
                MessageBox.Show("Нового клієнта успішно додано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            e.Handled = true; 
        }

        // Редагувати (Replace)
        private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ClientHistoryGrid != null && ClientHistoryGrid.SelectedItem != null;
            e.Handled = true;
        }

        private async void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ClientHistoryGrid.SelectedItem is Client selectedClient)
            {
                var editWindow = new EditClientWindow(selectedClient);

                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    editWindow.Owner = parentWindow;
                }

                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    await LoadClientHistoryAsync();
                    MessageBox.Show("Дані клієнта успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть рядок для редагування.", "Рядок не вибрано", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            e.Handled = true; 
        }
        // Знайти (Find)
        private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var searchWindow = new SearchClientsWindow(); 

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                searchWindow.Owner = parentWindow;
            }

            searchWindow.Show();

            e.Handled = true;
        }

        // Видалити (Delete)
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ClientHistoryGrid != null && ClientHistoryGrid.SelectedItem != null;
            e.Handled = true;
        }

        private async void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ClientHistoryGrid.SelectedItem is Client selectedClient) 
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити запис для '{selectedClient.Name}' (ID: {selectedClient.Id})?",
                                             "Підтвердження видалення",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var dbContext = new TaroDBEntities())
                        {
                            dbContext.Client.Attach(selectedClient);

                            dbContext.Client.Remove(selectedClient);

                            await dbContext.SaveChangesAsync();
                        } 

                        await LoadClientHistoryAsync();

                        MessageBox.Show("Запис успішно видалено.", "Видалення завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (DbUpdateException dbEx) 
                    {
                        Console.WriteLine($"Database Delete Error (DbUpdateException): {dbEx.ToString()}"); 
                        MessageBox.Show($"Помилка видалення запису з бази даних. Можливо, існують пов'язані дані, які заважають видаленню.\n\nДеталі: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                        "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine($"General Delete Error: {ex.ToString()}"); 
                        MessageBox.Show($"Сталася помилка під час видалення запису: {ex.Message}",
                                       "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true; 
        }

        private async void DeleteAllHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Ви впевнені, що хочете видалити ВСЮ історію передбачень?\nЦю дію неможливо скасувати.",
                                         "Підтвердження видалення",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteAllHistoryButton.IsEnabled = false; 
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
                catch (System.Data.SqlClient.SqlException sqlEx) 
                {
                    MessageBox.Show($"Помилка бази даних під час видалення історії: {sqlEx.Message}\nПеревірте відсутність обмежень або зв'язків, що блокують видалення.",
                                   "Помилка SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex) 
                {
                    MessageBox.Show($"Сталася помилка під час видалення історії: {ex.Message}",
                                    "Загальна помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DeleteAllHistoryButton.IsEnabled = true;
                }
            }
        }
    }
}