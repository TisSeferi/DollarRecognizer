using System.Collections.Generic;
using System.Windows;

namespace GestureUserProject1
{
    public interface GestureInterface
    {
        void AddPoint(Point point);
        List<Point> Resample(List<Point> points, int n);
        List<Point> RotateZero(List<Point> points);
        (List<Point> corners, double width, double height) CalculateBoundingBox(List<Point> points);
        List<Point> ScaleToSquare(List<Point> points, int size);


    }   
}
