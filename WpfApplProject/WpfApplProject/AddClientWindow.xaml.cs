using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject;
using System.Windows.Input; // Потрібно для команд
using System.Data.Entity.Infrastructure; // Для DbUpdateException (рекомендовано)

namespace FortuneTeller
{
    public partial class AddClientWindow : Window
    {
        public AddClientWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadQuestionsAsync();
            NameTextBox.Focus();
        }

        private async Task LoadQuestionsAsync()
        {
            // ... (код завантаження питань залишається без змін)
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    var questions = await dbContext.Question.OrderBy(q => q.Name).ToListAsync();
                    QuestionComboBox.ItemsSource = questions;
                    if (questions.Any())
                    {
                        QuestionComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій питань: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                QuestionComboBox.IsEnabled = false;
                // SaveButton.IsEnabled = false; // Тепер керується CanExecute
            }
        }

        // --- Обробники команди Save (Зберегти) ---
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Можна додати просту перевірку, чи є що зберігати (хоча б вибрано питання)
            // Або просто завжди дозволяти спробу збереження, а валідацію робити в Executed.
            // Для простоти, поки що завжди дозволимо.
            e.CanExecute = true; // Завжди можна спробувати зберегти
            e.Handled = true;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1. Валідація вводу (перенесено з SaveButton_Click)
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ім'я клієнта.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return; // Виходимо, не зберігаємо
            }
            if (QuestionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть категорію питання.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuestionComboBox.Focus();
                return; // Виходимо, не зберігаємо
            }
            if (string.IsNullOrWhiteSpace(AnswerTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть відповідь.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                AnswerTextBox.Focus();
                return; // Виходимо, не зберігаємо
            }

            // 2. Створення та збереження об'єкта Client (перенесено з SaveButton_Click)
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    // Генерація нового ID (якщо у вас немає автоінкременту в БД)
                    int nextId = 1;
                    if (await dbContext.Client.AnyAsync())
                    {
                        nextId = await dbContext.Client.MaxAsync(c => c.Id) + 1;
                    }

                    var newClient = new Client
                    {
                        Id = nextId, // Присвоюємо згенерований ID
                        Name = NameTextBox.Text.Trim(),
                        IDQuestion = (int)QuestionComboBox.SelectedValue, // Отримуємо ID вибраного питання
                        Answer = AnswerTextBox.Text.Trim()
                    };

                    dbContext.Client.Add(newClient);
                    await dbContext.SaveChangesAsync(); // Зберігаємо зміни в БД

                    // Сигналізуємо батьківському вікну, що збереження пройшло успішно
                    this.DialogResult = true; // Вікно закриється автоматично
                }
            }
            catch (DbUpdateException dbEx) // Обробка помилок оновлення БД
            {
                Console.WriteLine($"Database Save Error (DbUpdateException): {dbEx.ToString()}");
                MessageBox.Show($"Помилка збереження нового клієнта: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                // Не закриваємо вікно
            }
            catch (Exception ex) // Обробка інших помилок
            {
                Console.WriteLine($"General Save Error: {ex.ToString()}");
                MessageBox.Show($"Помилка збереження нового клієнта: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Не закриваємо вікно
            }
            finally
            {
                e.Handled = true; // Позначаємо команду як оброблену в будь-якому випадку
            }
        }

        // --- Обробники команди Undo (Скасувати) ---
        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            e.Handled = true;
        }

        // Старий обробник SaveButton_Click тепер не потрібен
        // private async void SaveButton_Click(object sender, RoutedEventArgs e) { ... }
    }
}