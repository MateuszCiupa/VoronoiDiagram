using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ProjZesp
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>

    enum WorkingMode { AddOutlinePoint, RemoveOutlinePoint, AddKeyPoint, RemoveKeyPoint }

    public partial class MainWindow : Window
    {
        private string InputFilePath { get; set; }
        private FileParser Parser { get; set; }
        private CanvasResizeData ResizeData;
        private MainWindowProperties WindowProperties { get; set; }
        private VoronoiDiagram voronoiDiagram;
        private WorkingMode workingMode;
        private List<System.Windows.Shapes.Line> outlineLinesCanvas;
        private List<System.Windows.Shapes.Line> vornoiDiagramEdges;
        private List<Ellipse> outlinePointsCanvas;
        private List<Ellipse> keyPointsCanvas;

        private struct MainWindowProperties
        {
            public int PointSize { get; set; }
            public int LineThickness { get; set; }
            public SolidColorBrush LineColour { get; set; }
            public SolidColorBrush OutlinePointColour { get; set; }
            public SolidColorBrush KeyPointColour { get; set; }

            public MainWindowProperties(int pointSize, int lineThickness, SolidColorBrush lineColour, SolidColorBrush outlinePointColour, SolidColorBrush keyPointColour) : this()
            {
                PointSize = pointSize;
                LineThickness = lineThickness;
                LineColour = lineColour;
                OutlinePointColour = outlinePointColour;
                KeyPointColour = keyPointColour;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Parser = new FileParser();
            ResizeData = new CanvasResizeData();
            outlineLinesCanvas = new List<System.Windows.Shapes.Line>();
            vornoiDiagramEdges = new List<System.Windows.Shapes.Line>();
            outlinePointsCanvas = new List<Ellipse>();
            keyPointsCanvas = new List<Ellipse>();
            WindowProperties = new MainWindowProperties(10, 2, Brushes.DarkGray, Brushes.Red, Brushes.Blue);
            workingMode = WorkingMode.AddOutlinePoint;
        }

        public void ReadFile()
        {
            InputFilePath = FilePathBox.Text;
            int readResult = Parser.ReadFile(InputFilePath);
            ConsoleBox.Text = Parser.ConsoleLine;
            if (readResult == 0)
            {
                ResizeData.CalculateData(Parser);
                UpdateMapCanvas();
            }       
        }

        private void UpdateMapCanvas()
        {
            UpdateButton.IsEnabled = false;
            ClearMapData();
            Parser.MakeOutlineLines();
            voronoiDiagram = new VoronoiDiagram(Parser.KeyPoints, Parser.OutlineLines);
            voronoiDiagram.GenerateVoronoi();
            DrawOutline();
            DrawKeyPoints();
            DrawVornoiDiagram();
            FileReadingBox.Text = Parser.PrintLists();
        }

        private void DrawOutline()
        {
            foreach (Edge outlineLine in Parser.OutlineLines)
            {
                outlineLinesCanvas.Add(DrawLine(outlineLine));
            }
            foreach (Point outlinePoint in Parser.OutlinePoints)
            {
                outlinePointsCanvas.Add(DrawEllipse(outlinePoint, WindowProperties.OutlinePointColour));
            }
        }

        private void DrawVornoiDiagram()
        {
            foreach(Edge voronoiEdge in voronoiDiagram.Edges)
            {
                vornoiDiagramEdges.Add(DrawLine(voronoiEdge));
            }
        }

        private void DrawKeyPoints()
        {
            foreach (KeyPoint keyPoint in Parser.KeyPoints)
            {
                keyPointsCanvas.Add(DrawEllipse(keyPoint, WindowProperties.KeyPointColour));
            }
        }

        private Ellipse DrawEllipse(Point point, SolidColorBrush colour)
        {
            Ellipse ellipse = new Ellipse { Width = WindowProperties.PointSize, Height = WindowProperties.PointSize };
            ellipse.MouseUp += Ellipse_MouseUp;
            double left = ResizeData.GetResizedX(point.X, MapCanvas.ActualWidth) - (WindowProperties.PointSize / 2);
            double top = ResizeData.GetResizedY(point.Y, MapCanvas.ActualHeight) - (WindowProperties.PointSize / 2);
            ellipse.Margin = new Thickness(left, top, 0, 0);
            ellipse.StrokeThickness = WindowProperties.PointSize;
            ellipse.Stroke = colour;
            MapCanvas.Children.Add(ellipse);
            return ellipse;
        }

        private System.Windows.Shapes.Line DrawLine(Edge edge)
        {
            System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
            line.Stroke = WindowProperties.LineColour;
            line.X1 = ResizeData.GetResizedX(edge.A.X, MapCanvas.ActualWidth);
            line.X2 = ResizeData.GetResizedX(edge.B.X, MapCanvas.ActualWidth);
            line.Y1 = ResizeData.GetResizedY(edge.A.Y, MapCanvas.ActualHeight);
            line.Y2 = ResizeData.GetResizedY(edge.B.Y, MapCanvas.ActualHeight);
            line.StrokeThickness = WindowProperties.LineThickness;
            MapCanvas.Children.Add(line);
            return line;
        }

        private void ClearMapData()
        {
            MapCanvas.Children.Clear();
            outlineLinesCanvas.Clear();
            vornoiDiagramEdges.Clear();
            outlinePointsCanvas.Clear();
            keyPointsCanvas.Clear();
        }

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (((Ellipse)sender).Stroke == WindowProperties.OutlinePointColour && workingMode == WorkingMode.RemoveOutlinePoint)
            {
                if (Parser.IsOutlineCorrect())
                {
                    int position = outlinePointsCanvas.IndexOf((Ellipse)sender);
                    int previousPosition = position == 0 ? outlineLinesCanvas.Count - 1 : position - 1;
                    MapCanvas.Children.Remove((Ellipse)sender);
                    outlinePointsCanvas.Remove((Ellipse)sender);
                    Parser.OutlinePoints.RemoveAt(position);
                    double X = ((Ellipse)sender).Margin.Left + WindowProperties.PointSize / 2;
                    double Y = ((Ellipse)sender).Margin.Top + WindowProperties.PointSize / 2;

                    System.Windows.Shapes.Line line1 = outlineLinesCanvas[position];
                    System.Windows.Shapes.Line line2 = outlineLinesCanvas[previousPosition];
                    if ((line1.X1 == X || line1.X2 == X) && (line1.Y1 == Y || line1.Y2 == Y))
                    {
                        MapCanvas.Children.Remove(line1);
                        outlineLinesCanvas.Remove(line1);
                    }
                    if ((line2.X1 == X || line2.X2 == X) && (line2.Y1 == Y || line2.Y2 == Y))
                    {
                        MapCanvas.Children.Remove(line2);
                        outlineLinesCanvas.Remove(line2);
                    }
                }
                else
                {
                    ConsoleBox.Text = Parser.ConsoleLine;
                }
                if (Parser.AreKeyPointsCorrect())
                {
                    UpdateButton.IsEnabled = true;
                }
                else
                {
                    ConsoleBox.Text = Parser.ConsoleLine;
                }
                UpdateMapCanvas();
            }
            else if (((Ellipse)sender).Stroke == WindowProperties.KeyPointColour && workingMode == WorkingMode.RemoveKeyPoint)
            {
                if (Parser.KeyPoints.Count > 1)
                {
                    int position = keyPointsCanvas.IndexOf((Ellipse)sender);
                    MapCanvas.Children.Remove((Ellipse)sender);
                    keyPointsCanvas.Remove((Ellipse)sender);
                    Parser.KeyPoints.RemoveAt(position);
                    if (Parser.AreKeyPointsCorrect())
                    {
                        UpdateButton.IsEnabled = true;
                    }
                    else
                    {
                        ConsoleBox.Text = Parser.ConsoleLine;
                    }
                    FileReadingBox.Text = Parser.PrintLists();
                }
                else
                {
                    ConsoleBox.Text = "Na mapie musi być co najmniej jeden punkt kluczowy.";
                }
            }
        }

        private void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(workingMode == WorkingMode.AddKeyPoint)
            {
                System.Windows.Point p = Mouse.GetPosition(MapCanvas);
                Point point = new Point(ResizeData.GetRealX(p.X, MapCanvas.ActualWidth), ResizeData.GetRealY(p.Y, MapCanvas.ActualWidth));
                if (PointUtils.IsPointInPolygon(point, Parser.OutlinePoints))
                {
                    KeyPoint keyPoint = new KeyPoint(point.X, point.Y, "Bez nazwy");
                    Parser.KeyPoints.Add(keyPoint);
                    keyPointsCanvas.Add(DrawEllipse(keyPoint, WindowProperties.KeyPointColour));
                    FileReadingBox.Text = Parser.PrintLists();
                    UpdateButton.IsEnabled = true;
                }
                else
                {
                    ConsoleBox.Text = "Nie można dodać punktu poza granicami mapy.";
                }
            }
            else if(workingMode == WorkingMode.AddOutlinePoint)
            {

            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((Button)sender == OpenFileButton)
            {
                ReadFile();
            }
            else if ((Button)sender == UpdateButton)
            {
                UpdateMapCanvas();
            }
        }

        public void Mode_Button_Click(object sender, RoutedEventArgs e)
        {
            if ((Button)sender == AddOutlinePointButton)
            {
                workingMode = WorkingMode.AddOutlinePoint;
                SwitchButtons(AddOutlinePointButton, RemoveOutlinePointButton, AddKeyPointButton, RemoveKeyPointButton);
            }
            else if ((Button)sender == RemoveOutlinePointButton)
            {
                workingMode = WorkingMode.RemoveOutlinePoint;
                SwitchButtons(RemoveOutlinePointButton, AddOutlinePointButton, AddKeyPointButton, RemoveKeyPointButton);
            }
            else if ((Button)sender == AddKeyPointButton)
            {
                workingMode = WorkingMode.AddKeyPoint;
                SwitchButtons(AddKeyPointButton, AddOutlinePointButton, RemoveOutlinePointButton, RemoveKeyPointButton);
            }
            else if ((Button)sender == RemoveKeyPointButton)
            {
                workingMode = WorkingMode.RemoveKeyPoint;
                SwitchButtons(RemoveKeyPointButton, AddOutlinePointButton, RemoveOutlinePointButton, AddKeyPointButton);
            }
        }

        private void SwitchButtons(Button chosenButton, Button otherButton1, Button otherButton2, Button otherButton3)
        {
            chosenButton.Background = new SolidColorBrush(Color.FromRgb(165, 165, 165));
            otherButton1.Background = otherButton2.Background = otherButton3.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ImagePathBox.IsEnabled = (bool)ApplyImageCheckbox.IsChecked;
            OpenImageButton.IsEnabled = (bool)ApplyImageCheckbox.IsChecked;
        }
    }
    public class CanvasResizeData
    {
        private double boundRatio;
        public Point LowerLeftCorner { get; set; }
        public double Length { get; set; }

        public CanvasResizeData()
        {
            LowerLeftCorner = new Point();
            Length = 0;
            boundRatio = 0.1;
        }

        public double GetResizedX(double x, double canvasWidth)
        {
            return Math.Round((boundRatio * canvasWidth) + (canvasWidth / Length) * (x - LowerLeftCorner.X) * (1 - 2 * boundRatio), 2);
        }

        public double GetRealX(double x, double canvasWidth)
        {
            return Math.Round(LowerLeftCorner.X + (x - boundRatio * canvasWidth) / ((canvasWidth / Length) * (1 - 2 * boundRatio)), 2);
        }

        public double GetResizedY(double y, double canvasWidth)
        {
            return Math.Round(-(-(boundRatio * canvasWidth) + (canvasWidth / Length) * (y - LowerLeftCorner.Y) * (1 - 2 * boundRatio)), 2);
        }

        public double GetRealY(double y, double canvasWidth)
        {
            return Math.Round(LowerLeftCorner.Y + (boundRatio * canvasWidth - y) / ((canvasWidth / Length) * (1 - 2 * boundRatio)), 2);
        }

        public void CalculateData(FileParser parser)
        {
            double[] coordinates = new double[parser.OutlinePoints.Count];
            double xMin = 0, xMax = 0, yMin = 0, yMax = 0;
            for (int i = 0; i < parser.OutlinePoints.Count; i++)
            {
                coordinates[i] = parser.OutlinePoints[i].X;
            }
            xMin = coordinates.Min();
            xMax = coordinates.Max();
            for (int i = 0; i < parser.OutlinePoints.Count; i++)
            {
                coordinates[i] = parser.OutlinePoints[i].Y;
            }
            yMin = coordinates.Min();
            yMax = coordinates.Max();
            LowerLeftCorner.X = xMin;
            LowerLeftCorner.Y = yMax;
            Length = xMax - xMin > yMax - yMin ? xMax - xMin : yMax - yMin;
        }
    }
}