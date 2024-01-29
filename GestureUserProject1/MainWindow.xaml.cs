using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GestureUserProject1
{
    public partial class MainWindow : Window
    {
        private OneDollarRecognizer oneDoll = new OneDollarRecognizer();
        private Point currPoint = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                currPoint = e.GetPosition(this);
                oneDoll.AddPoint(currPoint);
            }
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Call a method to process the drawn gesture
           ProcessGesture();
        }


        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Get the current mouse position on the canvas
                Point newPoint = e.GetPosition(drawingCanvas);

                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = currPoint.X,
                    Y1 = currPoint.Y,
                    X2 = newPoint.X,
                    Y2 = newPoint.Y,
                    StrokeThickness = 2
                };

                // Add the line to the canvas
                drawingCanvas.Children.Add(line);

                // Update currPoint to the new point for the next segment
                currPoint = newPoint;

                // Add the point to the recognizer's point list
                oneDoll.AddPoint(currPoint);
            }
        }

        private void Clear_Button(object sender, RoutedEventArgs e) 
        {
            drawingCanvas.Children.Clear();
            oneDoll.ClearPoints();
        }
        private void ProcessGesture()
        {
            // Resample the points in oneDoll.pointList
            var resampledPoints = oneDoll.Resample(oneDoll.pointList, 64); // Example: resample to 64 points

            // Clear the canvas and draw the resampled gesture
            drawingCanvas.Children.Clear();
            DrawPoints(resampledPoints);
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
                    StrokeThickness = 2
                };
                drawingCanvas.Children.Add(line);
            }
        }



        private void GestureRecognizer()
        {

        }

    }
}
