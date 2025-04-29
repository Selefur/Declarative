using System.Windows;

namespace FortuneTeller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Navigation occurs via the Frame Source and Hyperlinks now
        }

        // Remove Window_Loaded, Window_Closing, MainTabControl_SelectionChanged
        // And all associated logic (LoadCategories, LoadHistory etc.)
        // This logic will move to the respective pages.
    }
}