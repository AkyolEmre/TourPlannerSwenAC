using Microsoft.EntityFrameworkCore;
using Tour_Planner.Models;

namespace Tour_Planner.DAL
{
    public class TourRepository : ITourRepository
    {
        private readonly TourContext _context;

        public TourRepository(TourContext context)
        {
            _context = context;
        }

        public List<Tour> GetAllTours()
        {
            return _context.Tours.Include(t => t.TourLogs).ToList();
        }

        public Tour GetTourById(int id)
        {
            return _context.Tours.Include(t => t.TourLogs).FirstOrDefault(t => t.Id == id);
        }

        public void AddTour(Tour tour)
        {
            _context.Tours.Add(tour);
            _context.SaveChanges();
        }

        public void UpdateTour(Tour tour)
        {
            foreach (var tourLog in tour.TourLogs)
            {
                tourLog.DateTime = tourLog.DateTime.ToUniversalTime();
            }

            _context.Tours.Update(tour);
            _context.SaveChanges();
        }


        public void DeleteTour(int id)
        {
            var tour = _context.Tours.Find(id);
            if (tour != null)
            {
                _context.Tours.Remove(tour);
                _context.SaveChanges();
            }
        }
    }
}
