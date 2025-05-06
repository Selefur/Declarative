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
            }
        }

        // --- Обробники команди Save (Зберегти) ---
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; 
            e.Handled = true;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
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
            catch (DbUpdateException dbEx) 
            {
                Console.WriteLine($"Database Save Error (DbUpdateException): {dbEx.ToString()}");
                MessageBox.Show($"Помилка збереження нового клієнта: {dbEx.InnerException?.Message ?? dbEx.Message}",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"General Save Error: {ex.ToString()}");
                MessageBox.Show($"Помилка збереження нового клієнта: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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