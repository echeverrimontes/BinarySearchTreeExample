using Rhino;
using Rhino.Commands;
using System;
using System.Collections.Generic;
using binaryTreeExample.Classes;
using Rhino.Geometry;

namespace binaryTreeExample.Commands
{
    public class QueuEventPoints : Command
    {
        static QueuEventPoints _instance;
        public QueuEventPoints()
        {
            _instance = this;
        }

        ///<summary>The only instance of the QueuEventPoints command.</summary>
        public static QueuEventPoints Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "QueuEventPoints"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            BinaryTree T = new BinaryTree();

            //set of segments for which we want to compute all intersections
            List<Curve> S = UsefulFunctions.SelectCurve();
            int numberSegments = S.Count;
            //find the domain on y of the set of lines starting and end points
            double max = UsefulFunctions.SweepLineDomain(S, doc)[0];
            double min = UsefulFunctions.SweepLineDomain(S, doc)[1];

            Queue<Point3d> Q = new Queue<Point3d>();
            Point3d intPt = new Point3d();
            Queue<Point3d> intPtQueu = new Queue<Point3d>();

            //plane sweep algorithm and the line l sweep line
            //the status of the sweep line is the set of segments intersecting it
            //only at particular points is and update of the status needed: event points (in this algorithm the end points of the segments) 
            double ly = max; //max value in y direction initializes sweep line start
            RhinoApp.WriteLine("max ptY in start points: " + UsefulFunctions.SweepLineDomain(S, doc)[0].ToString());
            RhinoApp.WriteLine("min ptY in end points: " + UsefulFunctions.SweepLineDomain(S, doc)[1].ToString());

            Point3d[] ptStart = UsefulFunctions.PointsStart();
            UsefulFunctions.SortedList(ptStart);
            Array.Reverse(ptStart);
            Point3d[] ptEnd = UsefulFunctions.PointsEnd();
            UsefulFunctions.SortedList(ptEnd);

            bool balanced = true;
            //vertical sweeping line l is updated at particular moments where a new point start or end is encountered
            //particularly at the y position of the points: first encountered 

            //event queu that stores the events
            //if two event points have the same y-coordinate, then the one with smallest x-coordinate will be the first

            for (int j = 0; j < S.Count; j++) //number of segments
            {
                double tempStY = ptStart[j].Y;
                double tempStX = ptStart[j].X;


                //status structure T is an ordered sequence of segments intersecting the sweep line
                //to access the neighbors of a given segment S so that they can be tested for intersection
                //the status structure must be dynamic: inserted and deleted segments as they appear and disappear

                if (ly == tempStY)
                {
                    Q.Enqueue(ptStart[j]);
                    T.Insert(new KeyValuePair<double, Curve>(tempStX, S[j]));
                    /*
                    try
                    {
                        T.Insert(tempStX, S[j]); //root node, first to appear in the binary tree
                    }
                    catch
                    {
                        //RhinoApp.WriteLine("an element with key = {0} already exist", tempStX.ToString());
                    }*/

                }
                else if (Q.Contains(ptStart[j]))
                {
                    Point3d temp = Q.Dequeue();
                    if (Math.Round(temp.X, 2) > Math.Round(tempStX, 2))
                    {
                        Q.Enqueue(ptStart[j]);
                        Q.Enqueue(temp);
                        T.Insert(new KeyValuePair<double, Curve>(tempStX, S[j]));
                        ly = Math.Round(tempStY, 2); //the status of the sweep line is updated to the y location of point
                        /*
                        try
                        {
                            T.Insert(tempStX, S[j]);
                        }
                        catch
                        {
                            //RhinoApp.WriteLine("an element with key = {0} already exist", tempStX.ToString());
                        }*/
                    }
                    else
                    {
                        Q.Enqueue(temp);
                        Q.Enqueue(ptStart[j]);
                        ly = Math.Round(temp.Y, 2); //the status of the sweep line is updated to the y location of point
                    }
                }
                else
                {
                    Q.Enqueue(ptStart[j]);
                    ly = Math.Round(tempStY, 2); //the status of the sweep line is updated to the y location of point
                    T.Insert(new KeyValuePair<double, Curve>(tempStX, S[j]));
                    /*
                    try
                    {
                        T.Insert(tempStX, S[j]);
                    }
                    catch
                    {
                        //RhinoApp.WriteLine("an element with key = {0} already exist", tempStX.ToString());
                    }*/
                }

                //intPt = T.InOrderTraversal();

                //check if the binary tree is balanced at the moment
                balanced = T.IsBalanced(); //if balanced: in order traversal to check for intersections at the moment
                //RhinoApp.WriteLine(balanced.ToString());

                //test for intersection against the ones already intersecting the sweep line
                //order the segments form left to right as they intersect the sweep line
                //test only segments that are adjacent in the horizontal ordering
                //that is test a new segment only with the ones immediately left and right 
                //if (balanced)
                //{

                RhinoApp.WriteLine("Q count  = " + Q.Count.ToString());
                while (Q.Count > 0)
                { 
                    //determine the next event point p in Q and dequeue it
                    //HandleEventPoint(p)
                    Point3d p = Q.Dequeue();
                    Point3d pt = UsefulFunctions.HandleEventPoint(T, p);

                    //U(p) segments whose upper end point is p
                    List<Curve> U = new List<Curve>();
                    TreeNode currentNodeU = T.Find(pt.X);
                    if (currentNodeU != null)
                    {
                        if (currentNodeU.Data.Key < ptStart[j].X || currentNodeU.Data.Key > ptStart[j].X)
                        {
                            U.Add(currentNodeU.Data.Value);
                        }
                    }

                    //L(p) segments whose lower end point is p
                    List<Curve> L = new List<Curve>();
                    TreeNode currentNodeL = T.Find(pt.X);
                    if (currentNodeL != null)
                    {
                        if (currentNodeL.Data.Key < ptEnd[j].X || currentNodeL.Data.Key > ptEnd[j].X)
                        {
                            L.Add(currentNodeL.Data.Value);
                        }
                    }

                    //C(p) segments that contain p in their interior
                    List<Curve> C = new List<Curve>();
                    TreeNode currentNodeC = T.Find(pt.X);
                    if (currentNodeC != null)
                    {
                        if (pt.X < ptStart[j].X || pt.X > ptEnd[j].X)
                        {
                            C.Add(currentNodeC.Data.Value);
                        }
                    }

                    if (currentNodeU != null && currentNodeL != null && currentNodeC != null)
                    {
                        if (U.Contains(currentNodeU.Data.Value) && L.Contains(currentNodeL.Data.Value) &&
                            C.Contains(currentNodeC.Data.Value))
                        {
                            intPtQueu.Enqueue(pt); // intersection points
                        }
                    }
    
    
    
                    intPt = T.InOrderTraversal();
                    //if there are intersections then 
                    if (intPt != null)
                    {
                        TreeNode currentNode = T.Find(j); //node being analyzed at the moment
                        if (intPt.X == ly)
                        {
                            ly = Math.Round(tempStY,
                                2); //the status of the sweep line is updated to the y location of point
                            intPtQueu.Enqueue(intPt);
                            //swap line segments in the binary tree after finding intersections
                            T.SwappNodes(currentNode);
                        }
                    }
                }
          
            }

