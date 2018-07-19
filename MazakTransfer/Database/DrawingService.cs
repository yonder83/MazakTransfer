using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;

namespace MazakTransfer.Database
{
    public class DrawingService
    {
        public string GetDrawingCommentByName(string drawingName)
        {
            if (drawingName == null) return null;

            string comment = null;

            const string sql = @"SELECT TOP (1)
                                [Extent1].[Comment] AS [Comment]
                                FROM [Drawing] AS [Extent1]
                                WHERE [Extent1].[FileName] = @FileName";

            var connectionString = ConfigurationManager.ConnectionStrings["MazakTransferContext"].ConnectionString;
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
            using (var context = new MazakTransferContext())
            {
                var query = from drawing in context.Drawings
                            where drawing.FileName == drawingName
                            select drawing;

                Drawing drawingToAddComment = query.FirstOrDefault() ?? new Drawing();
                drawingToAddComment.Comment = comment;

                var state = context.Entry(drawingToAddComment).State;
                if (state == System.Data.Entity.EntityState.Detached)
                {
                    drawingToAddComment.FileName = drawingName;
                    context.Drawings.Add(drawingToAddComment);
                }

                int changes = context.SaveChanges();

                return changes > 0;
            }
        }
    }
}
