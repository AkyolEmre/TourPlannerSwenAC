using DataAccess.Repositories.Interfaces;
using Model;
using Npgsql;
using Dapper;

namespace DataAccess.Repositories
{
    public class TourLogRepository : ITourLogRepository
    {
        private NpgsqlConnection? connection;

        public List<TourLog> GetTourLogs(int tourID)
        {
            List<TourLog> tourlogs = new List<TourLog>();

            {
                tourlogs = connection.Query<TourLog>("SELECT * FROM tour_logs WHERE tour_id = @tour_id ORDER BY id ASC", new { tour_id = tourID }).ToList<TourLog>();
            }
            return tourlogs;
        }

        public void AddTourLog(TourLog tourlog)
        {
            {
                string query = """
                    INSERT INTO tour_logs (tour_id, logdate, comment, difficulty, total_distance, total_time, rating)
                    VALUES (@Tour_id, @Logdate, @Comment, @Difficulty, @Total_distance, @Total_time, @Rating)
                    """;
                connection.Query<TourLog>(query, tourlog);
            }
        }

        public void DeleteTourLog(int id)
        {
            {
                connection.Query("DELETE FROM tour_logs WHERE id = @id", new { id });
            }
        }

        public void UpdateTourLog(TourLog tourlog)
        {
            {
                string query = """
                    UPDATE tour_logs SET tour_id = @Tour_id, logdate = @Logdate, comment = @Comment, difficulty = @Difficulty, 
                    total_distance = @Total_distance, total_time = @Total_time, rating = @Rating WHERE id = @Id
                    """;
                connection.Query<Tour>(query, tourlog);
            }
        }
    }
}
