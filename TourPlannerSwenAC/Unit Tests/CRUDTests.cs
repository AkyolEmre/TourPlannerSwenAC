using Microsoft.EntityFrameworkCore;
using Tour_Planner.BL;
using Tour_Planner.DAL;
using Tour_Planner.Models;
using Tour_Planner.ViewModels;

namespace UnitTests
{
    [TestClass]
    public class TourPlannerVMTests
    {
        private TourContext context;
        private TourRepository repository;
        private TourPlannerVM tourPlannerVM;
        private TourService _tourService;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TourContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            context = new TourContext(options);
            repository = new TourRepository(context);
            _tourService = new TourService(repository);
            tourPlannerVM = new TourPlannerVM(_tourService);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.Database.EnsureDeleted();
            context.Dispose();

            var options = new DbContextOptionsBuilder<TourContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            context = new TourContext(options); 
        }

        [TestMethod]
        public void AddTour()
        {
            int initialCount = tourPlannerVM.Tours.Count;

            Tour test = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            tourPlannerVM.Tours.Add(test);

            Assert.AreEqual(initialCount + 1, tourPlannerVM.Tours.Count);
        }

        [TestMethod]
        public void UpdateTour()
        {
            Tour selectedTour = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            tourPlannerVM.SelectedTour = selectedTour;
            tourPlannerVM.UpdateTour();

            Assert.AreEqual(tourPlannerVM.NewTourName, selectedTour.Name);
        }

        [TestMethod]
        public void DeleteTour()
        {
            Tour tourToRemove = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            tourPlannerVM.Tours.Add(tourToRemove);
            tourPlannerVM.SelectedTour = tourToRemove;

            Assert.IsTrue(tourPlannerVM.Tours.Contains(tourToRemove));

            tourPlannerVM.DeleteTour();

            Assert.IsFalse(tourPlannerVM.Tours.Contains(tourToRemove));
        }

        [TestMethod]
        public void AddTourLog()
        {
            Tour test = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            tourPlannerVM.Tours.Add(test);
            tourPlannerVM.TourLogsSelectedTour = test;
            int initialCount = test.TourLogs.Count;

            test.TourLogs.Add(new TourLog
            {
                Tour = test,
                DateTime = DateTime.Now,
                Comment = "testlog_Comment",
                Difficulty = DifficultyLevel.Medium,
                TotalDistance = "5000",
                TotalTime = "3600",
                Rating = 5
            });

            Assert.AreEqual(initialCount + 1, test.TourLogs.Count);
        }

        [TestMethod]
        public void UpdateTourLog()
        {
            var selectedTour = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            var selectedTourLog = new TourLog
            {
                Tour = selectedTour,
                DateTime = DateTime.Now,
                Comment = "testlog_Comment",
                Difficulty = DifficultyLevel.Medium,
                TotalDistance = "5000",
                TotalTime = "3600",
                Rating = 5
            };

            tourPlannerVM.TourLogsSelectedTour = selectedTour;
            tourPlannerVM.SelectedTourLog = selectedTourLog;

            tourPlannerVM.SaveTourLog();

            Assert.AreEqual(selectedTourLog.DateTime, tourPlannerVM.NewDateTime);
            Assert.AreEqual(selectedTourLog.Comment, tourPlannerVM.NewComment);
        }

        [TestMethod]
        public void DeleteTourLog()
        {
            Tour test = new Tour
            {
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            TourLog testLog = new TourLog
            {
                Tour = test,
                DateTime = DateTime.Now,
                Comment = "testlog_Comment",
                Difficulty = DifficultyLevel.Medium,
                TotalDistance = "5000",
                TotalTime = "3600",
                Rating = 5
            };

            tourPlannerVM.TourLogsSelectedTour = test;
            tourPlannerVM.SelectedTourLog = testLog;

            tourPlannerVM.AddTourLog();
            tourPlannerVM.DeleteTourLog();

            Assert.IsFalse(tourPlannerVM.TourLogsSelectedTour.TourLogs.Contains(testLog));
        }
    }
}
