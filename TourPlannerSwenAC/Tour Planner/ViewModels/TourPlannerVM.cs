using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Tour_Planner.Models;
using Tour_Planner.BL;
using log4net;
using System.Windows;
using System.IO;
using System.Text;
using Tour_Planner.DAL;

namespace Tour_Planner.ViewModels
{
    public class TourPlannerVM : INotifyPropertyChanged
    {
        private readonly ITourService _tourService;
        private readonly RouteService _routeService;
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        public delegate Task RefreshMapDelegate();

        private ObservableCollection<Tour> filteredTours;
        private string _searchFilter;

        private Tour selectedTour;
        private Tour tourLogsSelectedTour;
        private TourLog selectedTourLog;

        private string newTourName;
        private string newTourDescr;
        private string newTourFrom;
        private TransportType newTourTransType;
        private string newTourTo;
        private int newTourDistance;
        private int newTourEstTime;

        public DateTime newdateTime;
        public string newcomment;
        public DifficultyLevel newdifficulty;
        public string newtotalDistance;
        public string newtotalTime;
        public int newrating;
        public Action<string, string, string> SaveFileDialogRequested { get; set; }
        public TourPlannerVM(ITourService tourService)
        {
            filteredTours = new ObservableCollection<Tour>();
            _searchFilter = string.Empty;
            selectedTour = new Tour();
            tourLogsSelectedTour = new Tour();
            selectedTourLog = new TourLog();


            newTourName = string.Empty;
            newTourDescr = string.Empty;
            newTourFrom = string.Empty;
            newTourTo = string.Empty;
            newtotalDistance = string.Empty;
            newtotalTime = string.Empty;
            newcomment = string.Empty;

            newTourTransType = TransportType.None;
            newTourDistance = 0;
            newTourEstTime = 0;
            newdateTime = DateTime.Now;
            newdifficulty = DifficultyLevel.None;
            newrating = 0;

            Tours = new ObservableCollection<Tour>();
            RefreshMap = async () => { };

            AddTourCommand = new RelayCommand(o => AddTour());
            UpdateTourCommand = new RelayCommand(o => UpdateTour());
            DeleteTourCommand = new RelayCommand(o => DeleteTour());

            AddTourLogCommand = new RelayCommand(o => AddTourLog());
            DeleteTourLogCommand = new RelayCommand(o => DeleteTourLog());
            SaveTourLogCommand = new RelayCommand(o => SaveTourLog());

            TourReportCommand = new RelayCommand(
                _ =>
                {
                    if (SelectedTour == null)
                    {
                        ShowPopup("Bitte zuerst eine Tour auswählen.");
                        return;
                    }
                    SaveFileDialogRequested?.Invoke(
                        "PDF Files (*.pdf)|*.pdf",
                        $"{SelectedTour.Name}_Report.pdf",
                        "Speicherort für Einzel-Tour-Report wählen"
                    );
                },
                _ => true
            );

            SummarizedTourReportCommand = new RelayCommand(
                _ =>
                {
                    SaveFileDialogRequested?.Invoke(
                        "PDF Files (*.pdf)|*.pdf",
                        $"Summary.pdf",
                        "Speicherort für All-Tours-Report wählen"
                    );
                },
                _ => true
            );

            ExportTourDataCommand = new RelayCommand(o => { ExportTourData(); ShowPopup("Information for Export Tour Data"); });
            ImportTourDataCommand = new RelayCommand(filePath => ImportTourData(filePath));

            ClearValuesCommand = new RelayCommand(o => ClearTourFields());
            ExportDataCommand = new RelayCommand(param => { ExportAllTours((ExportFormat)param); ShowPopup("Information for Export All Tours Data"); });

            _tourService = tourService;
            _routeService = new RouteService();

            NewDateTime = DateTime.Now;
            LoadTours();
        }

        public ICommand TourReportCommand { get; set; }
        public ICommand SummarizedTourReportCommand { get; set; }
        public ICommand ExportTourDataCommand { get; set; }
        public ICommand ImportTourDataCommand { get; set; }

