using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF3DGrid
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int gMax = 5;
        GeometryModel3D[,] groupGrid;

        public MainWindow()
        {
            groupGrid = new GeometryModel3D[gMax, gMax];
            InitializeComponent();
        }

        private void grid_Loaded(object sender, RoutedEventArgs e)
        {

            Model3DGroup mainGroup = new Model3DGroup();

            Rect bRect = new Rect();

            DirectionalLight dLight = new DirectionalLight();
            dLight.Direction = new Vector3D(1, -1, -1);
            dLight.Color = Colors.White;

            mainGroup.Children.Add(dLight);

            for (int i = 0; i < gMax; i++)
            {
                for (int j = 0; j < gMax; j++)
                {
                    float r = i / (float)gMax;
                    float g = j / (float)gMax;
                    int m = i > j ? i : j;
                    float b = 1 - (m/ (float)gMax);
                    Color color = new Color() { ScR = r, ScG = g, ScB = b, ScA = 1 };
                    //Color color = Colors.Brown;
                    groupGrid[i, j] = CreateHexagon(i, j, color);
                    mainGroup.Children.Add(groupGrid[i, j]);


                    Rect3D hexRect = groupGrid[i, j].Bounds;
                    double minX, maxX, minY, maxY;
                    minX = hexRect.X;
                    minY = hexRect.Y;
                    maxX = hexRect.X + hexRect.SizeX;
                    maxY = hexRect.Y + hexRect.SizeY;
                    if (minX < bRect.X) bRect.X = minX;
                    if (maxX > bRect.Right) bRect.Width = maxX - bRect.X;
                    if (minY < bRect.Y) bRect.Y = minY;
                    if (maxY > bRect.Top) bRect.Height = maxY - bRect.Y;
                }
            }

            SetViewPort(grid, bRect);

            ModelVisual3D model = new ModelVisual3D();
            model.Content = mainGroup;
            grid.Children.Add(model);
        }

        private void SetViewPort(Viewport3D v)
        {
            int scaler = 50;
            Vector3D camLoc = new Vector3D(scaler / 3, -scaler / 2, scaler);
            Vector3D camLook = new Vector3D(0, 2, -3);
            Vector3D up = new Vector3D(0, 0, 1);
            v.Camera = new PerspectiveCamera((Point3D)camLoc, camLook, up, 50);
        }

        private void SetViewPort(Viewport3D v, Rect r)
        {
            Vector3D camLoc = new Vector3D(r.X + (r.Width / 2), -(r.Bottom - (r.Height / 3)), r.Bottom - (r.Height / 3));
            Vector3D centerOfMass = new Vector3D(r.X + (r.Width / 2), r.Y + (r.Height / 2), 0);
            Vector3D camLook = centerOfMass - camLoc;
            Vector3D up = new Vector3D(0, 0, 1);
            v.Camera = new PerspectiveCamera((Point3D)camLoc, camLook, up, 60);
        }

        private GeometryModel3D CreateHexagon(int x, int y, Color color)
        {
            double relX = y % 2 == 0 ? x * 2 : (x * 2) + 1;
            double relY = y * 1.5;

            //Lower Left
            Point3D p0 = new Point3D(0, 0.5, 0);
            //Upper Left
            Point3D p1 = new Point3D(0, 1.5, 0);
            //Top
            Point3D p2 = new Point3D(1, 2, 0);
            //Upper Right
            Point3D p3 = new Point3D(2, 1.5, 0);
            //Lower Right
            Point3D p4 = new Point3D(2, 0.5, 0);
            //Bottom
            Point3D p5 = new Point3D(1, 0, 0);

            return new GeometryModel3D()
            {
                Geometry = new MeshGeometry3D()
                {
                    Positions = new Point3DCollection(new Point3D[] { p0, p1, p2, p3, p4, p5 }),
                    TriangleIndices = new Int32Collection(new Int32[] { 0, 5, 4, 0,4,3, 0,3,1, 1,3,2}),
                },
                Material = new DiffuseMaterial(new SolidColorBrush(color)),
                Transform = new TranslateTransform3D(relX, relY, 0), 
            };
        }

        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point mousePos = e.GetPosition(grid);
            PointHitTestParameters hitParams = new PointHitTestParameters(mousePos);
            VisualTreeHelper.HitTest(grid, null, ResultCallBack, hitParams);   
        }

        public HitTestResultBehavior ResultCallBack(HitTestResult result)
        {
                // Did we hit 3D?
            RayHitTestResult rayResult = result as RayHitTestResult;
            if (rayResult != null)
            {
                // Did we hit a MeshGeometry3D?
                RayMeshGeometry3DHitTestResult rayMeshResult =
                    rayResult as RayMeshGeometry3DHitTestResult;

                if (rayMeshResult != null)
                {
                    GeometryModel3D hitgeo = rayMeshResult.ModelHit as GeometryModel3D;
                    DiffuseMaterial dm = hitgeo.Material as DiffuseMaterial;
                    SolidColorBrush b = dm.Brush as SolidColorBrush;
                    Color c = b.Color;
                    c.ScB = 1 - c.ScB;
                    c.ScG = 1 - c.ScG;
                    c.ScR = 1 - c.ScR;
                    hitgeo.Material = new DiffuseMaterial(new SolidColorBrush(c));
                }
            }

            return HitTestResultBehavior.Continue;
        }
    }
}
