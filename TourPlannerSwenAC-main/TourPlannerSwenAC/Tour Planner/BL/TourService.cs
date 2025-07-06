using Tour_Planner.DAL;
using Tour_Planner.Models;

namespace Tour_Planner.BL
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;

        public TourService(ITourRepository tourRepository)
        {
            _tourRepository = tourRepository;
        }

        public IEnumerable<Tour> GetAllTours()
        {
            return _tourRepository.GetAllTours();
        }

        public void AddTour(Tour tour)
        {
            _tourRepository.AddTour(tour);
        }

        public void UpdateTour(Tour tour)
        {
            _tourRepository.UpdateTour(tour);
        }

        public void DeleteTour(int id)
        {
            var tourToDelete = _tourRepository.GetTourById(id);
            if (tourToDelete != null)
            {
                _tourRepository.DeleteTour(id);
            }
        }
    }
}
