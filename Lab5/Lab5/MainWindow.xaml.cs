using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Windows;
using System.Data.Entity;

namespace Lab5
{
    public partial class MainWindow : Window
    {
        private Lab5Entities1 dbContext;

        public MainWindow()
        {
            InitializeComponent();
            dbContext = new Lab5Entities1();

            LoadData();
        }

        private void LoadData()
        {
            // Завантаження даних у DataGrid
            dbContext.Клієнти.Load();
            dbContext.Компанії.Load();

            ClientsDataGrid.ItemsSource = dbContext.Клієнти.Local.ToBindingList();
            CompaniesDataGrid.ItemsSource = dbContext.Компанії.Local.ToBindingList();
        }
    }
}

