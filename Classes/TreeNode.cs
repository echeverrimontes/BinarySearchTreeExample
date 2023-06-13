using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace binaryTreeExample.Classes
{
    class TreeNode
    {
        private KeyValuePair<double, Curve> _data;

        public KeyValuePair<double, Curve> Data
        {
            get { return _data; }
            set { new KeyValuePair<double, Curve>(_data.Key, _data.Value); }
        }

        private TreeNode rightNode;

        public TreeNode RightNode
        {
            get { return rightNode; }
            set { rightNode = value; }

        } //right child, bigger than the parent node

        private TreeNode leftNode;

        public TreeNode LeftNode
        {
            get { return leftNode; }
            set { leftNode = value; }

        } //left child, smaller than the parent node

        private bool isDeleted; //soft delete variable

        public bool IsDeleted
        {
            get { return isDeleted; }

        }

        Point3d intPt = new Point3d();

        //constructor
        public TreeNode(KeyValuePair<double, Curve> value)
        {
            _data = value;
        }

        //method to set soft delete
        public void Delete()
        {
            isDeleted = true;
        }

        public TreeNode Find(double key)
        {
            //this node is the starting current node
            TreeNode currentNode = this;

            //loop through this node and all of the children of this node
            while (currentNode != null)
            {
                //if the current nodes data is equal to the value passed in return it
                if (key == currentNode.Data.Key && isDeleted == false) //soft delete check
                {
                    return currentNode;

                }
                else if (key > currentNode.Data.Key) //if the value passed in is greater than the current data, go to right child
                {
                    currentNode = currentNode.rightNode;

                }
                else //otherwise if the value is less than the current node data, go to left child
                {
                    currentNode = currentNode.leftNode;

                }

            }

            //node not found
            return null;
        }

        public TreeNode FindRecursive(double key)
        {
            //value passed in matches nodes data return the node
            if (key == Data.Key && isDeleted == false)
            {
                return this;

            }
            else if (key < Data.Key && leftNode != null
            ) //if the value passed in is less than the current data then go to the leftchild
            {
                return leftNode.FindRecursive(key);
            }
            else if (rightNode != null) //if its great than go to the right child node
            {
                return rightNode.FindRecursive(key);

            }
            else
            {
                return null;
            }
        }

        //recursively calls insert down the tree until it find an open spot
        public void Insert(KeyValuePair<double, Curve> value)
        {
            //if the value passed in is greater or equal to the data then insert to right node
            if (value.Key >= _data.Key)
            {
                //if right child node is null create one
                if (rightNode == null)
                {
                    rightNode = new TreeNode(value);
                }
                else
                {
                    //if right node is not null recursively call insert on the right node
                    rightNode.Insert(value);
                }
            }
            else
            {
                //if the value passed in is less than the data then insert to left node
                if (leftNode == null)
                {
                    //if the left node is null then create a new one
                    leftNode = new TreeNode(value);
                }
                else
                {
                    //if the left node is not null then recursively call insert on the left node
                    leftNode.Insert(value);
                }
            }

        }

        //return value in ascending order
        //Left->Root->Right nodes recursively of each subtree
        public Point3d InOrderTraversal()
        {
            //first go to left child, its children will be null so we print its data
            if (leftNode != null)
            {
                //select the curve on the left node and the curve on the right node and check for intersections
                Curve crvLeft = leftNode.Data.Value;
                if (rightNode != null)
                {
                    Curve crvRight = rightNode.Data.Value;
                    intPt = UsefulFunctions.IntersectionPoint(crvLeft, crvRight);
                }
                else
                {
                    leftNode.InOrderTraversal();
                }
                leftNode.InOrderTraversal();
            }

            //then we print the root node
            //RhinoApp.Write("(" + Data.Key + " , " + Data.Value + ") ");
            RhinoApp.Write("(" + intPt.X + " , " + intPt.Y + ") ");

            //then we go to the right node which will print itself as both its children are null
            if (rightNode != null)
            {
                rightNode.InOrderTraversal();
            }

            return intPt;
        }

        //return value in descending order
        //Root->Left->Right nodes recursively of each subtree
        public void PreorderTraversal()
        {
            //first we print the root node
            RhinoApp.WriteLine(Data.Key + " " + Data.Value + " ");

            //then go to left child, its children will be null so we print its data
            if (leftNode != null)
                leftNode.PreorderTraversal();

            //then we go to the right node which will print itself as both its children are null
            if (rightNode != null)
                rightNode.PreorderTraversal();

        }

        //Left->Right->Root nodes recursively of each subtree
        public void PostorderTraversal()
        {
            //first go to left child, its children will be null so we print its data
            if (leftNode != null)
                leftNode.PostorderTraversal();

            //then we go to the right node which will print itself as both its children are null
            if (rightNode != null)
                rightNode.PostorderTraversal();

            //first we print the root node
            Console.WriteLine(Data + " ");

        }

        public Nullable<double> SmallestValue()
        {
            //once we reach the last leaf node we return ist data
            if (leftNode == null)
            {
                return Data.Key;
            }
            else
            {
                //otherwise keep calling the next left node
                return leftNode.SmallestValue();
            }
        }

        public Nullable<double> LargestValue()
        {
            //once we reach the last leaf node we return ist data
            if (rightNode == null)
            {
                return Data.Key;
            }
            else
            {
                //otherwise keep calling the next left node
                return rightNode.LargestValue();
            }
        }

        public int NumberOfLeafNodes()
        {
            //return 1 when leaf nodes is found
            if (this.leftNode == null && this.rightNode == null)
            {
                return 1; //found a leaf node
            }

            int leftLeaves = 0;
            int rightLeaves = 0;

            //recursively call NumberOfLeafNodes returning 1 for each leaf found
            if (this.leftNode != null)
            {
                leftLeaves = leftNode.NumberOfLeafNodes();
            }

            if (this.rightNode != null)
            {
                rightLeaves = rightNode.NumberOfLeafNodes();
            }

            return rightLeaves + rightLeaves;

        }

        public int Height()
        {
            //return 1 when leaf node is found
            if (this.leftNode == null && this.rightNode == null)
            {
                return 1;
            }

            int left = 0;
            int right = 0;

            //recursively got hrough each branch
            if (this.leftNode != null)
                left = this.leftNode.Height();
            if (this.rightNode != null)
                right = this.rightNode.Height();

            if (left > right)
            {
                return (left + 1);
            }
            else
            {
                return (right + 1);
            }
        }

        public bool IsBalanced()
        {
            int leftHeight = leftNode != null ? leftNode.Height() : 0;
            int rightHeight = rightNode != null ? rightNode.Height() : 0;

            int heightDifference = leftHeight - rightHeight;

            if (Math.Abs(heightDifference) > 1)
            {
                return false;
            }
            else
            {
                return ((leftNode != null ? leftNode.IsBalanced() : true) &&
                        (rightNode != null ? rightNode.IsBalanced() : true));
            }

        }

    }
}
