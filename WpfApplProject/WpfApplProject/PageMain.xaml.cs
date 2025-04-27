using System;
using System.Windows.Controls;
using System.Windows.Navigation; // Required for RequestNavigateEventArgs

namespace WpfApplProject
{
    /// <summary>
    /// Interaction logic for PageMain.xaml
    /// </summary>
    public partial class PageMain : Page
    {
        public PageMain()
        {
            InitializeComponent();
        }

        // Handles navigation when a hyperlink is clicked
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // Navigate the frame to the URI specified in the hyperlink
            if (this.NavigationService != null && e.Uri != null)
            {
                this.NavigationService.Navigate(e.Uri);
            }
            // Mark the event as handled
            e.Handled = true;
        }
    }
}