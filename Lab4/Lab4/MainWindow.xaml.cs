using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using Lab4.Data;

namespace Lab4
{
    public partial class MainWindow : Window
    {
        private AdoAssistant myTable;
        private SqlDataAdapter adapter;
        private DataTable dataTable;

        public MainWindow()
        {
            InitializeComponent();
            myTable = new AdoAssistant();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            // Завантажуємо оновлені дані з бази
            dataTable = myTable.TableLoad(out adapter);
            if (dataTable.Rows.Count > 0)
            {
                // Оновлюємо ItemsSource для ListBox
                list.ItemsSource = dataTable.DefaultView;
                list.SelectedIndex = 0; // Вибираємо перший елемент у списку
            }
            else
            {
                MessageBox.Show("Дані не завантажені або таблиця порожня.");
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Відкриваємо вікно для додавання нового клієнта
                AddClientWindow addClientWindow = new AddClientWindow();
                bool? dialogResult = addClientWindow.ShowDialog();  // Використовуємо діалогове вікно

                // Перевірка результату діалогового вікна
                if (dialogResult == true)
                {
                    // Після того, як користувач заповнив форму і натиснув "OK"
                    int id = int.Parse(addClientWindow.IdTextBox.Text);  // Зчитуємо ID
                    string name = addClientWindow.NameTextBox.Text;
                    string phone = addClientWindow.PhoneTextBox.Text;
                    string address = addClientWindow.AddressTextBox.Text;
                    decimal orderTotal = decimal.Parse(addClientWindow.OrderTotalTextBox.Text);

                    // Додаємо новий запис у базу даних
                    myTable.AddClient(id, name, phone, address, orderTotal);

                    // Перезавантажуємо дані після додавання нового клієнта
                }
                LoadData();
            }
            catch (Exception ex)
            {
                // Виведення помилки, якщо сталася помилка
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевірка, чи вибрано клієнта в списку
                if (list.SelectedItem != null)
                {
                    // Отримуємо ID вибраного клієнта
                    DataRowView selectedRow = (DataRowView)list.SelectedItem;
                    int clientId = (int)selectedRow["Id"];

                    // Запит на видалення клієнта з бази даних
                    myTable.DeleteClient(clientId);

                    // Перезавантажуємо дані після видалення
                    LoadData(); // Оновлюємо ItemsSource після видалення
                }
                else
                {
                    MessageBox.Show("Будь ласка, виберіть клієнта для видалення.");
                }
            }
            catch (Exception ex)
            {
                // Виведення помилки, якщо сталася помилка
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Перевірка, чи вибрано клієнта для оновлення
                if (list.SelectedItem != null)
                {
                    // Отримуємо ID вибраного клієнта
                    DataRowView selectedRow = (DataRowView)list.SelectedItem;
                    int clientId = (int)selectedRow["Id"];
                    string name = selectedRow["Name"].ToString();
                    string phone = selectedRow["Phone"].ToString();
                    string address = selectedRow["Address"].ToString();
                    decimal orderTotal = (decimal)selectedRow["OrderTotal"];

                    // Відкриваємо вікно для оновлення даних клієнта
                    UpdateClientWindow updateClientWindow = new UpdateClientWindow(clientId, name, phone, address, orderTotal);
                    bool? dialogResult = updateClientWindow.ShowDialog();  // Використовуємо діалогове вікно

                    // Якщо користувач натиснув "OK"
                    if (dialogResult == true)
                    {
                        // Оновлюємо дані клієнта в базі
                        myTable.UpdateClient(
                            updateClientWindow.ClientId,
                            updateClientWindow.NameTextBox.Text,
                            updateClientWindow.PhoneTextBox.Text,
                            updateClientWindow.AddressTextBox.Text,
                            decimal.Parse(updateClientWindow.OrderTotalTextBox.Text)
                        );

                        // Перезавантажуємо дані після оновлення
                        LoadData();
                    }
                }
                else
                {
                    MessageBox.Show("Будь ласка, виберіть клієнта для оновлення.");
                }
            }
            catch (Exception ex)
            {
                // Виведення помилки, якщо сталася помилка
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

    }
}
