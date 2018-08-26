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
    public class ClassRepository : IClassRepository
    {
        private readonly string connectionString;

        public ClassRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<int> Insert(Class @class)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return await conn.ExecuteAsync(
                        @"INSERT INTO Classes VALUES(@SubjectChineseName, @Teachers, @ClassShortName, @Guid);",
                        @class);
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Class>> Select(Class @class, bool sqlLike = false, bool sqlAnd = true)
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (@class == null)
                        return await conn.QueryAsync<Class>(@"SELECT * FROM Classes");
                    else
                    {
                        string findOperator = sqlLike ? " LIKE " : "=";
                        if (sqlLike)
                        {
                            @class.SubjectChineseName = $"%{ @class.SubjectChineseName }%";
                            @class.Teachers = $"%{ @class.Teachers }%";
                            @class.ClassShortName = $"%{ @class.ClassShortName }%";
                            @class.Guid = $"%{ @class.Guid }%";
                        }

                        string suffix = "";
                        if (sqlAnd)
                        {
                            suffix += string.IsNullOrEmpty(@class.SubjectChineseName) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } SubjectChineseName{ findOperator }@SubjectChineseName";
                            suffix += string.IsNullOrEmpty(@class.Teachers) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } Teachers{ findOperator }@Teachers";
                            suffix += string.IsNullOrEmpty(@class.ClassShortName) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } ClassShortName{ findOperator }@ClassShortName";
                            suffix += string.IsNullOrEmpty(@class.Guid) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } Guid{ findOperator }@Guid";
                            suffix = string.IsNullOrEmpty(suffix) ? "" : (" WHERE" + suffix);
                        }
                        else
                        {
                            suffix += string.IsNullOrEmpty(@class.SubjectChineseName) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } SubjectChineseName{ findOperator }@SubjectChineseName";
                            suffix += string.IsNullOrEmpty(@class.Teachers) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } Teachers{ findOperator }@Teachers";
                            suffix += string.IsNullOrEmpty(@class.ClassShortName) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } ClassShortName{ findOperator }@ClassShortName";
                            suffix += string.IsNullOrEmpty(@class.Guid) ? "" :
                                $"{ (suffix.Length == 0 ? "" : " AND") } Guid{ findOperator }@Guid";
                            suffix = string.IsNullOrEmpty(suffix) ? "" : (" WHERE" + suffix);
                        }
                        return await conn.QueryAsync<Class>($@"SELECT * FROM Classes{ suffix };", @class);
                    }
                }
            }
            catch (NotImplementedException ex)
            {
                throw ex;
            }
        }
    }
}
