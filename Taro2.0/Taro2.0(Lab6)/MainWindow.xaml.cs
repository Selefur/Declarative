using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Data.Entity;
using Taro2._0_Lab6_; 
using System.Threading.Tasks;
using System.Windows.Media;

namespace FortuneTeller
{
    public partial class MainWindow : Window
    {
        private readonly List<string> cardImages = new List<string>
        {
            "Cards/Card0.jpg", "Cards/Card1.jpg", "Cards/Card2.jpg",
            "Cards/Card3.jpg", "Cards/Card4.jpg", "Cards/Card5.jpg",
            "Cards/Card6.jpg", "Cards/Card7.jpg", "Cards/Card8.jpg",
            "Cards/Card01.jpg", "Cards/Card02.jpg", "Cards/Card03.jpg"
        };
        private readonly List<string> defaultCards = new List<string>
        {
            "Cards/Card01.jpg", "Cards/Card02.jpg", "Cards/Card03.jpg"
        };
        private readonly List<string> predictionPhrases = new List<string>
        {
            "Так", "Ні", "Скоріше так", "Скоріше ні", "Можливо", "Спитай пізніше"
        };
        private readonly Random random = new Random();
        private List<Question> loadedCategories = new List<Question>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDefaultCards();
            await LoadCategoriesFromDbAsync();
            ClearPrediction();
            // Initial load for history tab if it's selected first (or just always load)
            await LoadClientHistoryAsync();
        }

