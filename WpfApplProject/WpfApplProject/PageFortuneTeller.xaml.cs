using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfApplProject; 

namespace FortuneTeller
{
    public partial class PageFortuneTeller : Page
    {
        private readonly TaroDBEntities _context = new TaroDBEntities(); 
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
        private readonly Random _random = new Random();
        private List<Question> _loadedCategories = new List<Question>();

        public PageFortuneTeller()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            LoadDefaultCards();
            await LoadCategoriesFromDbAsync();
            ClearPrediction();
            PredictButton.IsEnabled = _loadedCategories.Any(); 
        }

        // --- Copy Methods from original MainWindow.xaml.cs that are needed here ---
        // LoadCategoriesFromDbAsync
        // PredictButton_Click
        // GetPredictionPhrase
        // SavePredictionToDbAsync (Ensure DB Id handling is correct - IDENTITY or manual)
        // LoadDefaultCards
        // ClearPrediction
        // CreateImage

        private async Task LoadCategoriesFromDbAsync()
        {
            try
            {
                _loadedCategories = await _context.Question.OrderBy(q => q.Id).ToListAsync();

                if (CategoryComboBox != null)
                {
                    CategoryComboBox.ItemsSource = _loadedCategories;
                    if (_loadedCategories.Any())
                    {
                        CategoryComboBox.SelectedIndex = 0;
                        CategoryComboBox.IsEnabled = true;
                    }
                    else
                    {
                        CategoryComboBox.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження категорій: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
                if (CategoryComboBox != null) CategoryComboBox.IsEnabled = false;
                if (PredictButton != null) PredictButton.IsEnabled = false;
            }
        }

        private async void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я.", "Ім'я не введено", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть категорію питання.", "Категорія не обрана", MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return;
            }

            if (!(CategoryComboBox.SelectedItem is Question selectedQuestion))
            {
                MessageBox.Show("Помилка отримання вибраної категорії.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PredictButton.IsEnabled = false;
            PredictionTextBlock.Text = "Думаю...";

            // --- Card Logic ---
            CardGrid.Children.Clear();
            var availableCards = cardImages.Except(defaultCards).ToList();
            if (availableCards.Count < 3) { /* Handle error */ LoadDefaultCards(); PredictButton.IsEnabled = true; return; }

            var selectedCards = new HashSet<string>();
            while (selectedCards.Count < 3)
            {
                if (availableCards.Count == 0) break;
                int randomIndex = _random.Next(availableCards.Count);
                selectedCards.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }
            await Task.Delay(200); // Short delay for effect
            foreach (var cardPath in selectedCards)
            {
                CardGrid.Children.Add(CreateImage(cardPath));
            }
            // --- End Card Logic ---


            string prediction = GetPredictionPhrase();
            if (string.IsNullOrEmpty(prediction)) { /* Handle error */ PredictButton.IsEnabled = true; return; }

            await Task.Delay(300); // Short delay for effect
            if (PredictionTextBlock != null)
            {
                PredictionTextBlock.Text = $"{NameTextBox.Text}, твоя відповідь: {prediction}";
            }

            await SavePredictionToDbAsync(NameTextBox.Text, selectedQuestion.Id, prediction);
            PredictButton.IsEnabled = true;
        }

        private string GetPredictionPhrase()
        {
            if (predictionPhrases.Count > 0)
            {
                return predictionPhrases[_random.Next(predictionPhrases.Count)];
            }
            return "Не вдалося знайти фразу.";
        }

        private async Task SavePredictionToDbAsync(string clientName, int questionId, string answer)
        {
    
            try
            {
                using (var dbContext = new TaroDBEntities())
                {
                    int nextId = 1;
                    if (await dbContext.Client.AnyAsync())
                    {
                        nextId = await dbContext.Client.MaxAsync(c => c.Id) + 1;
                    }
                    var newClientEntry = new Client
                    {
                        Id = nextId, // Assign manually calculated ID
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
                 Console.WriteLine($"Database Save Error: {ex.ToString()}");
                 MessageBox.Show($"Помилка збереження передбачення: {ex.Message}", "Помилка бази даних", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private void LoadDefaultCards()
        {
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
                    Console.WriteLine($"Warning: Default card path '{cardPath}' is invalid or missing.");
                    var placeholder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Width = 150, Height = 250, Margin = new Thickness(5), Background = Brushes.DarkGray };
                    CardGrid.Children.Add(placeholder);
                }
            }
            while (CardGrid.Children.Count < 3 && CardGrid.Children.Count > 0)
            {
                var placeholder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1), Width = 150, Height = 250, Margin = new Thickness(5), Background = Brushes.DarkGray };
                CardGrid.Children.Add(placeholder);
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
                // Assumes 'Cards' folder setup as Content / Copy if newer
                Uri imageUri = new Uri(path, UriKind.Relative);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return new Image
                {
                    Source = bitmap,
                    // Height = 450, // Let Viewbox/UniformGrid manage size
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image '{path}': {ex.Message}");
                // Return placeholder if image fails
                return new Image { Height = 400, Margin = new Thickness(5) };
            }
        }

        // Optional: Dispose context if needed, though a new one is created each time
        // private void Page_Unloaded(object sender, RoutedEventArgs e)
        // {
        //     _context?.Dispose();
        // }
    }
}