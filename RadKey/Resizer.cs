using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadKey
{
    public partial class RadKey : Form
    {
        public static class Resizer
        { 
            // Indicates the field sizes/positions to be used by the resizing functions.
            private static class FieldData
            {            
                public static class Original
                {
                    public const int resultListHeight = 172;
                    public const int readingBoxHeight = 49;
                }

                public static class Resized
                {
                    public const int resultListHeight = 46;
                    public const int readingBoxHeight = 175;
                }

                public static class ResultsYPos
                {
                    public const int original = 177;
                    public const int resized = 303;
                }
            }

            // Return the Result List and Reading Box to their original size/position.
            public static void OriginalFieldSize(RadKey radKey)
            {
                radKey.readingBox.Size = new Size(radKey.readingBox.Width, FieldData.Original.readingBoxHeight);
                radKey.readingBox.ScrollBars = ScrollBars.None;

                radKey.resultList.Size = new Size(radKey.resultList.Width, FieldData.Original.resultListHeight);
                radKey.resultList.Location = new Point(radKey.resultList.Location.X, FieldData.ResultsYPos.original);
                radKey.meaningBox.Font = new Font(radKey.meaningBox.Font.FontFamily, (float)9);
                radKey.readingBox.Font = new Font(radKey.readingBox.Font.FontFamily, (float)11);
                
            }

            // Shrink the Result List and expand the Reading Box
            public static void ExpandReadingBox(RadKey radKey)
            {
                radKey.readingBox.Size = new Size(radKey.readingBox.Width, FieldData.Resized.readingBoxHeight);
                radKey.readingBox.ScrollBars = ScrollBars.Vertical;

                radKey.resultList.Size = new Size(radKey.resultList.Width, FieldData.Resized.resultListHeight);
                radKey.resultList.Location = new Point(radKey.resultList.Location.X, FieldData.ResultsYPos.resized);
                

                radKey.meaningBox.Font = new Font(radKey.meaningBox.Font.FontFamily, (float)11.25);
                radKey.readingBox.Font = new Font(radKey.readingBox.Font.FontFamily, (float)9.5);
            }
        }
    }
}