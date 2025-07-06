using Tour_Planner.Models;

namespace Tour_Planner.BL
{
    public interface ITourService
    {
        IEnumerable<Tour> GetAllTours();
        void AddTour(Tour tour);
        void UpdateTour(Tour tour);
        void DeleteTour(int id);
    }
}
