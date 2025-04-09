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
                .Select(c => new {
                    c.Код_компанії,
                    c.Витрати,
                    CompanyName = db.Компанії.FirstOrDefault(comp => comp.Код_компанії == c.Код_компанії).Назва_компанії  
                })
                .GroupBy(c => new { c.Код_компанії, c.CompanyName })
                .Select(g => new
                {
                    НазваКомпанії = g.Key.CompanyName,
                    МінімальніВитрати = g.Min(c => c.Витрати)
                })
                .ToList();

            MinExpensesDataGrid.ItemsSource = minExpensesByCompany;

            var revenueData = db.Клієнти
                .Select(c => new {
                    c.Код_компанії,
                    c.Витрати,
                    CompanyName = db.Компанії.FirstOrDefault(comp => comp.Код_компанії == c.Код_компанії).Назва_компанії  
                })
                .ToList()
                .GroupBy(c => new { c.Код_компанії, c.CompanyName })
                .Select(g => new
                {
                    НазваКомпанії = g.Key.CompanyName,
                    ЗагальніВитрати = g.Sum(c => c.Витрати)
                })
                .ToList();

            TotalRevenueDataGrid.ItemsSource = revenueData;

            var clientsWithCompanies = db.Клієнти
                .Join(db.Компанії,
                    client => client.Код_компанії,
                    company => company.Код_компанії,
                    (client, company) => new
                    {
                        Клієнт = client.Назва,
                        Телефон = client.Телефон,
                        Компанія = company.Назва_компанії, 
                        Витрати = client.Витрати
                    })
                .ToList();

            ClientsWithCompaniesDataGrid.ItemsSource = clientsWithCompanies;
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