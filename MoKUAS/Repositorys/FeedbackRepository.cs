using Dapper;
using MoKUAS.Interfaces;
using MoKUAS.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MoKUAS.Repositorys
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly string connectionString;

        public FeedbackRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<Feedback> SelectByCreatorAsync(Feedback feedback)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.QueryFirstOrDefaultAsync<Feedback>(
                        @"SELECT * FROM Feedbacks WHERE Creator=@Creator;", feedback);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }

        public async Task<int> Insert(Feedback feedback)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.ExecuteAsync(
                        @"INSERT INTO Feedbacks VALUES (@Email, @Type, @Content, @DateTime, @Creator, @Guid);",
                        feedback);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }
    }
}