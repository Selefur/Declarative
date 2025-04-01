using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lab5
{
    public partial class MainWindow : Window
    {
        private Lab5Entities1 db = new Lab5Entities1();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            ClientsDataGrid.ItemsSource = db.Клієнти.ToList();
            CompaniesDataGrid.ItemsSource = db.Компанії.ToList();

            var minExpensesByCompany = db.Клієнти
            .GroupBy(c => c.Код_компанії)  // Групуємо по Код_компанії
            .Select(g => new
            {
                КодКомпанії = g.Key,
                МінімальніВитрати = g.Min(c => c.Витрати)  // Знаходимо мінімальні витрати
            })
            .ToList();

            // Відображаємо результат у DataGrid
            MinExpensesDataGrid.ItemsSource = minExpensesByCompany;
           
            TotalRevenueDataGrid.ItemsSource = db.Клієнти
                .GroupBy(c => c.Код_компанії)
                .Select(g => new
                {
                    КодКомпанії = g.Key,
                    ЗагальніНадходження = g.Sum(c => (decimal?)c.Витрати) ?? 0
                })
                .ToList();

        }

        private void SearchClientButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = ClientNameTextBox.Text.Trim();
            FilteredClientsDataGrid.ItemsSource = db.Клієнти
                .Where(c => c.Назва.Contains(searchText))
                .ToList();
        }
    }
}
