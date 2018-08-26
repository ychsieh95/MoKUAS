using Dapper;
using MoKUAS.Interfaces;
using MoKUAS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MoKUAS.Repositorys
{
    public class CommentRepository : ICommentRepository
    {
        private readonly string connectionString;

        public CommentRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<int> Delete(Comment comment)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.ExecuteAsync(
                        @"DELETE FROM Comments WHERE Guid=@Guid;",
                        comment);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }

        public async Task<int> Insert(Comment comment)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.ExecuteAsync(
                        @"INSERT INTO Comments VALUES (@ClassGuid,       @IsBlackboard,  @IsBook,            @IsPPT,         @IsBroadcast,
                                                       @IsBuild,         @IsInteractive, @RollCallFrequency, @ByInPerson,    @BySignInSheet,
                                                       @ByOnline,        @ByClasswork,   @ByTest,            @HaveClasswork, @HaveTest,
                                                       @HaveMidtermExam, @HaveFinalExam, @Grade,             @Remark,        @Creator,
                                                       @RecordTime,      @Guid);",
                        comment);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Comment>> Select(Comment comment)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (comment == null)
                        return await conn.QueryAsync<Comment>(@"SELECT * FROM Classes", comment);
                    else
                    {
                        string suffix = "";
                        suffix += string.IsNullOrEmpty(comment.ClassGuid) ? "" : $"{ (suffix.Length == 0 ? "" : " AND") } ClassGuid=@ClassGuid";
                        suffix += string.IsNullOrEmpty(comment.Creator) ? "" : $"{ (suffix.Length == 0 ? "" : " AND") } Creator=@Creator";
                        suffix += string.IsNullOrEmpty(comment.Guid) ? "" : $"{ (suffix.Length == 0 ? "" : " AND") } Guid=@Guid";
                        suffix = string.IsNullOrEmpty(suffix) ? "" : (" WHERE" + suffix);
                        return await conn.QueryAsync<Comment>($@"SELECT * FROM Comments{ suffix };", comment);
                    }
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }

        public async Task<int> Update(Comment comment)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.ExecuteAsync(
                        @"UPDATE Comments SET ClassGuid=@ClassGuid,             IsBlackboard=@IsBlackboard,   IsBook=@IsBook,                       IsPPT=@IsPPT,                 IsBroadcast=@IsBroadcast,
                                              IsBuild=@IsBuild,                 IsInteractive=@IsInteractive, RollCallFrequency=@RollCallFrequency, ByInPerson=@ByInPerson,       BySignInSheet=@BySignInSheet,
                                              ByOnline=@ByOnline,               ByClasswork=@ByClasswork,     ByTest=@ByTest,                       HaveClasswork=@HaveClasswork, HaveTest=@HaveTest,
                                              HaveMidtermExam=@HaveMidtermExam, HaveFinalExam=@HaveFinalExam, Grade=@Grade,                         Remark=@Remark,               Creator=@Creator,
                                              RecordTime=@RecordTime
                                        WHERE Guid=@Guid;",
                    comment);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }
    }
}
