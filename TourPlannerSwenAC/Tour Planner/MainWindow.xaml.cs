using System.Windows;
using System.Windows.Controls;
using Tour_Planner.Models;
using Tour_Planner.ViewModels;
using log4net.Config;
using log4net;
using Tour_Planner.DAL;
using Tour_Planner.BL;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;


namespace Tour_Planner
{
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        private readonly ITourService _tourService;
        public MainWindow(ITourService tourService)
        {
            XmlConfigurator.Configure();
            Log.Info("Application starting...");

            InitializeComponent();
            _tourService = tourService;
            DataContext = new TourPlannerVM(_tourService);

            ((TourPlannerVM)DataContext).RefreshMap = UpdateMapWebView;
        }

        protected async Task UpdateMapWebView()
        {
            await webView.EnsureCoreWebView2Async(null);
            // Assuming "example.html" is at the root of your project directory and set to "Copy to Output Directory"
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(appDir, "Resources/leaflet.html");
            webView.CoreWebView2.Navigate(filePath);
        }

        private void TourListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tourListBox.SelectedItem != null)
            {
                Tour selectedTour = tourListBox.SelectedItem as Tour;
                Log.Info($"Tour selected: {selectedTour.Name}");
                (DataContext as TourPlannerVM).SelectedTour = selectedTour;
            }
        }

        private void ImportFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                if (DataContext is TourPlannerVM viewModel)
                {
                    viewModel.ImportTourDataCommand.Execute(selectedFileName);
                }
            }
        }
        private void HamburgerMenu_Checked(object sender, RoutedEventArgs e)
        {
            menuItemsPanel.Visibility = Visibility.Visible;
        }

        private void HamburgerMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            menuItemsPanel.Visibility = Visibility.Collapsed;
        }
    }
}