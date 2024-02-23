using PdfSharp.Drawing;
using System.Text;

namespace GPCAEventsCheckIn.Helper
{
    public class LinesHelper
    {
        public List<string> GetLines(string text, XFont font, double maxWidth, XGraphics gfx)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');

            StringBuilder currentLine = new StringBuilder();
            double currentLineWidth = 0;

            foreach (string word in words)
            {
                double wordWidth = gfx.MeasureString(word, font).Width;

                if (currentLineWidth + wordWidth <= maxWidth)
                {
                    currentLine.Append(word + " ");
                    currentLineWidth += wordWidth;
                }
                else
                {
                    lines.Add(currentLine.ToString().Trim());
                    currentLine.Clear();
                    currentLine.Append(word + " ");
                    currentLineWidth = wordWidth;
                }
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().Trim());
            }

            return lines;
        }
    }
}
