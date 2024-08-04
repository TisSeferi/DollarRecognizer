using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GestureUserProject1
{
    internal class PDollar
    {
        public List<TemplateP> templateList = new List<TemplateP>();

        public List<PointStroke> Resample(List<PointStroke> points, int n)
        {
            double I = PathLength(points) / (n - 1);
            double D = 0.0;
            List<PointStroke> newPoints = new List<PointStroke> { points[0] };

            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].StrokeID == points[i - 1].StrokeID)
                {
                    PointStroke pt1 = points[i - 1];
                    PointStroke pt2 = points[i];
                    double d = EuclideanDistance(pt1, pt2);

                    if ((D + d) >= I)
                    {
                        double qx = pt1.X + ((I - D) / d) * (pt2.X - pt1.X);
                        double qy = pt1.Y + ((I - D) / d) * (pt2.Y - pt1.Y);
                        PointStroke q = new PointStroke(qx, qy, pt1.StrokeID);
                        newPoints.Add(q);
                        points.Insert(i, q);
                        D = 0.0;
                    }
                    else
                    {
                        D += d;
                    }
                }
            }
            while (newPoints.Count < n)
            {
                newPoints.Add(points.Last());
            }

            return newPoints;
        }


        public double SqrEuclideanDistance(PointStroke a, PointStroke b)
        {
            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
        }

        public double EuclideanDistance(PointStroke a, PointStroke b)
        {
            return Math.Sqrt(SqrEuclideanDistance(a, b));
        }

        public double PathLength(List<PointStroke> points)
        {
            double d = 0;
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].StrokeID == points[i - 1].StrokeID)
                {
                    d += EuclideanDistance(points[i - 1], points[i]);
                }
            }
            return d;
        }

        public List<PointStroke> Scale(List<PointStroke> points)
        {
            double xmin = double.PositiveInfinity;
            double xmax = 0;
            double ymin = double.PositiveInfinity;
            double ymax = 0;

            foreach (var p in points)
            {
                xmin = Math.Min(xmin, p.X);
                ymin = Math.Min(ymin, p.Y);
                xmax = Math.Max(xmax, p.X);
                ymax = Math.Max(ymax, p.Y);
            }
            var scale = Math.Max(xmax - xmin, ymax - ymin);
            var newPoints = new List<PointStroke>();
            foreach (var p in points)
            {
                newPoints.Add(new PointStroke((p.X - xmin) / scale, (p.Y - ymin) / scale, p.StrokeID));
            }

            return newPoints;
        }

        public List<PointStroke> TranslateToOrigin(List<PointStroke> points, int n)
        {
            var c = (0.0, 0.0);

            foreach (var p in points)
            {
                c = (c.Item1 + p.X, c.Item2 + p.Y);
            }

            c = (c.Item1 / n, c.Item2 / n);
            var newPoints = new List<PointStroke>();
            foreach (var p in points)
            {
                newPoints.Add(new PointStroke(p.X - c.Item1, p.Y - c.Item2, p.StrokeID));
            }

            return newPoints;
        }

        public List<PointStroke> Normalize(List<PointStroke> points, int n)
        {
            var resampledPoints = Resample(points, n);
            var scaledPoints = Scale(resampledPoints);
            var translatedPoints = TranslateToOrigin(scaledPoints, 64);

            return translatedPoints;
        }

        public void addTemplate(string name, List<PointStroke> points)
        {
            var template = new TemplateP(name, points);
            templateList.Add(template);
        }


        public (TemplateP bestMatch, double score) Recognize(List<PointStroke> userGesture, List<TemplateP> templates)
        {
            List<PointStroke> normTemplate = new List<PointStroke>();
            List<PointStroke> newPoints = Normalize(userGesture, 64);
            var score = double.PositiveInfinity;
            TemplateP bestMatch = null;

            foreach (var template in templates)
            {
                normTemplate.AddRange(Normalize(template.Points, 64));
                var d = GreedyCloudMatch(newPoints, normTemplate);

                if (score > d)
                {
                    score = d;
                    bestMatch = template;
                }
                normTemplate.Clear();
            }

            score = Math.Max((2.0 - score) / 2.0, 0.0);

            return (bestMatch, score);
        }

        public double GreedyCloudMatch(List<PointStroke> points, List<PointStroke> templates)
        {
            var epsi = 0.50;
            var step = (int)Math.Floor(Math.Pow(points.Count, 1.0f - epsi));
            var min = double.PositiveInfinity;

            for (var i = 0; i < points.Count; i += step)
            {
                var d1 = CloudDistance(points, templates, i);
                var d2 = CloudDistance(templates, points, i);
                min = Math.Min(min, Math.Min(d1, d2));
            }

            return min;
        }

        public double CloudDistance(List<PointStroke> points, List<PointStroke> tmpl, int start)
        {
            int n = points.Count;
            bool[] matched = new bool[n];
            double sum = 0;
            int i = start;

            do
            {
                int index = -1;
                double min = double.PositiveInfinity;
                for (int j = 0; j < n; j++)
                {
                    if (!matched[j])
                    {
                        double d = EuclideanDistance(points[i], tmpl[j]);
                        if (d < min)
                        {
                            min = d;
                            index = j;
                        }
                    }
                }
                matched[index] = true;
                double weight = 1 - ((i - start + n) % n) / n;
                sum += weight * min;
                i = (i + 1) % n;

            } while (i != start % n);

            return sum;
        }

    }
}