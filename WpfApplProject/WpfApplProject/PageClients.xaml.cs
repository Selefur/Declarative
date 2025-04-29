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

namespace FortuneTeller
{
    public partial class PageClients : Page
    {
        private bool isDirty = false; // <-- Додано логічне поле

        public PageClients()
        {
            InitializeComponent();
            // Прив'язки команд будуть у XAML
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

        // --- Обробники команд ---

        // Скасувати (Undo)
        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Дозволити скасування, якщо є незбережені зміни
            e.CanExecute = isDirty;
            e.Handled = true; // Позначити як оброблене
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Логіка скасування змін (наприклад, перезавантажити дані)
            MessageBox.Show("Виконано команду Скасувати (Undo)");
            // await LoadClientHistoryAsync(); // Потенційно перезавантажити дані для скасування
            isDirty = false; // Скасування повертає до "чистого" стану
            e.Handled = true;
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

        private void Replace_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Логіка редагування вибраного запису
            MessageBox.Show($"Виконано команду Редагувати (Replace) для елемента: {((Client)ClientHistoryGrid.SelectedItem)?.Name}");
            // Тут може бути код для відкриття форми редагування
            // Після початку редагування:
            // isDirty = true;
            e.Handled = true;
        }

        // Зберегти (Save)
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Дозволити збереження, якщо є незбережені зміни
            e.CanExecute = isDirty;
            e.Handled = true;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Логіка збереження змін
            MessageBox.Show("Виконано команду Зберегти (Save)");
            // Тут буде логіка збереження даних у базу даних
            // try { await _context.SaveChangesAsync(); isDirty = false; } catch ...
            isDirty = false; // Після успішного збереження
            e.Handled = true;
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
            // Логіка пошуку (наприклад, відкрити діалог пошуку)
            MessageBox.Show("Виконано команду Знайти (Find)");
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
            // Логіка видалення вибраного запису
            if (ClientHistoryGrid.SelectedItem is Client selectedClient)
            {
                var result = MessageBox.Show($"Ви впевнені, що хочете видалити запис для '{selectedClient.Name}'?",
                                             "Підтвердження видалення", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show($"Виконано команду Видалити (Delete) для: {selectedClient.Name}");
                    // Логіка видалення з бази даних
                    // try { _context.Client.Remove(selectedClient); await _context.SaveChangesAsync(); await LoadClientHistoryAsync(); } catch...
                    await LoadClientHistoryAsync(); // Оновити список після видалення
                }
            }
            e.Handled = true;
        }

        // Існуючий обробник кнопки "Видалити всю історію" залишається без змін,
        // оскільки команда Delete стосується вибраного елемента.
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
                        // Якщо є обмеження зовнішнього ключа або інші залежності, може знадобитися інший підхід
                        // Наприклад, завантажити всі ID і видалити їх по одному або пачками
                        // Або TRUNCATE TABLE Client, якщо СУБД це підтримує і немає обмежень
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