using SQLite;
using System;
using System.Linq;

namespace MazakTransfer.Database
{
    public class DrawingService
    {
        private const string DatabaseName = "MazakTransfer.sqlite";

        public static void CreateDatabaseIfNotExists()
        {
            //Create database file if it doesn't exists
            using (var connection = new SQLiteConnection(DatabaseName))
            {
                connection.CreateTable<Drawing>();
            }
        }

        public string GetDrawingCommentByName(string drawingName)
        {
            if (drawingName == null) return null;

            string comment = null;

            const string sql = @"SELECT [Extent1].[Comment] AS [Comment]
                                FROM [Drawing] AS [Extent1]
                                WHERE [Extent1].[FileName] = ?
                                LIMIT 1;";

            using (var connection = new SQLiteConnection(DatabaseName))
            {
                var results = connection.Query<Drawing>(sql, drawingName);
                if (results.Count > 0)
                    comment = results[0].Comment;

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

            using (var connection = new SQLiteConnection(DatabaseName))
            {
                int rowsAffected;
                var drawing = connection.Table<Drawing>().FirstOrDefault(v => v.FileName == drawingName);
                if (drawing == null)
                {
                    //Don't save if comment is empty
                    if (comment == string.Empty)
                        return false;

                    //Add new drawing if it doesn't exist in db
                    drawing = new Drawing
                    {
                        Comment = comment,
                        FileName = drawingName
                    };

                    rowsAffected = connection.Insert(drawing);
                    return rowsAffected > 0;
                }

                if (String.Equals(comment, drawing.Comment))
                {
                    //Only update comment if it haven't changed
                    return false;
                }

                const string updateSql = @"UPDATE [Drawing] SET [Comment] = ? WHERE [Id] = ?";

                rowsAffected = connection.Execute(updateSql, comment, drawing.Id);
                return rowsAffected > 0;
            }
        }
    }
}
