using System;
using System.Configuration;
using System.Data.SqlServerCe;

namespace MazakTransfer.Database
{
    public class DrawingService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MazakTransferContainer"].ConnectionString;

        public string GetDrawingCommentByName(string drawingName)
        {
            if (drawingName == null) return null;

            string comment = null;

            const string sql = @"SELECT TOP (1)
                                [Extent1].[Comment] AS [Comment]
                                FROM [Drawing] AS [Extent1]
                                WHERE [Extent1].[FileName] = @FileName";

            using (var connection = new SqlCeConnection(connectionString))
            {
                using (var command = new SqlCeCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("FileName", drawingName);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            comment = (string) reader["Comment"];
                        }

                        reader.Close();
                        connection.Close();
                    }
                }
            }

            return comment;
        }

        /// <summary>
        /// Talentaa kommentin piirrustukselle, palauttaa truen jos tallennus tehtiin.
        /// </summary>
        /// <param name="drawingName"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool SaveCommentByDrawingName(string drawingName, string comment)
        {
            if (drawingName == null) return false;

            Drawing drawing = null;

            const string sql = @"SELECT TOP (1) 
               [Extent1].[Id] AS [Id], 
               [Extent1].[FileName] AS [FileName], 
               [Extent1].[Comment] AS [Comment]
               FROM [Drawing] AS [Extent1]
               WHERE [Extent1].[FileName] = @FileName";

            using (var connection = new SqlCeConnection(connectionString))
            {
                using (var command = new SqlCeCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("FileName", drawingName);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            drawing = new Drawing
                            {
                                Id = (long) reader["Id"],
                                FileName = (string) reader["FileName"],
                                Comment = (string) reader["Comment"]
                            };
                        }

                        reader.Close();
                        connection.Close();
                    }
                }
            }

            if (drawing == null)
            {
                const string insertSql = @"INSERT [Drawing]([FileName], [Comment])
                VALUES (@FileName, @Comment);";

                using (var connection = new SqlCeConnection(connectionString))
                {
                    using (var command = new SqlCeCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("FileName", drawingName);
                        command.Parameters.AddWithValue("Comment", comment);
                        connection.Open();
                        int success = command.ExecuteNonQuery();
                        connection.Close();
                        return success > 0;
                    }
                }
            }

            if (String.Equals(comment, drawing.Comment))
            {
                return false;
            }

            const string updateSql = @"UPDATE [Drawing]
               SET [Comment] = @Comment
               WHERE ([Id] = @Id)";

            using (var connection = new SqlCeConnection(connectionString))
            {
                using (var command = new SqlCeCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("Comment", comment);
                    command.Parameters.AddWithValue("Id", drawing.Id);
                    connection.Open();
                    int success = command.ExecuteNonQuery();
                    connection.Close();
                    return success > 0;
                }
            }
        }
    }
}
