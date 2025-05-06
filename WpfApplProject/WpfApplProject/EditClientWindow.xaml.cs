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
        private readonly Client _clientToEdit; 

        public EditClientWindow(Client client)
        {
            InitializeComponent();
            if (client == null)
            {
                MessageBox.Show("Помилка: Не передано дані клієнта для редагування.", "Критична помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close(); 
                return;
            }
            _clientToEdit = client;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadQuestionsAsync();
            PopulateFields();          
            NameTextBox.Focus();
        }

        // Метод для заповнення полів даними з _clientToEdit
        private void PopulateFields()
        {
            if (_clientToEdit != null)
            {
                NameTextBox.Text = _clientToEdit.Name;
                AnswerTextBox.Text = _clientToEdit.Answer;
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

                    if (_clientToEdit.IDQuestion.HasValue) 
                    {
                        if (questions.Any(q => q.Id == _clientToEdit.IDQuestion.Value))
                        {
                            QuestionComboBox.SelectedValue = _clientToEdit.IDQuestion.Value;
                        }
                        else
                        {
                            
                            if (questions.Any()) QuestionComboBox.SelectedIndex = 0;
                            MessageBox.Show("Попередження: Категорія питання, що була раніше вибрана для цього клієнта, не знайдена. Виберіть нову.", "Питання не знайдено", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else if (questions.Any())
                    {
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
                    dbContext.Client.Attach(_clientToEdit);

                    _clientToEdit.Name = NameTextBox.Text.Trim();
                    _clientToEdit.IDQuestion = (int)QuestionComboBox.SelectedValue;
                    _clientToEdit.Answer = AnswerTextBox.Text.Trim();

                    await dbContext.SaveChangesAsync();

                    this.DialogResult = true;
                }
            }
            catch (DbUpdateConcurrencyException dbConcEx) 
            {
                Console.WriteLine($"Database Save Error (DbUpdateConcurrencyException): {dbConcEx.ToString()}");
                MessageBox.Show("Помилка збереження: Дані було змінено іншим користувачем після того, як ви їх завантажили. Перезавантажте дані та спробуйте ще раз.",
                                "Конфлікт даних", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.DialogResult = false; 
            }
            catch (DbUpdateException dbEx) 
            {
                Console.WriteLine($"Database Save Error (DbUpdateException): {dbEx.ToString()}");
                MessageBox.Show($"Помилка збереження даних: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"General Save Error: {ex.ToString()}");
                MessageBox.Show($"Помилка збереження даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                e.Handled = true;
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