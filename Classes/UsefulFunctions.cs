using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input.Custom;

namespace binaryTreeExample.Classes
{
    class UsefulFunctions
    {
        /// <summary>
        /// This function finds start and end points in a series of lines
        /// </summary>
        /// <param name="lineCurves"></param>
        /// <param name="doc"></param>
        public static List<double> SweepLineDomain(List<Curve> lineCurves, RhinoDoc doc)
        {
            //1. initialize an empty line segment start point / end point
            Point3d[] Qst = new Point3d[lineCurves.Count];
            Point3d[] Qend = new Point3d[lineCurves.Count];

            int n = Qst.Length;

            List<double> max_min = new List<double>();

            //calculate the span between the max and the min y values of the start
            //and end points of the curves on the canvas regarding the y values
            Point3d ptStart;
            Point3d ptEnd;

            double ymax = 0;
            double ymin = 0;

            for (int i = 0; i < lineCurves.Count; i++)
            {
                ptStart = lineCurves[i].PointAtStart;
                ptEnd = lineCurves[i].PointAtEnd;
                Qst[i] = ptStart;
                Qend[i] = ptEnd;

                doc.Objects.AddPoint(Qst[i]);
                doc.Objects.AddPoint(Qend[i]);
                
            }

            //sort lists based on the y coordinate since the sweep line goes in the y direction 
            QuickSort(Qst, 0, n - 1);
            Array.Reverse(Qst);
            QuickSort(Qend, 0, n - 1);

            max_min.Add(Math.Round(Qst[0].Y, 2));
            max_min.Add(Math.Round(Qend[0].Y, 2));

            return max_min;
        }

        /// <summary>
        /// select lines in a document
        /// </summary>
        /// <returns></returns>
        public static List<Curve> SelectCurve()
        {
            GetObject gc = new GetObject();
            gc.SetCommandPrompt("Select curves");
            gc.GeometryFilter = ObjectType.Curve;
            gc.GetMultiple(1, 0);

            int lineCount = 0;
            int arcCount = 0;
            int circleCount = 0;
            int polylineCount = 0;

            //Create a collection of curves
            List<Curve> crvs = new List<Curve>(gc.ObjectCount);

            for (int i = 0; i < gc.ObjectCount; i++)
            {
                Curve crv = gc.Object(i).Curve();
                if (null != crv)
                {
                    crvs.Add(crv);

                    LineCurve line_curve = crv as LineCurve;
                    if (line_curve != null)
                        lineCount++;

                    ArcCurve arc_curve = crv as ArcCurve;
                    if (arc_curve != null)
                    {
                        if (arc_curve.IsCircle())
                            circleCount++;
                        else
                            arcCount++;
                    }

                    PolylineCurve poly_curve = crv as PolylineCurve;
                    if (poly_curve != null)
                        polylineCount++;
                }
            }
            /*
            string s = string.Format(
                "the user selected {0} lines, {1} circles, {2} arcs, {3} polylines and {4} in total",
                lineCount.ToString(), circleCount.ToString(), arcCount.ToString(), polylineCount.ToString(),
                crvs.Count.ToString());

            RhinoApp.WriteLine(s);*/

            return crvs;
        }

        /// <summary>
        /// Select StartPoints for line segments
        /// </summary>
        /// <returns></returns>
        public static Point3d[] PointsStart()
        {
            List<Curve> crvs = SelectCurve();


            //1. initialize an empty event queu Q, insert segment start and endpoints in Q
            Point3d[] ptSt = new Point3d[crvs.Count];

            Point3d ptStart;

            for (int i = 0; i < crvs.Count; i++)
            {
                Hashtable segs = new Hashtable();
                ptStart = crvs[i].PointAtStart;
                segs.Add(i, crvs[i]);
                ptSt[i] = ptStart;
            }

            return ptSt;
        }

