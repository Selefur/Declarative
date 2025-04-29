using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject;
using System.Windows.Input;
using System.Data.Entity.Infrastructure; 

namespace FortuneTeller
{
    public partial class EditClientWindow : Window
    {
        private readonly Client _clientToEdit; // Зберігаємо клієнта для редагування

        // Змінений конструктор: приймає Client
        public EditClientWindow(Client client)
        {
            InitializeComponent();
            if (client == null)
            {
                // Це не повинно статися, якщо викликається правильно, але краще перевірити
                MessageBox.Show("Помилка: Не передано дані клієнта для редагування.", "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close(); // Закриваємо вікно, якщо немає даних
                return;
            }
            _clientToEdit = client; // Зберігаємо переданого клієнта
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadQuestionsAsync(); // Завантажуємо питання
            PopulateFields();           // Заповнюємо поля даними
            NameTextBox.Focus();
        }

        // Метод для заповнення полів даними з _clientToEdit
        private void PopulateFields()
        {
            if (_clientToEdit != null)
            {
                NameTextBox.Text = _clientToEdit.Name;
                AnswerTextBox.Text = _clientToEdit.Answer;
                // Вибір правильного питання в ComboBox відбудеться після їх завантаження в LoadQuestionsAsync
            }
        }

        private async Task LoadQuestionsAsync()
        {
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    var questions = await dbContext.Question.OrderBy(q => q.Name).ToListAsync();
                    QuestionComboBox.ItemsSource = questions;

                    // Після завантаження питань, встановлюємо вибране значення
                    if (_clientToEdit.IDQuestion.HasValue) // Перевіряємо, чи є ID питання
                    {
                        // Переконуємось, що таке питання існує в списку
                        if (questions.Any(q => q.Id == _clientToEdit.IDQuestion.Value))
                        {
                            QuestionComboBox.SelectedValue = _clientToEdit.IDQuestion.Value;
                        }
                        else
                        {
                            // Якщо питання клієнта немає в списку (видалено?),
                            // можна вибрати перше або залишити порожнім
                            if (questions.Any()) QuestionComboBox.SelectedIndex = 0;
                            MessageBox.Show("Попередження: Категорія питання, що була раніше вибрана для цього клієнта, не знайдена. Виберіть нову.", "Питання не знайдено", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (questions.Any())
                    {
                        // Якщо у клієнта не було питання, вибираємо перше доступне
                        QuestionComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій питань: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                QuestionComboBox.IsEnabled = false;
            }
        }

        // --- Обробники команди Save (Зберегти) ---
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // Завжди можна спробувати зберегти
            e.Handled = true;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // 1. Валідація вводу (така ж, як при додаванні)
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ім'я клієнта.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }
            if (QuestionComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть категорію питання.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuestionComboBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(AnswerTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть відповідь.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                AnswerTextBox.Focus();
                return;
            }

            // 2. Оновлення існуючого об'єкта Client та збереження
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    // **Дуже важливо: Прикріпити об'єкт до нового контексту**
                    // Це повідомляє EF, що ми працюємо з існуючим записом.
                    dbContext.Client.Attach(_clientToEdit);

                    // **Оновити властивості прикріпленого об'єкта**
                    _clientToEdit.Name = NameTextBox.Text.Trim();
                    _clientToEdit.IDQuestion = (int)QuestionComboBox.SelectedValue;
                    _clientToEdit.Answer = AnswerTextBox.Text.Trim();

                    // Позначити об'єкт як змінений (Attach робить його Unchanged,
                    // оновлення властивостей робить його Modified)
                    // Явно вказувати State не обов'язково, якщо властивості змінено,
                    // але для ясності можна: dbContext.Entry(_clientToEdit).State = EntityState.Modified;

                    // Зберегти зміни (EF згенерує UPDATE)
                    await dbContext.SaveChangesAsync();

                    // Сигналізуємо про успіх
                    this.DialogResult = true;
                }
            }
            catch (DbUpdateConcurrencyException dbConcEx) // Обробка конфліктів паралельного доступу
            {
                Console.WriteLine($"Database Save Error (DbUpdateConcurrencyException): {dbConcEx.ToString()}");
                MessageBox.Show("Помилка збереження: Дані було змінено іншим користувачем після того, як ви їх завантажили. Перезавантажте дані та спробуйте ще раз.",
                                "Конфлікт даних", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.DialogResult = false; // Сигналізуємо, що збереження не вдалося через конфлікт
            }
            catch (DbUpdateException dbEx) // Обробка інших помилок оновлення
            {
                Console.WriteLine($"Database Save Error (DbUpdateException): {dbEx.ToString()}");
                MessageBox.Show($"Помилка збереження даних: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) // Загальні помилки
            {
                Console.WriteLine($"General Save Error: {ex.ToString()}");
                MessageBox.Show($"Помилка збереження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                e.Handled = true; // Позначаємо команду як оброблену
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
    }
}