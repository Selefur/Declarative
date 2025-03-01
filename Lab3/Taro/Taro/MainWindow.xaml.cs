using System;
using System.Collections.Generic;
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
            "Cards/Card3.jpg"
        };
        private readonly Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            CardGrid.Children.Clear();
            var selectedCards = new HashSet<string>();

            while (selectedCards.Count < 3)
            {
                string card = cardImages[random.Next(cardImages.Count)];
                selectedCards.Add(card);
            }

            foreach (var card in selectedCards)
            {
                Image img = new Image
                {
                    Source = new BitmapImage(new Uri(card, UriKind.Relative)),
                    Height = 150,
                    Stretch = System.Windows.Media.Stretch.Uniform
                };
                CardGrid.Children.Add(img);
            }
        }
    }
}