        public ICommand AddTourCommand { get; set; }
        public ICommand UpdateTourCommand { get; set; }
        public ICommand DeleteTourCommand { get; set; }

        public ICommand AddTourLogCommand { get; set; }
        public ICommand DeleteTourLogCommand { get; set; }
        public ICommand SaveTourLogCommand { get; set; }

        public ICommand ClearValuesCommand { get; set; }
        public ICommand ExportDataCommand { get; set; }

        public RefreshMapDelegate RefreshMap { get; set; }

        public void TourReport()
        {
            if (SelectedTour != null)
            {
                SelectedTour.Name = NewTourName;
                SelectedTour.Description = NewTourDescr;
                SelectedTour.From = NewTourFrom;
                SelectedTour.To = NewTourTo;
                SelectedTour.TransportType = NewTourTransType;
                SelectedTour.Distance = NewTourDistance;
                SelectedTour.EstimatedTime = NewTourEstTime;

                Log.Info($"Download TourReport: {SelectedTour.Name}");
                ReportGenerator.GenerateTourReport(SelectedTour);
            }
        }
        public void SummarizedTourReport()
        {
            if (SelectedTour != null)
            {
                SelectedTour.Name = NewTourName;
                SelectedTour.Description = NewTourDescr;
                SelectedTour.From = NewTourFrom;
                SelectedTour.To = NewTourTo;
                SelectedTour.TransportType = NewTourTransType;
                SelectedTour.Distance = NewTourDistance;
                SelectedTour.EstimatedTime = NewTourEstTime;

                Log.Info($"Download SummarizedTourReport: {SelectedTour.Name}");
                ReportGenerator.GenerateSummarizeReport(SelectedTour);
            }
        }
        public void ExportTourData()
        {
            if (SelectedTour != null)
            {
                SelectedTour.Name = NewTourName;
                SelectedTour.Description = NewTourDescr;
                SelectedTour.From = NewTourFrom;
                SelectedTour.To = NewTourTo;
                SelectedTour.TransportType = NewTourTransType;
                SelectedTour.Distance = NewTourDistance;
                SelectedTour.EstimatedTime = NewTourEstTime;

                ExportImportService.ExportTourToFile(selectedTour);
                Log.Info($"Export Tour Data: {SelectedTour.Name}");
            }
        }
        public void ImportTourData(object filePath)
        {
            string path = filePath as string;
            if (!string.IsNullOrEmpty(path))
            {
                var tmpTour = ExportImportService.ImportTourFromFile(path, Tours.ToList());
                if (tmpTour != null)
                {
                    if (Tours != null)
                    {
                        Log.Info($"Importing tour data from: {path}");
                        Log.Info($"---{tmpTour.Name},{tmpTour.Id}---");
                        _tourService.AddTour(tmpTour);
                        Tours.Add(tmpTour);
                        OnPropertyChanged(nameof(FilteredTours));
                    }

                    else
                    {
                        ShowPopup("Error Importing Check for same Id`s");
                        Log.Info("Tours collection is not initialized.");
                    }
                }
                else 
                {
                    ShowPopup("Error Importing Check for same Id`s");
                }
            }     
        }
        public void ExportAllTours(ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.CSV:
                    ExportImportService.ExportAsCSV(Tours.ToList());
                    break;
                case ExportFormat.XML:
                    ExportImportService.ExportAsXML(Tours.ToList());
                    break;
                case ExportFormat.JSON:
                    ExportImportService.ExportAsJSON(Tours.ToList());
                    break;
                default:
                    break;
            }
        }

