using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Reflection;


namespace GestureUserProject1
{
    public partial class MainWindow : Window
    {
        private Point currPoint = new Point();
        private OneDollarRecognizer oneDoll = new OneDollarRecognizer();
        private PennyPincher pp = new PennyPincher();
        private PDollar pDoll = new PDollar();
        private List<Point> currentGesturePoints = new List<Point>();
        //private List<Template> templates = TemplateDictionary.loadGestureTemplate();
        private bool isDrawing = false;
        private bool templateLoaded = false;

        public MainWindow()
        {
            InitializeComponent();

            List<Template> storedTemps = TemplateDictionary.loadGestureTemplate();

            foreach (Template template in storedTemps)
            {
                string name = template.Name;
                List<Point> points = template.Points;
                var oneProcessedPoints = OneProcessGesture(points);
                var pennyProcessedPoints = pp.Prepare(points, true);
                oneDoll.addTemplate(name, oneProcessedPoints);
                pp.addTemplate(name, pennyProcessedPoints);
                var pProcessedPoints = pDoll.Normalize(ConvertPS(points), 64);
                pDoll.addTemplate(name, pProcessedPoints);
                templateLoaded = true;
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentGesturePoints.Clear();
            drawingCanvas.Children.Clear();
            isDrawing = true;
            currPoint = e.GetPosition(drawingCanvas);
            currentGesturePoints.Add(currPoint);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            if (currentGesturePoints.Count > 0 && templateLoaded)
            {
                OneRecognizeDrawnGesture();
                PennyRecognizeDrawnGesture();
                PRecognizerDrawnGesture();
            }
        }


        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && isDrawing)
            {
                var newPoint = e.GetPosition(drawingCanvas);
                currentGesturePoints.Add(newPoint);
                DrawPoints(currentGesturePoints);

            }
        }

        public void RecordButton_Click(object sender, RoutedEventArgs e)
        {

            SaveGesture(currentGesturePoints);
            drawingCanvas.Children.Clear();
        }

        private void Clear_Button(object sender, RoutedEventArgs e)
        {
            drawingCanvas.Children.Clear();
            currentGesturePoints.Clear();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadGesture();
        }


        private void DrawPoints(List<Point> points)
        {
            if (points.Count < 2) return;

            for (int i = 1; i < points.Count; i++)
            {
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = points[i - 1].X,
                    Y1 = points[i - 1].Y,
                    X2 = points[i].X,
                    Y2 = points[i].Y,
                    StrokeThickness = 3
                };
                drawingCanvas.Children.Add(line);
            }
        }

        public void SaveGesture(List<Point> points)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Gesture Files (*.txt)|*.txt",
                DefaultExt = "txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, points.Select(p => $"{p.X},{p.Y}"));
            }
        }

        public void LoadGesture()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Gesture Files (*.txt)|*.txt",
                DefaultExt = "txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                List<Point> points = new List<Point>();
                string fileName = openFileDialog.FileName;

                try
                {

                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            var lines = line.Split(',');
                            if (lines.Length == 2)
                            {
                                double x = double.Parse(lines[0]);
                                double y = double.Parse(lines[1]);
                                points.Add(new Point(x, y));
                            }
                        }
                    }

                    string templateName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    var oneProcessedPoints = OneProcessGesture(points);
                    var pennyProcessedPoints = pp.Prepare(points, true);
                    oneDoll.addTemplate(templateName, oneProcessedPoints);
                    pp.addTemplate(templateName, pennyProcessedPoints);
                    var pProcessedPoints = pDoll.Normalize(ConvertPS(points), 64);
                    pDoll.addTemplate(templateName, pProcessedPoints);
                    templateLoaded = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load gesture: {ex.Message}");
                }
            }
        }

        private List<Point> OneProcessGesture(List<Point> points)
        {
            var resampledPoints = oneDoll.Resample(points, 64);
            var rotatedPoints = oneDoll.RotateZero(resampledPoints);
            int size = 64;
            var scaledPoints = oneDoll.ScaleToSquare(rotatedPoints, size);
            var normalizedPoints = oneDoll.TranslatetoOrigin(scaledPoints);

            return normalizedPoints;
        }


        private void OneRecognizeDrawnGesture()
        {
            try
            {
                var processedPoints = OneProcessGesture(currentGesturePoints);
                var (bestMatch, score) = oneDoll.Recognize(processedPoints, oneDoll.templateList);
                if (bestMatch != null)
                {
                    Text(10, 10, $"OneDollar: Best match: {bestMatch.Name}, Score: {score}", Colors.Black);
                }
                else
                {
                    Text(10, 10, "No match found.", Colors.Black);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PennyRecognizeDrawnGesture()
        {
            var (bestMatch, score) = pp.Recognize(currentGesturePoints, pp.templateList);
            if (bestMatch != null)
            {
                Text(10, 22, $"PP: Best match: {bestMatch.Name}, Score: {score}", Colors.Black);
            }
            else
            {
                Text(10, 22, "No match found.", Colors.Black);
            }
        }

        private void PRecognizerDrawnGesture()
        {
            var (bestMatch, score) = pDoll.Recognize(ConvertPS(currentGesturePoints), pDoll.templateList);
            if (bestMatch != null)
            {
                Text(10, 34, $"PR: Best match: {bestMatch.Name}, Score: {score}", Colors.Black);
            }
            else
            {
                Text(10, 34, "No match found.", Colors.Black);
            }
        }

        private List<PointStroke> ConvertPS(List<Point> points)
        {
            List<PointStroke> result = new List<PointStroke>();
            int strokeId = 1;

            foreach (var p in points)
            {
                result.Add(new PointStroke(p.X, p.Y, strokeId));
            }

            return result;
        }

        private void Text(double x, double y, string text, Color color)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = new SolidColorBrush(color);
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            drawingCanvas.Children.Add(textBlock);
        }


    }
}