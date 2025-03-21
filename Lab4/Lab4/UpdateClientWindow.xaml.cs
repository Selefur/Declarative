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
using System.Windows.Shapes;

namespace Lab4
{
    /// <summary>
    /// Interaction logic for UpdateClientWindow.xaml
    /// </summary>
    public partial class UpdateClientWindow : Window
    {
        public int ClientId { get; private set; }

        public UpdateClientWindow(int clientId, string name, string phone, string address, decimal orderTotal)
        {
            InitializeComponent();
            ClientId = clientId;

            IdTextBox.Text = clientId.ToString();
            NameTextBox.Text = name;
            PhoneTextBox.Text = phone;
            AddressTextBox.Text = address;
            OrderTotalTextBox.Text = orderTotal.ToString("0.00");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

}