        public async void AddTour()
        {
            if (ValidateTourInput())
            {
                await GetRouteInfo();
                var newTour = new Tour
                {
                    Name = NewTourName,
                    Description = NewTourDescr,
                    From = NewTourFrom,
                    To = NewTourTo,
                    TransportType = NewTourTransType,
                    Distance = NewTourDistance,
                    EstimatedTime = NewTourEstTime,
                    Img = "Placeholder"
                };

                Log.Info($"Tour Added: {newTour.Name}");
                _tourService.AddTour(newTour);
                Tours.Add(newTour);

                FilteredTours = new ObservableCollection<Tour>(Tours);
            }          
        }
        public void UpdateTour()
        {
            if (ValidateTourInput())
            {
                SelectedTour.Name = NewTourName;
                SelectedTour.Description = NewTourDescr;
                SelectedTour.From = NewTourFrom;
                SelectedTour.To = NewTourTo;
                SelectedTour.TransportType = NewTourTransType;
                SelectedTour.Distance = NewTourDistance;
                SelectedTour.EstimatedTime = NewTourEstTime;
                SelectedTour.Img = "Placeholder";

                Log.Info($"Tour Updated: {SelectedTour.Name}");
                _tourService.UpdateTour(SelectedTour);

                FilteredTours = new ObservableCollection<Tour>(Tours);
            }
        }
        public void DeleteTour()
        {
            if (SelectedTour != null)
            {
                Log.Info($"Tour Deleted: {SelectedTour.Name}");
                _tourService.DeleteTour(SelectedTour.Id);
                Tours.Remove(SelectedTour);
                FilteredTours = new ObservableCollection<Tour>(Tours);
            }
        }
        private void LoadTours()
        {
            Tours = new ObservableCollection<Tour>(_tourService.GetAllTours());
            FilteredTours = new ObservableCollection<Tour>(Tours);

            foreach (Tour tour in Tours) { Log.Info(tour.Name+": "+tour.Id); }

            Log.Info($"Tours Loading Count: {Tours.Count()}");
        }
        public void AddTourLog()
        {
            if (ValidateTourLogInput())
            {
                var newTourLog = new TourLog
                {
                    Tour = TourLogsSelectedTour,
                    DateTime = NewDateTime,
                    Comment = NewComment,
                    Difficulty = NewDifficulty,
                    TotalDistance = NewTotalDistance,
                    TotalTime = NewTotalTime,
                    Rating = NewRating
                };

                Log.Info($"Adding Tourlog to Tour: {newTourLog.Tour.Name}");
                TourLogsSelectedTour.TourLogs.Add(newTourLog);
                _tourService.UpdateTour(TourLogsSelectedTour);
                FilteredTours = new ObservableCollection<Tour>(Tours);
            }           
        }
        public void DeleteTourLog()
        {
            if (SelectedTourLog != null && TourLogsSelectedTour != null)
            {
                Log.Info($"Deleting Tourlog from Tour: {SelectedTourLog.Tour.Name}");
                TourLogsSelectedTour.TourLogs.Remove(SelectedTourLog);
                _tourService.UpdateTour(TourLogsSelectedTour);
                FilteredTours = new ObservableCollection<Tour>(Tours);
            }
        }
        public void SaveTourLog()
        {
            if (ValidateTourLogInput())
            {
                SelectedTourLog.DateTime = NewDateTime;
                SelectedTourLog.Comment = NewComment;
                SelectedTourLog.Difficulty = NewDifficulty;
                SelectedTourLog.TotalDistance = NewTotalDistance;
                SelectedTourLog.TotalTime = NewTotalTime;
                SelectedTourLog.Rating = NewRating;

                Log.Info($"Updating Tourlog from Tour: {SelectedTourLog.Tour.Name}");
                _tourService.UpdateTour(TourLogsSelectedTour);
                FilteredTours = new ObservableCollection<Tour>(Tours);
            }
        }

