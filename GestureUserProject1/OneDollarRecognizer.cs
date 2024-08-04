using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Media.Media3D;

namespace GestureUserProject1
{
    internal class OneDollarRecognizer
    {
        public List<Point> pointList = new List<Point>();
        private double size;

        public void AddPoint(Point point)
        {
            pointList.Add(point);
        }
        
        public void ClearPoints() { pointList.Clear(); }

        public double Distance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
        }

        public List<Point> Resample(List<Point> points, int n)
        {
            double I = PathLength(points) / (n - 1); // Calculate the interval length
            double D = 0.0;
            List<Point> srcPts = new List<Point>(points);
            List<Point> dstPts = new List<Point>(n);
            dstPts.Add(srcPts[0]); // Initialize the destination points with the first point from the source

            for (int i = 1; i < srcPts.Count; i++)
            {
                Point pt1 = srcPts[i - 1];
                Point pt2 = srcPts[i];

                double d = Distance(pt1, pt2);
                if ((D + d) >= I)
                {
                    double qx = pt1.X + ((I - D) / d) * (pt2.X - pt1.X);
                    double qy = pt1.Y + ((I - D) / d) * (pt2.Y - pt1.Y);
                    Point q = new Point(qx, qy);
                    dstPts.Add(q); // Append the new point 'q' to the destination list
                    points.Insert(i, q);
                    D = 0.0;
                }
                else
                {
                    D += d;
                }
            }

            // Sometimes we fall a rounding-error short of adding the last point, so add it if needed
            if (dstPts.Count == n - 1)
            {
                dstPts.Add(srcPts[srcPts.Count - 1]);
            }

            return dstPts;
        }

        public double PathLength(List<Point> points)
        {
            double d = 0;
            for (int i = 1; i < points.Count; i++)
            {
                d += Distance(points[i - 1], points[i]);
            }
            return d;
        }

        public Point Centroid(List<Point> points)
        {
            double xSum = 0;
            double ySum = 0;
            int count = points.Count;

            foreach (var point in points)
            {
                xSum += point.X;
                ySum += point.Y;
            }

            return new Point(xSum / count, ySum / count);
        }

        public List<Point> RotateZero(List<Point> points)
        {
            Point c = Centroid(points);
            double theta = Math.Atan2(c.Y - points[0].Y, c.X - points[0].X);
            List<Point> newPoints = new List<Point>();
            newPoints = RotateBy(points, theta);
            return newPoints;
        }

        public List<Point> RotateBy(List<Point> points, double theta) 
        {
            Point c = Centroid(points);
            List<Point> newPoints = new List<Point>();
            Point q = new Point();

            foreach (var p in points) 
            {
                q.X = (p.X - c.X) * Math.Cos(theta) - (p.Y - c.Y) * Math.Sin(theta) + c.Y;
                q.Y = (p.Y - c.Y) * Math.Sin(theta) - (p.Y - c.Y) * Math.Cos(theta) + c.Y;
                newPoints.Add(q);
            }

            return newPoints;
        }

        public List<Point> ScaleToSquare(List<Point> points, int size)
        {
            List<Point> newPoints = new List<Point>();
            var (corners, Bwidth, Bheight) = BoundingBox(points);
            Point q = new Point();

            foreach (Point p in points)
            {
                q.X = p.X * (size / Bwidth);
                q.Y = p.Y * (size / Bheight);

                newPoints.Add(q);
            }

            return newPoints;
        }

        public (List<Point> corners, double width, double height) BoundingBox(List<Point> points)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            double width = maxX - minX;
            double height = maxY - minY;

            List<Point> corners = new List<Point>
            {
                new Point(minX, minY),
                new Point(maxX, minY),
                new Point(minX, maxY),
                new Point(maxX, maxY)
            };

            return (corners, width, height);
        }


        public List<Point> TranslatetoOrigin(List<Point> points)
        {
            List<Point> newPoints = new List<Point>();
            Point c = Centroid(points);
            Point q = new Point();

            foreach (var p in points)
            {
                q.X = p.X - c.X;
                q.Y= p.Y - c.Y;

                newPoints.Add(q);
            }

            return newPoints;
        }

        /*
        public (Template bestMatch, double score) Recognize(List<Point> userGesture, List<Template> templates)
        {
            double bestDist = double.PositiveInfinity;
            Template bestMatch = null;

            foreach (Template template in templates)
            {
                double distance = DistanceAtBestAngle(userGesture, template);

                if (distance < bestDist)
                {
                    bestDist = distance;
                    bestMatch = template;
                }
            }

            double score = 1.0 - bestDist / (0.5 * Math.Sqrt(2) * size);

            return (bestMatch, score);
        }
        */

    }
}