using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazakTransfer.Database
{
    public class DrawingService
    {
        public Drawing GetDrawingByName(string drawingName)
        {
            using (var context = new MazakTransferContainer())
            {
                var query = from drawing in context.Drawings
                            where drawing.FileName == drawingName
                            select drawing;

                return query.FirstOrDefault();
            }
        }

        /// <summary>
        /// Talentaa kommentin piirrustukselle, palauttaa truen jos tallennus tehtiin.
        /// </summary>
        /// <param name="drawingName"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool SaveCommentByDrawingName(string drawingName, string comment)
        {
            using (var context = new MazakTransferContainer())
            {
                var query = from drawing in context.Drawings
                            where drawing.FileName == drawingName
                            select drawing;

                Drawing drawingToAddComment = query.FirstOrDefault() ?? new Drawing();
                drawingToAddComment.Comment = comment;

                var state = context.Entry(drawingToAddComment).State;
                if (state == System.Data.EntityState.Detached)
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