        private void ShowPopup(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private async Task GetRouteInfo()
        {
            if (string.IsNullOrWhiteSpace(NewTourFrom) || string.IsNullOrWhiteSpace(NewTourTo))
                return;

            var (responseBody, routeResponse) = await _routeService.GetRoute(NewTourFrom, NewTourTo, NewTourTransType);

            if (string.IsNullOrEmpty(responseBody) || routeResponse == null)
            {
                NewTourDistance = 0;
                NewTourEstTime = 0;
                return;
            }

            try
            {
                string filePath = "Resources/directions.js";
                string newJsonDirection = "var directions = " + responseBody;
                await File.WriteAllTextAsync(filePath, newJsonDirection, Encoding.UTF8);
            }
            catch (IOException ioEx)
            {
                Log.Warn($"Konnte directions.js nicht schreiben: {ioEx.Message}");
            }

            if (routeResponse.Features != null && routeResponse.Features.Count > 0)
            {
                var firstFeature = routeResponse.Features[0];
                var summary = firstFeature.Properties.Summary;

                NewTourDistance = (int)summary.Distance;
                NewTourEstTime = (int)summary.Duration;
            }
            else
            {
                NewTourDistance = 0;
                NewTourEstTime = 0;
            }

            if (RefreshMap != null)
            {
                await RefreshMap.Invoke();
            }
        }


        private void ApplyFilter()
        {
            string searchText = SearchFilter?.ToLower() ?? "";

            FilteredTours = new ObservableCollection<Tour>(Tours.Where(tour =>
                tour.Name.ToLower().Contains(searchText) ||
                tour.Description.ToLower().Contains(searchText) ||
                tour.From.ToLower().Contains(searchText) ||
                tour.To.ToLower().Contains(searchText) ||
                tour.ChildFriendliness.ToLower().Contains(searchText)

            ));
        }
        private void ClearTourFields()
        {
            NewTourName = string.Empty;
            NewTourDescr = string.Empty;
            NewTourFrom = string.Empty;
            NewTourTo = string.Empty;
            NewTourTransType = TransportType.None;
            NewTourDistance = 0;
            NewTourEstTime = 0;
        }
        private void ClearTourLogFields()
        {
            NewDateTime = DateTime.Now;
            NewComment = string.Empty;
            NewDifficulty = DifficultyLevel.None;
            NewTotalDistance = string.Empty;
            NewTotalTime = string.Empty;
            NewRating = 0;
        }
        private bool ValidateTourLogInput()
        {
            string error = null;

            if (TourLogsSelectedTour == null)
            {
                error = "No tour selected. Please select a tour before adding a tour log.";
            }
            else if (NewDateTime == null)
            {
                error = "Date cannot be null or empty";
            }
            else if (string.IsNullOrWhiteSpace(NewComment))
            {
                error = "Comment cannot be null or empty.";
            }
            else if (string.IsNullOrWhiteSpace(NewTotalDistance) || !int.TryParse(NewTotalDistance, out int distance) || distance <= 0)
            {
                error = "Total Distance must be a positive integer.";
            }
            else if (string.IsNullOrWhiteSpace(NewTotalTime) || !int.TryParse(NewTotalTime, out int time) || time <= 0)
            {
                error = "Total Time must be a positive integer.";
            }
            else if (NewRating == null || NewRating < 0 || NewRating > 10)
            {
                error = "Rating must be between 0 and 10.";
            }

            if (error != null)
            {
                MessageBox.Show($"Validation failed: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Info($"Validation failed: {error}");

                return false;
            }
            else
            {
                return true;
            }
        }
        private bool ValidateTourInput()
        {
            string error = null;

            if (string.IsNullOrEmpty(NewTourName))
            {
                error = "Name cannot be null or empty.";
            }
            else if (NewTourName.Length > 100)
            {
                error = "Name cannot be longer than 100 characters.";
            }

            else if (string.IsNullOrEmpty(NewTourDescr))
            {
                error = "Description cannot be null or empty.";
            }
            else if (NewTourDescr.Length > 500)
            {
                error = "Description cannot be longer than 500 characters.";
            }

            else if (string.IsNullOrEmpty(NewTourFrom))
            {
                error = "From cannot be null or empty.";
            }

            else if (string.IsNullOrEmpty(NewTourTo))
            {
                error = "To cannot be null or empty.";
            }

            else if (!Enum.IsDefined(typeof(TransportType), NewTourTransType))
            {
                error = "Transport Type is not valid.";
            }

            else if (NewTourDistance < 0)
            {
                error = "Distance must be a positive integer.";
            }
            else if (NewTourEstTime < 0)
            {
                error = "Estimated Time must be a positive integer.";
            }

            if (error != null)
            {
                MessageBox.Show($"Validation failed: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Info($"Validation failed: {error}");

                return false;
            }

            else
            {
                return true;
            }
        }

        public ObservableCollection<Tour> Tours { get; set; }
        public ObservableCollection<Tour> FilteredTours
        {
            get => filteredTours;
            set { filteredTours = value; OnPropertyChanged(); }
        }
        public string SearchFilter
        {
            get => _searchFilter;
            set { _searchFilter = value; ApplyFilter(); OnPropertyChanged(); }
        }

        public Tour SelectedTour
        {
            get => selectedTour;
            set
            {
                selectedTour = value;
                OnPropertyChanged();
                if (selectedTour != null)
                {
                    NewTourName = selectedTour.Name;
                    NewTourDescr = selectedTour.Description;
                    NewTourFrom = selectedTour.From;
                    NewTourTo = selectedTour.To;
                    NewTourTransType = selectedTour.TransportType;
                    GetRouteInfo();
                }
                else ClearTourFields();
            }
        }
        public string NewTourName
        {
            get { return newTourName; }
            set
            {
                newTourName = value;
                OnPropertyChanged();
            }
        }
        public string NewTourDescr
        {
            get { return newTourDescr; }
            set
            {
                newTourDescr = value;
                OnPropertyChanged();
            }
        }
        public string NewTourFrom
        {
            get { return newTourFrom; }
            set
            {
                newTourFrom = value;
                OnPropertyChanged();
            }
        }
        public string NewTourTo
        {
            get { return newTourTo; }
            set
            {
                newTourTo = value;
                OnPropertyChanged();
            }
        }
        public TransportType NewTourTransType
        {
            get { return newTourTransType; }
            set
            {
                newTourTransType = value;
                OnPropertyChanged();
            }
        }
        public int NewTourDistance
        {
            get { return newTourDistance; }
            set
            {
                newTourDistance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDistance));
            }
        }
        public int NewTourEstTime
        {
            get { return newTourEstTime; }
            set
            {
                newTourEstTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedEstimatedTime));
            }
        }

        public Tour TourLogsSelectedTour
        {
            get => tourLogsSelectedTour;
            set { tourLogsSelectedTour = value; OnPropertyChanged(); }
        }
        public TourLog SelectedTourLog
        {
            get => selectedTourLog;
            set
            {
                selectedTourLog = value;
                OnPropertyChanged();
                if (selectedTourLog != null)
                {
                    NewDateTime = selectedTourLog.DateTime;
                    NewComment = selectedTourLog.Comment;
                    NewDifficulty = selectedTourLog.Difficulty;
                    NewTotalDistance = selectedTourLog.TotalDistance;
                    NewTotalTime = selectedTourLog.TotalTime;
                    NewRating = selectedTourLog.Rating;
                }
                else ClearTourLogFields();
            }
        }


        public DateTime NewDateTime
        {
            get { return newdateTime; }
            set
            {
                newdateTime = value;
                OnPropertyChanged();
            }
        }
        public string NewComment
        {
            get { return newcomment; }
            set
            {
                newcomment = value;
                OnPropertyChanged();
            }
        }
        public DifficultyLevel NewDifficulty
        {
            get { return newdifficulty; }
            set
            {
                newdifficulty = value;
                OnPropertyChanged();
            }
        }
        public string NewTotalDistance
        {
            get { return newtotalDistance; }
            set
            {
                newtotalDistance = value;
                OnPropertyChanged();
            }
        }
        public string FormattedDistance
        {
            get
            {
                return $"{NewTourDistance / 1000.0:F2} km";
            }
        }
        public string FormattedEstimatedTime
        {
            get
            {
                int hours = NewTourEstTime / 3600;
                int minutes = (NewTourEstTime % 3600) / 60;
                int seconds = NewTourEstTime % 60;
                return $"{hours}h {minutes}min {seconds}sec";
            }
        }
        public string NewTotalTime
        {
            get { return newtotalTime; }
            set
            {
                newtotalTime = value;
                OnPropertyChanged();
            }
        }
        public int NewRating
        {
            get { return newrating; }
            set
            {
                newrating = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
