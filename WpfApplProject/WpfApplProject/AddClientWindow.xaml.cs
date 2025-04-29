using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using System.Threading.Tasks;
using WpfApplProject;
using System.Windows.Input;

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
                SaveButton.IsEnabled = false;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // ... (код збереження залишається без змін)
            // 1. Валідація вводу
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

            // 2. Створення та збереження об'єкта Client
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    int nextId = 1;
                    if (await dbContext.Client.AnyAsync())
                    {
                        nextId = await dbContext.Client.MaxAsync(c => c.Id) + 1;
                    }

                    var newClient = new Client
                    {
                        Id = nextId,
                        Name = NameTextBox.Text.Trim(),
                        IDQuestion = (int)QuestionComboBox.SelectedValue,
                        Answer = AnswerTextBox.Text.Trim()
                    };

                    dbContext.Client.Add(newClient);
                    await dbContext.SaveChangesAsync();

                    this.DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Save Error in AddClientWindow: {ex.ToString()}");
                MessageBox.Show($"Помилка збереження нового клієнта: {ex.Message}\n\nДеталі: {ex.InnerException?.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Обробники команди Undo (Скасувати) ---
        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // У цьому вікні скасування завжди можливе
            e.CanExecute = true;
            e.Handled = true;
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false; 
            e.Handled = true;
        }

        // Старий обробник CancelButton_Click тепер не потрібен
        // private void CancelButton_Click(object sender, RoutedEventArgs e)
        // {
        //     this.DialogResult = false;
        // }
    }
}