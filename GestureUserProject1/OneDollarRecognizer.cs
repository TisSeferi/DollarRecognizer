using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GestureUserProject1
{
    internal class OneDollarRecognizer : GestureInterface
    {
        public List<Point> pointList = new List<Point>();
        public List<Template> templateList = new List<Template>();

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
            double I = PathLength(points) / (n - 1);
            double D = 0.0;
            List<Point> newPoints = new List<Point>();
            newPoints.Add(points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                Point pt1 = points[i - 1];
                Point pt2 = points[i];

                double d = Distance(pt1, pt2);
                if ((D + d) >= I)
                {
                    double qx = pt1.X + ((I - D) / d) * (pt2.X - pt1.X);
                    double qy = pt1.Y + ((I - D) / d) * (pt2.Y - pt1.Y);
                    Point q = new Point(qx, qy);
                    newPoints.Add(q);
                    points.Insert(i, q);
                    D = 0.0;
                }
                else
                {
                    D += d;
                }
            }

            if (newPoints.Count == n - 1)
            {
                newPoints.Add(points[points.Count - 1]);
            }

            return newPoints;
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
            var c = Centroid(points);
            var newPoints = new List<Point>();

            foreach (var p in points)
            {
                var x = (p.X - c.X) * Math.Cos(theta) - (p.Y - c.Y) * Math.Sin(theta) + c.X;
                var y = (p.X - c.X) * Math.Sin(theta) + (p.Y - c.Y) * Math.Cos(theta) + c.Y;
                newPoints.Add(new Point(x, y));
            }

            return newPoints;
        }

        public List<Point> ScaleToSquare(List<Point> points, double size)
        {
            var (corners, width, height) = BoundingBox(points);
            var newPoints = new List<Point>();

            foreach (var p in points)
            {
                var q = new Point(p.X * (size / width), p.Y * (size / height));
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
                q.Y = p.Y - c.Y;

                newPoints.Add(q);
            }

            return newPoints;
        }

        public void addTemplate(string name, List<Point> points)
        {
            var template = new Template(name, points);
            templateList.Add(template);
        }


        public (Template bestMatch, double score) Recognize(List<Point> userGesture, List<Template> templates)
        {
            double bestDist = double.PositiveInfinity;
            Template bestMatch = null;
            double size = 128;
            double thetaA = -45 * (Math.PI / 180);
            double thetaB = 45 * (Math.PI / 180);
            double thetaD = 2 * (Math.PI / 180);

            foreach (Template template in templates)
            {
                double distance = DistanceAtBestAngle(userGesture, template, thetaA, thetaB, thetaD);

                if (distance < bestDist)
                {
                    bestDist = distance;
                    bestMatch = template;
                }
            }

            double score = 1.0 - bestDist / (0.5 * size * Math.Sqrt(2));

            return (bestMatch, score);
        }

        public double DistanceAtBestAngle(List<Point> points, Template template, double thetaA, double thetaB, double thetaD)
        {

            double phi = 0.5 * (-1 + Math.Sqrt(5));

            double x1 = phi * thetaA + (1 - phi) * thetaB;
            double f1 = DistanceAtAngle(points, template, x1);
            double x2 = (1 - phi) * thetaA + (phi * thetaB);
            double f2 = DistanceAtAngle(points, template, x2);

            while (Math.Abs(thetaB - thetaA) > thetaD)
            {
                if (f1 < f2)
                {
                    thetaB = x2;
                    x2 = x1;
                    f2 = f1;
                    x1 = phi * thetaA + (1 - phi) * thetaB;
                    f1 = DistanceAtAngle(points, template, x1);
                }

                else
                {
                    thetaA = x1;
                    x1 = x2;
                    f1 = f2;
                    x2 = (1 - phi) * thetaA + (phi * thetaB);
                    f2 = DistanceAtAngle(points, template, x2);
                }

            }

            return Math.Min(f1, f2);
        }

        public double DistanceAtAngle(List<Point> points, Template template, double theta)
        {
            OneDollarRecognizer rotate = new OneDollarRecognizer();
            List<Point> newPoints = new List<Point>(rotate.RotateBy(points, theta));
            double d = PathDistance(newPoints, template.Points);
            return d;
        }

        public double PathDistance(List<Point> A, List<Point> B)
        {
            double d = 0;
            for (int i = 0; i < A.Count; i++)
            {
                d += Distance(A[i], B[i]);
            }

            return d / A.Count;
        }

    }
}