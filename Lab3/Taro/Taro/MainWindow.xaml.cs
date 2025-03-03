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
        private readonly List<string> cardImages = new List<string>
        {
            "Cards/Card0.jpg",
            "Cards/Card1.jpg",
            "Cards/Card2.jpg",
            "Cards/Card3.jpg",
            "Cards/Card4.jpg",
            "Cards/Card5.jpg",
            "Cards/Card6.jpg",
            "Cards/Card7.jpg",
            "Cards/Card8.jpg"
        };

        private readonly Random random = new Random();
        private readonly List<string> defaultCards = new List<string> { "Cards/Card01.jpg", "Cards/Card02.jpg", "Cards/Card03.jpg" };

        public MainWindow()
        {
            InitializeComponent();
            LoadDefaultCards();
        }

        private void LoadDefaultCards()
        {
            CardGrid.Children.Clear();
            foreach (var card in defaultCards)
            {
                CardGrid.Children.Add(CreateImage(card));
            }
        }

        private void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            CardGrid.Children.Clear();

            // Виключаємо стартові картинки з можливих варіантів
            var availableCards = cardImages.Except(defaultCards).ToList();
            var selectedCards = new HashSet<string>();

            while (selectedCards.Count < 3 && availableCards.Count > 0)
            {
                string card = availableCards[random.Next(availableCards.Count)];
                selectedCards.Add(card);
            }

            foreach (var card in selectedCards)
            {
                CardGrid.Children.Add(CreateImage(card));
            }
        }

        private Image CreateImage(string path)
        {
            return new Image
            {
                Source = new BitmapImage(new Uri(path, UriKind.Relative)),
                Height = 600,
                Stretch = System.Windows.Media.Stretch.Uniform
            };
        }
    }
}
