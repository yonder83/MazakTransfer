using System;
using System.Data.SQLite;
using System.IO;

namespace MazakTransfer.Database
{
    public class DrawingService
    {
        //private readonly string connectionString = ConfigurationManager.ConnectionStrings["MazakTransferContainer"].ConnectionString;
        private const string ConnectionString = "Data Source=MazakTransfer.sqlite;Version=3;";
        private const string DatabaseName = "MazakTransfer.sqlite";

        private const string SqlCreateTable = @"
CREATE TABLE IF NOT EXISTS Drawing (
    Id INTEGER NOT NULL PRIMARY KEY,
    FileName nvarchar(20) NOT NULL,
    Comment nvarchar(4000) NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS UQ_Drawing_FileName ON Drawing (FileName);
";

        public static void CreateDatabaseIfNotExists()
        {
            //Create database file if it doesn't exists
            if (!File.Exists(DatabaseName))
            {
                SQLiteConnection.CreateFile(DatabaseName);
            }

            //Create table if it does not exists
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var command = new SQLiteCommand(SqlCreateTable, connection);
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public string GetDrawingCommentByName(string drawingName)
        {
            if (drawingName == null) return null;

            string comment = null;

            const string sql = @"SELECT [Extent1].[Comment] AS [Comment]
                                FROM [Drawing] AS [Extent1]
                                WHERE [Extent1].[FileName] = @FileName
                                LIMIT 1;";

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                using (var command = new SQLiteCommand(sql, connection))
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

            const string sql = @"SELECT [Extent1].[Id] AS [Id], 
               [Extent1].[FileName] AS [FileName], 
               [Extent1].[Comment] AS [Comment]
               FROM [Drawing] AS [Extent1]
               WHERE [Extent1].[FileName] = @FileName 
               LIMIT 1;";

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                using (var command = new SQLiteCommand(sql, connection))
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
                const string insertSql = @"INSERT INTO [Drawing]([FileName], [Comment])
                VALUES (@FileName, @Comment);";

                using (var connection = new SQLiteConnection(ConnectionString))
                {
                    using (var command = new SQLiteCommand(insertSql, connection))
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

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                using (var command = new SQLiteCommand(updateSql, connection))
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
