using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Tour_Planner.Models
{
    public enum TransportType
    {
        Walk,
        Bicycle,
        Car,
        None
    }

    public enum ExportFormat
    {
        CSV,
        XML,
        JSON
    }

    public class Tour : INotifyPropertyChanged
    {
        private ObservableCollection<TourLog> tourLogs;

        public Tour()
        {
            Name = string.Empty;
            Description = string.Empty;
            From = string.Empty;
            To = string.Empty;
            Img = string.Empty;

            tourLogs = new ObservableCollection<TourLog>();
            tourLogs.CollectionChanged += (s, e) => UpdateComputedProperties();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Img { get; set; }

        private TransportType transportType;
        public TransportType TransportType
        {
            get => transportType;
            set
            {
                if (transportType != value)
                {
                    transportType = value;
                    OnPropertyChanged(nameof(TransportType));
                }
            }
        }

        public int Distance { get; set; }
        public int EstimatedTime { get; set; }

        public ObservableCollection<TourLog> TourLogs
        {
            get => tourLogs;
            set
            {
                tourLogs = value;
                OnPropertyChanged(nameof(TourLogs));
                UpdateComputedProperties();
            }
        }

        public int Popularity => TourLogs?.Count ?? 0;

        public string ChildFriendliness
        {
            get
            {
                if (TourLogs == null || !TourLogs.Any())
                    return "Not enough data";

                double averageDifficulty = TourLogs.Average(log => (int)log.Difficulty);
                double averageTime = TourLogs.Average(log => Convert.ToDouble(log.TotalTime));
                double averageDistance = TourLogs.Average(log => Convert.ToDouble(log.TotalDistance));

                bool isChildFriendly = averageDifficulty <= 2 && averageTime <= 3600 && averageDistance <= 5000;

                return isChildFriendly ? "Child-Friendly" : "Not Child-Friendly";
            }
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(Popularity));
            OnPropertyChanged(nameof(ChildFriendliness));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