            foreach (Point3d pt in ptStart)
            {
                double qX = Math.Round(pt.X, 2);
                double qY = Math.Round(pt.Y, 2);
                RhinoApp.Write("(" + qX.ToString() + "," + qY.ToString() + ") ");
            }

            RhinoApp.WriteLine();

            foreach (Point3d pt in ptEnd)
            {
                double qX = Math.Round(pt.X, 2);
                double qY = Math.Round(pt.Y, 2);
                RhinoApp.Write("(" + qX.ToString() + "," + qY.ToString() + ") ");
            }
            
            RhinoApp.WriteLine();
            
            foreach (Point3d pt in Q)
            {
                double qX = Math.Round(pt.X, 2);
                double qY = Math.Round(pt.Y, 2);
                RhinoApp.Write("(" + qX.ToString() + "," + qY.ToString() + ") ");
            }

            foreach (Point3d pt in intPtQueu)
            {
                double ptX = Math.Round(pt.X, 2);
                double ptY = Math.Round(pt.Y, 2);
                //RhinoApp.Write("(" + ptX.ToString() + "," + ptY.ToString() + ") ");
                doc.Objects.AddPoint(pt);
            }
            doc.Views.Redraw();
            /*
            TreeNode node = T.Find(2);
            int depth = T.Height();
            //RhinoApp.WriteLine(node.ToString()); //the key must exist in the list
            RhinoApp.WriteLine(depth.ToString());
            RhinoApp.WriteLine();
            RhinoApp.WriteLine("InOrder Traversal:");
            T.InOrderTraversal();
            RhinoApp.WriteLine(" ");
            /*RhinoApp.WriteLine("PostOrder Traversal:");
            T.PostorderTraversal();
            RhinoApp.WriteLine("PreOrder Traversal After Removing Operation:");
            T.PreorderTraversal();*/

            /*
            RhinoApp.WriteLine(intPtQueu.Count.ToString());
            foreach (Point3d q in Q)
            {
                double qX = Math.Round(q.X, 2);
                double qY = Math.Round(q.Y, 2);
                RhinoApp.Write("(" + qX.ToString() + "," + qY.ToString() + ") ");
            }*/
            
            return Result.Success;
        }
    }
}