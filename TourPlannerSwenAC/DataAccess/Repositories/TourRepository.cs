using DataAccess.Repositories.Interfaces;
using DataAccess.Database;
using Model;
using Npgsql;
using Dapper;

namespace DataAccess.Repositories
{
    public class TourRepository : ITourRepository
    {
        private NpgsqlConnection? connection;

        public Tour GetTour(int id)
        {
            Tour? tour;

            {
                tour = connection.Query<Tour>("SELECT * FROM tours WHERE id = @id LIMIT 1", new { id }).ToList().FirstOrDefault();
            }
            return tour;
        }

        public List<Tour> GetAllTours()
        {
            List<Tour> tours;

            {
                tours = connection.Query<Tour>("SELECT * FROM tours ORDER BY id ASC").ToList();
            }
            return tours;
        }

        public void AddTour(Tour tour)
        {
            {
                string query = """
                    INSERT INTO tours (name, description, from_location, to_location, transportation_type, distance, estimated_time, route_information)
                    VALUES (@Name, @Description, @From_location, @To_location, @Transportation_type, @Distance, @Estimated_time, @Route_information)
                    """;
                connection.Query<Tour>(query, tour);
            }
        }

        public void DeleteTour(int id)
        {

            // Delete tour
            {
                connection.Query("DELETE FROM tours WHERE id = @id", new { id });
            }
        }

        public void UpdateTour(Tour tour)
        {
            // Update tour
            {
                string query = """
                    UPDATE tours SET name = @Name, description = @Description, from_location = @From_location, to_location = @To_location, transportation_type = @Transportation_type, 
                    distance = @Distance, estimated_time = @Estimated_time, route_information = @Route_information WHERE id = @Id
                    """;
                connection.Query<Tour>(query, tour);
            }
        }
    }
}
