using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class AStar
    {
        /// <summary>
        /// AStar algorithm method that finds a path using start node, target node, and all nodes.
        /// Returns a list of nodes
        /// </summary> 
        public List<MDSNode> FindPath(MDSNode startNode, MDSNode targetNode, List<MDSNode> allNodes)
        {
            List<MDSNode> openSet = new List<MDSNode>();
            List<MDSNode> closedSet = new List<MDSNode>();

            openSet.Add(startNode);

            while(openSet.Count > 0)
            {
                MDSNode currentNode = openSet[0];

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FCost < currentNode.FCost
                        || (openSet[i].FCost == currentNode.FCost
                            && openSet[i].HCost < currentNode.HCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    Debug.Log("Returning the correct node");

                    return RetracePath(startNode, targetNode);
                }

                foreach (MDSNode connection in currentNode.neighbors)
                {
                    if (!closedSet.Contains(connection))
                    {
                        float costToConnection = currentNode.GCost + GetEstimate(currentNode, connection) + connection.Cost;

                        if (costToConnection < connection.GCost || !openSet.Contains(connection))
                        {
                            connection.GCost = costToConnection;
                            connection.HCost = GetEstimate(connection, targetNode);
                            connection.Parent = currentNode;

                            if (!openSet.Contains(connection))
                            {
                                openSet.Add(connection);
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Method that back tracks from the end node, and creates a path to the start node
        /// </summary>
        private List<MDSNode> RetracePath(MDSNode startNode, MDSNode endNode)
        {
            List<MDSNode> path = new List<MDSNode>();

            MDSNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }

        private float GetEstimate(MDSNode first, MDSNode second)
        {
            float distance;

            float xDistance = Mathf.Abs(first.transform.position.x - first.transform.position.x);
            float yDistance = Mathf.Abs(second.transform.position.z - second.transform.position.z);

            if (xDistance > yDistance)
            {
                distance = 14 * yDistance + 10 * (xDistance - yDistance);
            }
            else
            {
                distance = 14 * xDistance + 10 * (yDistance - xDistance);
            }

            return distance;
        }
    }
}