        private async Task LoadCategoriesFromDbAsync()
        {
            try
            {
                using (var dbContext = new TaroEntities()) // Replace TaroEntities with your actual context name
                {
                    loadedCategories = await dbContext.Question.OrderBy(q => q.Id).ToListAsync();
                }

                // Check if CategoryComboBox exists before using it
                if (CategoryComboBox != null)
                {
                    CategoryComboBox.ItemsSource = loadedCategories;
                    if (loadedCategories.Count > 0)
                    {
                        CategoryComboBox.SelectedIndex = 0;
                    }
                    CategoryComboBox.IsEnabled = true; // Make sure it's enabled if loading succeeds
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій з бази даних: {ex.Message}\n\nПеревірте рядок підключення та доступність бази даних.",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);

                // Disable relevant controls if categories fail to load
                if (CategoryComboBox != null) CategoryComboBox.IsEnabled = false;
                if (PredictButton != null) PredictButton.IsEnabled = false;
            }
        }

        private async Task LoadClientHistoryAsync()
        {
            // Check if ClientHistoryGrid exists before proceeding
            if (ClientHistoryGrid == null) return;

            try
            {
                List<Client> history;
                using (var dbContext = new TaroEntities()) // Replace TaroEntities with your actual context name
                {
                    // Eagerly load the related Question entity
                    history = await dbContext.Client
                                          .Include(c => c.Question) // Include related Question data
                                          .OrderByDescending(c => c.Id)
                                          .ToListAsync();
                }
                ClientHistoryGrid.ItemsSource = history;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії з бази даних: {ex.Message}",
                               "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                // Optionally clear the grid or show an error state
                ClientHistoryGrid.ItemsSource = null;
            }
        }


        private async void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я.", "Ім'я не введено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть категорію питання.", "Категорія не обрана", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Question selectedQuestion = CategoryComboBox.SelectedItem as Question;
            if (selectedQuestion == null)
            {
                MessageBox.Show("Помилка отримання вибраної категорії.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if CardGrid exists
            if (CardGrid == null) return;

            CardGrid.Children.Clear();
            var availableCards = cardImages.Except(defaultCards).ToList();

            var selectedCards = new HashSet<string>();
            while (selectedCards.Count < 3)
            {
                if (availableCards.Count == 0) break; // Should not happen with the check above, but safe
                int randomIndex = random.Next(availableCards.Count);
                string chosenCard = availableCards[randomIndex];
                selectedCards.Add(chosenCard);
                availableCards.RemoveAt(randomIndex);
            }

            foreach (var cardPath in selectedCards)
            {
                CardGrid.Children.Add(CreateImage(cardPath));
            }

            string prediction = GetPredictionPhrase();
           
            // Check if PredictionTextBlock exists
            if (PredictionTextBlock != null)
            {
                PredictionTextBlock.Text = $"{NameTextBox.Text}, твоя відповідь: {prediction}";
            }

            // Save to DB
            await SavePredictionToDbAsync(NameTextBox.Text, selectedQuestion.Id, prediction);

            // Refresh history if the history tab is currently selected
            if (MainTabControl != null && MainTabControl.SelectedIndex == 1)
            {
                await LoadClientHistoryAsync();
            }
        }


        private string GetPredictionPhrase()
        {
            if (predictionPhrases.Count > 0)
            {
                int randomIndex = random.Next(predictionPhrases.Count);
                return predictionPhrases[randomIndex];
            }
            return "Не вдалося знайти фразу."; // Return a default error message or handle appropriately
        }


        private async Task SavePredictionToDbAsync(string clientName, int questionId, string answer)
        {
            try
            {
                using (var dbContext = new TaroEntities()) // Replace TaroEntities with your actual context name
                {
                    // Determine the next available ID. This can be prone to race conditions
                    // if multiple instances run concurrently. Databases often handle this better
                    // with IDENTITY columns. If your ID is auto-incrementing, remove this logic.
                    int nextId = 1;
                    if (await dbContext.Client.AnyAsync()) // Check if there are any clients
                    {
                        nextId = await dbContext.Client.MaxAsync(c => c.Id) + 1;
                    }


                    var newClientEntry = new Client
                    {
                        Id = nextId, // Assign the manually calculated ID
                        Name = clientName.Trim(), // Trim whitespace
                        IDQuestion = questionId,
                        Answer = answer
                    };

                    dbContext.Client.Add(newClientEntry);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the full exception details if possible (for debugging)
                // Consider using a logging framework
                Console.WriteLine($"Database Save Error: {ex.ToString()}");

                MessageBox.Show($"Помилка збереження передбачення в базу даних: {ex.Message}",
                               "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LoadDefaultCards()
        {
            if (CardGrid == null) return;

            CardGrid.Children.Clear();
            foreach (var cardPath in defaultCards)
            {
                // Basic validation if the path exists in the main list
                if (!string.IsNullOrEmpty(cardPath) && cardImages.Contains(cardPath))
                {
                    CardGrid.Children.Add(CreateImage(cardPath));
                }
                else
                {
                    // Log or handle the case where a default card path is invalid
                    Console.WriteLine($"Warning: Default card path '{cardPath}' is invalid or missing from cardImages list.");
                    // Optionally add a placeholder image
                    var placeholder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Width = 100, Height = 150, Margin = new Thickness(5) };
                    CardGrid.Children.Add(placeholder);
                }
            }
            // Ensure exactly 3 elements (cards or placeholders) if needed
            while (CardGrid.Children.Count < 3)
            {
                var placeholder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Width = 100, Height = 150, Margin = new Thickness(5) };
                CardGrid.Children.Add(placeholder);
                if (CardGrid.Children.Count >= 3) break; // Safety break
            }
            ClearPrediction();
        }

        private void ClearPrediction()
        {
            if (PredictionTextBlock != null)
            {
                PredictionTextBlock.Text = string.Empty;
            }
        }

        private Image CreateImage(string path)
        {
            try
            {
                return new Image
                {
                    Source = new BitmapImage(new Uri(path, UriKind.Relative)), // Assuming paths are relative to executable
                    Height = 450, // Consider making this dynamic or binding it
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                // Log error or show placeholder
                Console.WriteLine($"Error loading image '{path}': {ex.Message}");
                // Return a placeholder or default image
                return new Image { Height = 450, Margin = new Thickness(5) /*, Source = placeholderBitmap */ };
            }
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure the event source is the TabControl itself, not an element within it
            if (e.OriginalSource == MainTabControl)
            {
                // Check if the selected tab is the History tab (index 1) and the grid exists
                if (MainTabControl.SelectedIndex == 1 && ClientHistoryGrid != null)
                {
                    await LoadClientHistoryAsync();
                }
            }
        }

        private async void DeleteAllHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Confirmation dialog
            var result = MessageBox.Show("Ви впевнені, що хочете видалити ВСЮ історію передбачень?",
                                         "Підтвердження видалення",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var dbContext = new TaroEntities())
                    {
                        await dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM Client");
                    }

                    // Refresh the DataGrid after deletion
                    await LoadClientHistoryAsync();

                    MessageBox.Show("Історію передбачень було успішно видалено.",
                                    "Видалення завершено", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка під час видалення історії: {ex.Message}",
                                   "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
     
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}