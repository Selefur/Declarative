using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FortuneTeller
{
    public partial class MainWindow : Window
    {
        // Шляхи до ВСІХ можливих карт (включаючи ті, що можуть бути стартовими)
        private readonly List<string> cardImages = new List<string>
        {
            "Cards/Card0.jpg", "Cards/Card1.jpg", "Cards/Card2.jpg",
            "Cards/Card3.jpg", "Cards/Card4.jpg", "Cards/Card5.jpg",
            "Cards/Card6.jpg", "Cards/Card7.jpg", "Cards/Card8.jpg",
            "Cards/Card01.jpg", "Cards/Card02.jpg", "Cards/Card03.jpg"
        };

        // Шляхи до ТРЬОХ карт, які показуються СПОЧАТКУ
        private readonly List<string> defaultCards = new List<string>
        {
            "Cards/Card01.jpg", "Cards/Card02.jpg", "Cards/Card03.jpg"
        };

        // Список категорій питань
        private readonly List<string> categories = new List<string>
        {
            "Гроші", "Сім'я", "Кар'єра", "Хобі", "Любов", "Здоров'я", "Подорожі"
        };

        // Можливі варіанти відповідей
        private readonly List<string> predictionPhrases = new List<string>
        {
            "Так", "Ні", "Скоріше так", "Скоріше ні", "Можливо", "Спитай пізніше"
        };

        private readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            LoadDefaultCards();
            PopulateCategories();
            ClearPrediction();
        }

        private void LoadDefaultCards()
        {
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
                var placeholder = new Image { Height = 500, Margin = new Thickness(5), Stretch = System.Windows.Media.Stretch.Uniform };
                CardGrid.Children.Add(placeholder);
                if (CardGrid.Children.Count >= 3) break;
            }
            ClearPrediction();
        }

        private void PopulateCategories()
        {
            CategoryComboBox.ItemsSource = categories;
            if (categories.Count > 0)
            {
                CategoryComboBox.SelectedIndex = 0;
            }
        }

        // Обробник натискання кнопки "Передбачення"
        private void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            // Перевірка, чи введено ім'я
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я перед тим, як отримати передбачення.",
                                "Ім'я не введено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Вихід з методу, якщо ім'я не введено
            }

            // Якщо ім'я введено, продовжуємо виконання:
            CardGrid.Children.Clear();

            var availableCards = cardImages.Except(defaultCards).ToList();

            var selectedCards = new HashSet<string>();
            while (selectedCards.Count < 3)
            {
                // Перевірка на випадок, якщо availableCards стане порожнім (малоймовірно при попередній перевірці, але безпечніше)
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

            ShowPrediction();
        }

        private void ShowPrediction()
        {
            if (predictionPhrases.Count > 0)
            {
                string prediction = predictionPhrases[random.Next(predictionPhrases.Count)];
                PredictionTextBlock.Text = $"{NameTextBox.Text}, твоя відповідь: {prediction}"; // Додаємо ім'я до відповіді
            }
            else
            {
                PredictionTextBlock.Text = "Немає фраз для передбачення.";
            }
        }

        private void ClearPrediction()
        {
            PredictionTextBlock.Text = string.Empty;
        }

        private Image CreateImage(string path)
        {
            try
            {
                return new Image
                {
                    Source = new BitmapImage(new Uri(path, UriKind.Relative)),
                    Height = 500,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{path}': {ex.Message}");
                return new Image { Height = 500, Margin = new Thickness(5) };
            }
        }
    }
}