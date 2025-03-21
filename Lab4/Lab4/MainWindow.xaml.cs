using System;
using System.Data;
using System.Windows;
using Lab4.Data;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded; // Додаємо обробник події завантаження вікна
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdoAssistant myTable = new AdoAssistant();
            DataTable data = myTable.TableLoad();

            if (data.Rows.Count > 0) // Перевіряємо, чи є дані
            {
                list.ItemsSource = data.DefaultView; // Перетворюємо DataTable у View
                list.SelectedIndex = 0; // Вибираємо перший елемент у списку
            }
            else
            {
                MessageBox.Show("Дані не завантажені або таблиця порожня.");
            }
        }
    }
}
