using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lab51
{
    public partial class MainWindow : Window
    {
        private Entities db = new Entities();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            ClientsDataGrid.ItemsSource = db.Клієнти
             .Select(c => new
             {
                c.Назва,
                c.Телефон,
                c.Код_компанії,
                c.Надходження,
                c.Витрати
              })
               .ToList();

            // Вкладка "Компанії"
            CompaniesDataGrid.ItemsSource = db.Компанії
                .Select(g =>new
                {
                    g.Код_компанії,
                    g.Назва_компанії
                })
                .ToList();

            // Вкладка "Мінімальні витрати"
            var minExpensesByCompany = db.Клієнти
                .Where(c => c.Код_компанії != null)
                .GroupBy(c => c.Код_компанії)
                .Select(g => new
                {
                    НазваКомпанії = db.Компанії
                        .Where(comp => comp.Код_компанії == g.Key)

                        .Select(comp => comp.Назва_компанії)
                        .FirstOrDefault(),
                    МінімальніВитрати = g.Min(c => c.Витрати)
                })
                .ToList();

            MinExpensesDataGrid.ItemsSource = minExpensesByCompany;

            // Вкладка "Надходження по компаніях"
            var totalRevenueData = db.Клієнти
                .Where(c => c.Код_компанії != null)
                .GroupBy(c => c.Код_компанії)
                .Select(g => new
                {
                    НазваКомпанії = db.Компанії
                        .Where(comp => comp.Код_компанії == g.Key)
                        .Select(comp => comp.Назва_компанії)
                        .FirstOrDefault(),
                    ЗагальніВитрати = g.Sum(c => c.Витрати)
                })
                .ToList();

            TotalRevenueDataGrid.ItemsSource = totalRevenueData;
        }
            

        // Вкладка "Пошук клієнта"
        private void SearchClientButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = ClientNameTextBox.Text.Trim();
            FilteredClientsDataGrid.ItemsSource = db.Клієнти
                .Where(c => c.Назва.Contains(searchText))
                .Select(c => new
                {
                    c.Назва,
                    c.Телефон,
                    c.Код_компанії,
                    c.Надходження,
                    c.Витрати
                })
                .ToList();
        }
    }
}
