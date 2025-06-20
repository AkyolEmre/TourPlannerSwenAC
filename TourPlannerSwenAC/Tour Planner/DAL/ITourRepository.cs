using Tour_Planner.Models;

namespace Tour_Planner.DAL
{
    public interface ITourRepository
    {
        List<Tour> GetAllTours();
        Tour GetTourById(int id);
        void AddTour(Tour tour);
        void UpdateTour(Tour tour);
        void DeleteTour(int id);
    }
}