        /// <summary>
        /// Select EndPoints for line segments
        /// </summary>
        /// <returns></returns>
        public static Point3d[] PointsEnd()
        {
            List<Curve> crvs = SelectCurve();

            //1. initialize an empty event queu Q, insert segment start and endpoints in Q
            Point3d[] ptE = new Point3d[crvs.Count];

            Point3d ptEnd;

            for (int i = 0; i < crvs.Count; i++)
            {
                ptEnd = crvs[i].PointAtEnd;
                ptE[i] = ptEnd;
            }

            return ptE;
        }

        /// <summary>
        /// handle event points start points, intersection points, end points within the queu Q
        /// </summary>
        /// <param name="p"></param>
        public static Point3d HandleEventPoint(BinaryTree T, Point3d p)
        {
            Point3d intPt = new Point3d();
            TreeNode currentNode = T.Find(Math.Round(p.X, 2)); //node being analyzed at the moment
            if (currentNode != null)
            {
                if (currentNode.LeftNode != null && currentNode.RightNode != null)
                {
                    Curve Sl = currentNode.LeftNode.Data.Value;
                    Curve Sr = currentNode.RightNode.Data.Value;
                    intPt = IntersectionPoint(Sl, Sr);
                }
            }

            return intPt;
        }

        /// <summary>
        /// intersections using the dictionary key/value pair
        /// </summary>
        /// <param name="T"></param>
        /// <returns></returns>
        public static Point3d IntersectionPoint(Curve Sl, Curve Sr)
        {
            Point3d ptA = new Point3d(); //intersection point 
            //check for intersections of pairs of segments left and right according to the status structure (sorted dictionary)
            //take each segment and find left and right neighbors 
            //check for intersection
            CurveIntersections intersect = Intersection.CurveCurve(Sl, Sr, 0.001, 0.001);

            if (intersect.Count >= 1)
            {
                RhinoApp.WriteLine(intersect.Count.ToString());
                ptA = intersect[0].PointA;
            }
            else
            {
                RhinoApp.WriteLine("there are no curve intersections");
            }

            return ptA;
        }

        //a utility function to sort an array of points
        /// <summary>
        /// Swap two elements in an array
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        static void Swap(Point3d[] arr, int i, int j)
        {
            Point3d temp = arr[i];
            arr[i] = arr[j];
            arr[j] = temp;
        }

        /* This function takes last element as pivot, places
             the pivot element at its correct position in sorted
             array, and places all smaller (smaller than pivot)
             to left of pivot and all greater elements to right
             of pivot */
        static int Partition(Point3d[] arr, int low, int high)
        {

            // pivot
            double pivot = arr[high].Y;

            // Index of smaller element and indicates the right position
            // of pivot found so far
            int i = (low - 1);

            for (int j = low; j <= high - 1; j++)
            {

                // If current element is smaller than the pivot
                if (arr[j].Y < pivot)
                {
                    // Increment index of smaller element
                    i++;
                    Swap(arr, i, j);
                }
            }
            Swap(arr, i + 1, high);
            return (i + 1);
        }

        /* The main function that implements QuickSort
                    arr[] --> Array to be sorted,
                    low --> Starting index,
                    high --> Ending index
           */
        static void QuickSort(Point3d[] arr, int low, int high)
        {
            if (low < high)
            {

                // pi is partitioning index, arr[p] is now at right place
                int pi = Partition(arr, low, high);

                // Separately sort elements before partition and after partition
                QuickSort(arr, low, pi - 1);
                QuickSort(arr, pi + 1, high);
            }
        }

        public static Point3d[] SortedList(Point3d[] ptList)
        {
            int n = ptList.Length - 1;

            QuickSort(ptList, 0, n);

            return ptList;
        }

        // Function to print an array
        static void PrintArray(Point3d[] arr, int size)
        {
            for (int i = 0; i < size; i++)
                Console.Write(arr[i] + " ");

            Console.WriteLine();
        }
    }
}
