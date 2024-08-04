using System;
using System.Collections.Generic;
using System.Windows;

namespace GestureUserProject1
{
    internal class PennyPincher : GestureInterface
    {
        public List<Template> templateList = new List<Template>();

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

        public List<Point> Prepare(List<Point> points, bool normalize)
        {
            var pts = Resample(points, 64);
            var vecs = new List<Point>();

            for (int i = 1; i < pts.Count; i++)
            {
                var vec = Subtract(pts[i], pts[i - 1]);
                vecs.Add(normalize ? Normalize(vec) : vec);
            }

            return vecs;
        }

        public Point Normalize(Point vector)
        {
            var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            return new Point(vector.X / length, vector.Y / length);
        }

        public Point Subtract(Point point1, Point point2)
        {
            return new Point(point1.X - point2.X, point1.Y - point2.Y);
        }

        
        public (Template bestMatch, double score) Recognize(List<Point> userGesture, List<Template> templates)
        {
            List<Point> c = Prepare(userGesture, true);
            double similarity = double.NegativeInfinity;
            Template T = null;

            foreach (var t in templates)
            {
                double d = 0;
                for (int i = 0; i < c.Count - 1; i++)
                {
                    d = d + t.Points[i].X * c[i].X + t.Points[i].Y * c[i].Y;
                }

                if (d > similarity)
                {
                    similarity = d;
                    T = t;
                }
            }

            return (T, similarity);
        }

        public void addTemplate(string name, List<Point> points)
        {
            var template = new Template(name, points);
            templateList.Add(template);
        }

    }
}