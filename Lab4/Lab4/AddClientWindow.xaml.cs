using System;
using System.Windows;
using Lab4.Data;

namespace Lab4
{
    public partial class AddClientWindow : Window
    {
        private AdoAssistant _adoAssistant;

        public AddClientWindow()
        {
            InitializeComponent();
            _adoAssistant = new AdoAssistant(); // Ініціалізація класу для роботи з БД
        }

        private void AddClientButton_Click(object sender, RoutedEventArgs e)
        {
            // Зчитуємо введені дані
            int id;
            string name = NameTextBox.Text;
            string phone = PhoneTextBox.Text;
            string address = AddressTextBox.Text;
            decimal orderTotal;

            // Перевірка на коректність введення ID
            if (!int.TryParse(IdTextBox.Text, out id))
            {
                MessageBox.Show("Введіть коректний ID.");
                return;
            }

            // Перевірка на коректність введення суми
            if (!decimal.TryParse(OrderTotalTextBox.Text, out orderTotal))
            {
                MessageBox.Show("Введіть коректну суму замовлення.");
                return;
            }

            // Викликаємо метод для додавання клієнта
            _adoAssistant.AddClient(id, name, phone, address, orderTotal);  // Тепер передаємо ID

            // Закриваємо форму після додавання
            MessageBox.Show("Клієнта успішно додано!");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

    }
}
