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
            // Встановити початковий стан isDirty (зазвичай false після завантаження)
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
            // Після завантаження/перезавантаження дані вважаються "чистими"
            isDirty = false;
        }


        // Створити (New)
        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Зазвичай, створювати можна завжди
            e.CanExecute = true;
            e.Handled = true;
        }

        private async void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Створюємо екземпляр нового вікна
            var addClientWindow = new AddClientWindow();

            // Встановлюємо власника вікна (для правильної модальної поведінки)
            // Потрібно отримати посилання на головне вікно, в якому знаходиться Page
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                addClientWindow.Owner = parentWindow;
            }

            // Показуємо вікно як модальний діалог (блокує батьківське вікно до закриття)
            bool? result = addClientWindow.ShowDialog();

            // Перевіряємо результат діалогу:
            // Якщо DialogResult == true, значить користувач натиснув "Зберегти" і збереження пройшло успішно
            if (result == true)
            {
                // Оновлюємо список клієнтів у головній таблиці
                await LoadClientHistoryAsync();
                MessageBox.Show("Нового клієнта успішно додано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            // Якщо result == false або null, користувач натиснув "Скасувати" або закрив вікно хрестиком - нічого не робимо

            e.Handled = true; // Позначаємо команду як оброблену
        }

        // Редагувати (Replace)
        private void Replace_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Дозволити редагування, якщо вибрано елемент у таблиці
            e.CanExecute = ClientHistoryGrid != null && ClientHistoryGrid.SelectedItem != null;
            e.Handled = true;
        }

        private async void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1. Отримати вибраний елемент
            if (ClientHistoryGrid.SelectedItem is Client selectedClient)
            {
                // 2. Створити вікно редагування, передавши вибраного клієнта
                var editWindow = new EditClientWindow(selectedClient);

                // 3. Встановити власника
                Window parentWindow = Window.GetWindow(this);
                if (parentWindow != null)
                {
                    editWindow.Owner = parentWindow;
                }

                // 4. Показати вікно як модальний діалог
                bool? result = editWindow.ShowDialog();

                // 5. Перевірити результат і оновити DataGrid, якщо збереження було успішним
                if (result == true)
                {
                    // Зміни було збережено у вікні редагування,
                    // тому просто перезавантажуємо дані в таблиці
                    await LoadClientHistoryAsync();
                    MessageBox.Show("Дані клієнта успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть рядок для редагування.", "Рядок не вибрано", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            e.Handled = true; // Позначаємо команду як оброблену
        }
        // Знайти (Find)
        private void Find_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Зазвичай, шукати можна завжди
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Find_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Створюємо екземпляр нового вікна пошуку
            var searchWindow = new SearchClientsWindow(); // Назва нового вікна

            // Встановлюємо власника (опціонально, але добре для модальності, якщо потрібно)
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                searchWindow.Owner = parentWindow;
            }

            // Показуємо вікно (не модально, щоб можна було працювати з обома)
            searchWindow.Show();
            // Якщо потрібне блокування головного вікна, використовуйте:
            // searchWindow.ShowDialog();

            e.Handled = true;
        }

        // Видалити (Delete)
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Дозволити видалення, якщо вибрано елемент у таблиці
            e.CanExecute = ClientHistoryGrid != null && ClientHistoryGrid.SelectedItem != null;
            e.Handled = true;
        }

        private async void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1. Отримати вибраний елемент
            if (ClientHistoryGrid.SelectedItem is Client selectedClient) // Використовуємо pattern matching
            {
                // 2. Підтвердити видалення у користувача
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити запис для '{selectedClient.Name}' (ID: {selectedClient.Id})?",
                                             "Підтвердження видалення",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // 3. Спробувати видалити запис з бази даних
                    try
                    {
                        // Створюємо новий контекст даних ТІЛЬКИ для цієї операції
                        using (var dbContext = new TaroDBEntities())
                        {
                            // Оскільки selectedClient був завантажений іншим екземпляром контексту,
                            // нам потрібно "прикріпити" його до поточного контексту,
                            // щоб EF знав, який запис у базі даних потрібно видалити.
                            dbContext.Client.Attach(selectedClient);

                            // Позначити об'єкт для видалення
                            dbContext.Client.Remove(selectedClient);

                            // Зберегти зміни в базі даних
                            await dbContext.SaveChangesAsync();
                        } // Контекст автоматично звільняється тут

                        // 4. Якщо видалення пройшло успішно, оновити DataGrid
                        await LoadClientHistoryAsync();

                        MessageBox.Show("Запис успішно видалено.", "Видалення завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (DbUpdateException dbEx) // Обробка помилок оновлення БД (напр., обмеження зовнішнього ключа)
                    {
                        Console.WriteLine($"Database Delete Error (DbUpdateException): {dbEx.ToString()}"); // Логування для налагодження
                        MessageBox.Show($"Помилка видалення запису з бази даних. Можливо, існують пов'язані дані, які заважають видаленню.\n\nДеталі: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                        "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                        // Не перезавантажуємо дані, оскільки видалення не вдалося
                    }
                    catch (Exception ex) // Обробка інших можливих помилок
                    {
                        Console.WriteLine($"General Delete Error: {ex.ToString()}"); // Логування для налагодження
                        MessageBox.Show($"Сталася помилка під час видалення запису: {ex.Message}",
                                       "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        // Не перезавантажуємо дані, оскільки видалення не вдалося
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
                DeleteAllHistoryButton.IsEnabled = false; // Тимчасово вимкнути кнопку
                try
                {
                    using (var deleteContext = new TaroDBEntities())
                    {
                        // Використовуємо ExecuteSqlCommandAsync для ефективного видалення всіх записів
                        await deleteContext.Database.ExecuteSqlCommandAsync("DELETE FROM Client");
                    }

                    // Оновити відображення DataGrid після видалення
                    await LoadClientHistoryAsync(); // isDirty стане false тут

                    MessageBox.Show("Історію передбачень було успішно видалено.",
                                    "Видалення завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Data.SqlClient.SqlException sqlEx) // Обробка специфічних помилок SQL
                {
                    MessageBox.Show($"Помилка бази даних під час видалення історії: {sqlEx.Message}\nПеревірте відсутність обмежень або зв'язків, що блокують видалення.",
                                   "Помилка SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex) // Загальна обробка помилок
                {
                    MessageBox.Show($"Сталася помилка під час видалення історії: {ex.Message}",
                                    "Загальна помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    DeleteAllHistoryButton.IsEnabled = true; // Знову увімкнути кнопку
                }
            }
        }
    }
}