using System.Windows;

namespace GestureUserProject1
{
    public class PointStroke
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int? StrokeID { get; set; }


        public PointStroke(double x, double y, int? strokeId = null)
        {
            X = x; Y = y; StrokeID = strokeId;
        }
    }
}