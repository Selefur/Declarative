using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Data.Entity; 
using Taro2._0_Lab6_;
using System.Threading.Tasks;


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

        // Список для зберігання завантажених категорій
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
            await LoadClientHistoryAsync();
        }

        private async Task LoadCategoriesFromDbAsync()
        {
            try
            {
                using (var dbContext = new TaroEntities())
                {
                    loadedCategories = await dbContext.Question.OrderBy(q => q.Id).ToListAsync();
                }

                CategoryComboBox.ItemsSource = loadedCategories;
                if (loadedCategories.Count > 0)
                {
                    CategoryComboBox.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій з бази даних: {ex.Message}\n\nПеревірте рядок підключення та доступність бази даних.",
                                "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);

                // Блокуємо функціонал, якщо категорії не завантажились
                // Перевіряємо наявність перед доступом
                if (CategoryComboBox != null)
                {
                    CategoryComboBox.IsEnabled = false;
                }
            }
        }

        private async Task LoadClientHistoryAsync()
        {
            try
            {
                List<Client> history;
                using (var dbContext = new TaroEntities())
                {
                    history = await dbContext.Client
                                          .Include(c => c.Question)
                                          .OrderByDescending(c => c.Id)
                                          .ToListAsync(); 
                }
                // Перевіряємо, чи ClientHistoryGrid існує
                if (ClientHistoryGrid != null)
                {
                    ClientHistoryGrid.ItemsSource = history;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження історії з бази даних: {ex.Message}",
                               "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
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

            CardGrid.Children.Clear();
            var availableCards = cardImages.Except(defaultCards).ToList();

            if (availableCards.Count < 3)
            {
                ClearPrediction();
                MessageBox.Show("Недостатньо унікальних карт для нового передбачення.", "Помилка карт", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadDefaultCards();
                return;
            }

            var selectedCards = new HashSet<string>();
            while (selectedCards.Count < 3)
            {
                if (availableCards.Count == 0) break;
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
            if (string.IsNullOrEmpty(prediction))
            {
                MessageBox.Show("Не вдалося згенерувати передбачення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            PredictionTextBlock.Text = $"{NameTextBox.Text}, твоя відповідь: {prediction}";

            await SavePredictionToDbAsync(NameTextBox.Text, selectedQuestion.Id, prediction);

            if (MainTabControl.SelectedIndex == 1)
            {
                await LoadClientHistoryAsync();
            }
        }

        private string GetPredictionPhrase()
        {
            if (predictionPhrases.Count > 0)
            {
                return predictionPhrases[random.Next(predictionPhrases.Count)];
            }
            return null;
        }

        private async Task SavePredictionToDbAsync(string clientName, int questionId, string answer)
        {
            try
            {
                using (var dbContext = new TaroEntities())
                {
                    int nextId = 1;
                    if (await dbContext.Client.AnyAsync()) 
                    {
                        nextId = await dbContext.Client.MaxAsync(c => c.Id) + 1;
                    }

                    var newClientEntry = new Client
                    {
                        Id = nextId,
                        Name = clientName.Trim(),
                        IDQuestion = questionId,
                        Answer = answer
                    };

                    dbContext.Client.Add(newClientEntry);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження передбачення в базу даних: {ex.Message}",
                               "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDefaultCards()
        {
            // Перевіряємо наявність CardGrid
            if (CardGrid == null) return;

            CardGrid.Children.Clear();
            foreach (var cardPath in defaultCards)
            {
                if (!string.IsNullOrEmpty(cardPath) && cardImages.Contains(cardPath))
                {
                    CardGrid.Children.Add(CreateImage(cardPath));
                }
                else
                {
                    Console.WriteLine($"Warning: Default card path '{cardPath}' is invalid or missing from cardImages list.");
                }
            }
            while (CardGrid.Children.Count < 3)
            {
                var placeholder = new Image { Height = 450, Margin = new Thickness(5), Stretch = System.Windows.Media.Stretch.Uniform };
                CardGrid.Children.Add(placeholder);
                if (CardGrid.Children.Count >= 3) break;
            }
            ClearPrediction();
        }
        private void ClearPrediction()
        {
            // Перевіряємо наявність PredictionTextBlock
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
                    Source = new BitmapImage(new Uri(path, UriKind.Relative)),
                    Height = 450,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{path}': {ex.Message}");
                return new Image { Height = 450, Margin = new Thickness(5) };
            }
        }


        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource == MainTabControl) // Перевіряємо, що подія саме від TabControl
            {
                if (MainTabControl.SelectedIndex == 1 && ClientHistoryGrid != null) // Перевіряємо індекс і наявність грід
                {
                    await LoadClientHistoryAsync();
                }
            }
            e.Handled = true; // Позначаємо подію як оброблену, щоб уникнути можливих проблем з вкладеними елементами
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
        }
    }
}