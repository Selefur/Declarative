using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace FortuneTeller
{
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (this.NavigationService != null && e.Uri != null)
            {
                this.NavigationService.Navigate(e.Uri);
            }
            e.Handled = true;
        }
    }
}