using Microsoft.EntityFrameworkCore;
using Tour_Planner.BL;
using Tour_Planner.DAL;
using Tour_Planner.Models;
using Tour_Planner.ViewModels;

namespace UnitTests
{
    [TestClass]
    public class ServiceTests
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
            context.Database.EnsureDeleted(); // Löscht die Datenbank
            context.Dispose(); // Verwirft den Kontext

            var options = new DbContextOptionsBuilder<TourContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

            context = new TourContext(options); // Erstellt einen neuen Kontext
        }

        [TestMethod]
        public void AddTour()
        {
            var test = new Tour
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

            _tourService.AddTour(test);

            Assert.AreEqual(test, context.Tours.First());
        }

        [TestMethod]
        public void UpdateTour()
        {
            var test = new Tour
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

            context.Tours.Add(test);
            context.SaveChanges();

            test.Name = "UpdatedTour";

            _tourService.UpdateTour(test);

            Assert.AreEqual("UpdatedTour", context.Tours.First().Name);
        }

        [TestMethod]
        public void DeleteTour()
        {
            var test = new Tour
            {
                Id = 100,
                Name = "test_Name",
                Description = "test_Descr",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Car,
                Distance = 0,
                EstimatedTime = 0,
                Img = "tour1.jpg"
            };

            context.Tours.Add(test);
            context.SaveChanges();

            _tourService.DeleteTour(100);

            Assert.IsFalse(context.Tours.Contains(test));
        }

        [TestMethod]
        public void GetAllTours()
        {
            var test = new Tour
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

            var test2 = new Tour
            {
                Name = "test_Name2",
                Description = "test_Descr2",
                From = "3910 Zwettl",
                To = "1200 Wien",
                TransportType = TransportType.Walk,
                Distance = 20,
                EstimatedTime = 20,
                Img = "tour2.jpg"
            };

            context.Tours.Add(test);
            context.Tours.Add(test2);
            context.SaveChanges();

            IEnumerable<Tour> tours = _tourService.GetAllTours();

            Assert.AreEqual(2, tours.Count());
        }
    }
}

