using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace binaryTreeExample.Classes
{
    class BinaryTree
    {
        //variables
        Point3d intPt = new Point3d();

        //properties
        private TreeNode _root;

        //methods
        public TreeNode Find(double key)
        {
            //if the root is not null then we call the find method on the root
            if (_root != null)
            {
                //call node method find
                return _root.Find(key);
            }
            else
            {
                //the root is null so we return null, nothing to find
                return null;
            }
        }

        public TreeNode FindRecursive(float key)
        {
            //if the root is not null then we call the recursive find method on the root
            if (_root != null)
            {
                //call node method FindRecursive
                return _root.FindRecursive(key);
            }
            else
            {
                //the root is null so we will return null, nothing found
                return null;
            }

        }

        public void Insert(KeyValuePair<double, Curve> data)
        {
            //if the root is not null then we call the Insert method on the root node
            if (_root != null)
            {
                _root.Insert(data);
            }
            else
            {
                //if root is null then we set the root to be a new node based on the data passed in
                _root = new TreeNode(data);
            }

        }

        public void Remove(TreeNode node)
        {
            //set the current and parent node to root, so when we remove we can remove using the parents reference
            TreeNode current = _root;
            TreeNode parent = _root;
            bool isLeftChild = false; //keeps track of which child parent should be removed

            //empty tree
            if (current == null)
            {
                //nothing to be removed, end method
                return;
            }

            //find the node
            //loop through until node is not found or if we found the node with matching data
            while (current != null && current.Data.Key != node.Data.Key)
            {
                //set current node to be new parent reference, then we look at its children
                parent = current;

                //if the data we are looking for is less than the current node the we look at its left child
                if (node.Data.Key < current.Data.Key)
                {
                    current = current.LeftNode;
                    isLeftChild = true; //set the varible to determin which child we are looking at
                }
                else
                {
                    //otherwise we look at its right child
                    current = current.RightNode;
                    isLeftChild = false; //set the variable to determin which child we are looking at
                }

            }

            //if the node is not found nothing to delete just return
            if (current == null)
            {
                return;
            }

            //we found a leaf node aka no children
            if (current.RightNode == null && current.LeftNode == null)
            {
                //the root doesn`t have parent to check what child it is, so just set to null
                if (current == _root)
                {
                    _root = null;
                }
                else
                {
                    //when not the root node
                    //see which child of the parent should be deleted
                    if (isLeftChild)
                    {
                        //remove reference to left child node
                        parent.LeftNode = null;
                    }
                    else
                    {
                        //remove reference to right child node
                        parent.RightNode = null;
                    }
                }
            }
            else if (current.RightNode == null) //current only has left child, so we set the parents node child to be this nodes left child
            {
                //if the current node is the root then we just set root to left child node
                if (current == _root)
                {
                    _root = current.LeftNode;
                }
                else
                {
                    //see which child of the parent should be deleted
                    if (isLeftChild) //is this the right child or left child
                    {
                        //current is left child so we set the left node of the parent to the current nodes right child
                        parent.LeftNode = current.RightNode;
                    }
                    else
                    {
                        //current is right child so we set the right node of the parent to the current nodes right child
                        parent.RightNode = current.RightNode;
                    }
                }
            }
            else //current node has both a left and a right child
            {
                //when both child nodes exist we can go to the right node and then find the leaf node of the left child as this will be the least number
                //that is greater than the current node. It may have right child, so the right child would become..left child of the parent of this leaf aka successor node

                //find successor node aka least greater node
                TreeNode successor = GetSuccessor(current);
                //if the current node is the root node then the new root is the successor node
                if (current == _root)
                {
                    _root = successor;
                }
                else if (isLeftChild)
                {
                    //if this is the left child set the parents left child node as the successor node
                    parent.LeftNode = successor;
                }
                else
                {
                    //if this is the right child set the parents right child node as the successor node
                    parent.RightNode = successor;
                }
            }
        }

        public void SwappNodes(TreeNode rootNode)
        {
            //set the current and parent node to leftNode, so when we remove we can remove using the parents reference
            TreeNode current = rootNode;
            TreeNode parent = rootNode;

            //remove left and swap with right && remove right and swap with left
            if (rootNode != null)
            {
                //make copies of the nodes for removing and swapping
                TreeNode copyLeftNode = parent.LeftNode;
                parent.LeftNode = null;
                TreeNode copyRightNode = parent.RightNode;
                parent.RightNode = null;

                current.LeftNode = copyLeftNode;
                current.RightNode = copyRightNode;
            }
        }

        private TreeNode GetSuccessor(TreeNode node)
        {
            TreeNode parentOfSuccessor = node;
            TreeNode successor = node;
            TreeNode current = node.RightNode;

            //starting at the right child we go down every left child node
            while (current != null)
            {
                parentOfSuccessor = successor;
                successor = current;
                current = current.LeftNode; //go to next left node
            }
            //if successor is not just the right node then
            if (successor != node.RightNode)
            {
                //set the left node on the parent node of the successor node to the right child node of the successor in case it has noe
                parentOfSuccessor.LeftNode = successor.RightNode;
                //attach the right child node of the node being deleted to the successors right node
                successor.RightNode = node.RightNode;
            }
            //attach the left child node tof the node being deleted to the successor left node 
            successor.LeftNode = node.LeftNode;

            return successor;
        }

        public void SoftDelete(KeyValuePair<float, Curve> data)
        {
            //find node then set property isDeleed to true
            TreeNode toDelete = Find(data.Key);
            if (toDelete != null)
            {
                toDelete.Delete();
            }

        }

        //find smallest
        public Nullable<double> Smallest()
        {
            //if we have a root node then we can search for the smallest node
            if (_root != null)
            {
                return _root.SmallestValue();
            }
            else
            {
                //otherwise we return null
                return null;
            }

        }

        public Nullable<double> Largest()
        {
            //once we reach the last leaf node we return ist data
            if (_root != null)
            {
                return _root.LargestValue();
            }
            else
            {
                //otherwise we return null
                return null;
            }

        }

        //tree traversal
        //in order - goes left to right basically find the left leaf node then it's parent 
        //then see if the right node has a left node then recursively go up the tree
        //basically keep going left then recursive to parent then right
        //numbers will be in ascending order
        public Point3d InOrderTraversal()
        {
            if (_root != null)
                _root.InOrderTraversal();

            return intPt;
        }

        public void PreorderTraversal()
        {
            if (_root != null)
                _root.PreorderTraversal();

        }

        public void PostorderTraversal()
        {
            if (_root != null)
                _root.PostorderTraversal();


        }

        public int NumberOfLeafNodes()
        {
            //if root is null then number of leafs is zero
            if (_root == null)
            {
                return 0;
            }

            return _root.NumberOfLeafNodes();
        }

        public int Height()
        {
            //if root is null then height is zero
            if (_root == null)
                return 0;

            return _root.Height();
        }

        /// <summary>
        /// check if the tree is balanced. A balanced tree occurs
        /// when the height of two subtrees of any node do not differ in more than 1
        /// </summary>
        /// <returns></returns>
        public bool IsBalanced()
        {
            if (_root == null)
            {
                return true;
            }

            return _root.IsBalanced();
        }

    }
}
