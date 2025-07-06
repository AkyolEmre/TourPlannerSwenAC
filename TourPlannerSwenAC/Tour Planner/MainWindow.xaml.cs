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
using System.Windows.Input;


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

            // VM erstellen und DataContext setzen
            var vm = new TourPlannerVM(_tourService);
            DataContext = vm;

            // Map-Refresh zuweisen
            vm.RefreshMap = UpdateMapWebView;

            // Unser neues Event abonnieren
            vm.SaveFileDialogRequested += OnSaveFileDialogRequested;
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
                Tour ?selectedTour = tourListBox.SelectedItem as Tour;
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

        private void OnSaveFileDialogRequested(string filter, string defaultFileName, string title)
        {
            var dlg = new SaveFileDialog
            {
                Filter = filter,
                FileName = defaultFileName,
                Title = title
            };

            if (dlg.ShowDialog() != true)
                return;

            // DataContext holen
            if (!(DataContext is TourPlannerVM vm))
                return;

            // Je nach Dateiendung den richtigen Report aufrufen
            if (defaultFileName.EndsWith("_Report.pdf"))
            {
                // hier alle Tours exportieren
                ReportGenerator.GenerateTourReport(
                    vm.Tours,      // IEnumerable<Tour>
                    dlg.FileName   // der vom Dialog gewählte Pfad
                );
            }
            else
            {
                // nur die aktuell ausgewählte Tour
                if (vm.SelectedTour == null)
                {
                    MessageBox.Show("Bitte zuerst eine Tour auswählen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ReportGenerator.GenerateSummarizeReport(
                    vm.SelectedTour,
                    dlg.FileName
                );
            }

            MessageBox.Show("Export erfolgreich!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}