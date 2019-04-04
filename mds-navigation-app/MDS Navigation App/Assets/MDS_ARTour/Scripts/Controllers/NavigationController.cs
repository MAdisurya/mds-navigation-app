using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public enum NavInitStatus
    {
        INCOMPLETE,
        COMPLETED
    }

    public class NavigationController : MonoBehaviour
    {
        private bool _initialized = false;

        private float maxDistance = 1.1f;

        private MDSNode currentNode;

        private AStar m_AStar = new AStar();

        private List<MDSNode> path = new List<MDSNode>();

        private NavInitStatus m_InitStatus = NavInitStatus.INCOMPLETE;

        public Transform _referencePoint;

        // Getters & Setters
        public NavInitStatus InitStatus
        {
            get { return m_InitStatus; }
        }

        public MDSNode ReturnClosestNode(List<MDSNode> nodes, Vector3 point)
        {
            float minDist = Mathf.Infinity;

            MDSNode closestNode = null;

            foreach (MDSNode node in nodes)
            {
                float dist = Vector3.Distance(node.transform.position, point);

                if (dist < minDist)
                {
                    closestNode = node;
                    minDist = dist;
                }
            }

            return closestNode;
        }

        public void StartNavigation()
        {
            if (!_initialized)
            {
                _initialized = true;

                Debug.Log("Starting Navigation!");

                List<MDSNode> allNodes = MainController.Instance.GetNodeController().NodeObjList;

                Debug.Log("Nodes: " + allNodes.Count);

                MDSNode closestNode = ReturnClosestNode(allNodes, _referencePoint.position);

                Debug.Log("Closest node: " + closestNode.gameObject.name);

                MDSNode targetNode = MainController.Instance.GetNodeController().TargetNode;

                Debug.Log("Target node: " + targetNode.gameObject.name);

                // Set neighbor nodes for all nodes
                foreach (MDSNode node in allNodes)
                {
                    node.FindNeighbors(maxDistance);
                }

                // Get path from AStar algorithm
                path = m_AStar.FindPath(closestNode, targetNode, allNodes);

                if (path == null)
                {
                    // Increase search distance for neighbors if none are found
                    maxDistance += .1f;
                    _initialized = false;

                    Debug.Log("Increasing search distance to: " + maxDistance);

                    StartNavigation();

                    return;
                }

                // Set next nodes
                for (int i = 0; i < path.Count - 1; i++)
                {
                    path[i].NextInList = path[i + 1];
                }

                // Activate the first node
                path[0].Activate();
                m_InitStatus = NavInitStatus.COMPLETED;
            }
        }

        public void OnTrigger(Collider other)
        {
            if (m_InitStatus == NavInitStatus.COMPLETED && other.GetComponent<MDSNode>() != null)
            {
                currentNode = other.GetComponent<MDSNode>();

                if (path.Contains(currentNode))
                {
                    if (currentNode.NextInList != null)
                    {
                        // Activate the next node
                        currentNode.NextInList.Activate();
                    }

                    // Deactivate the current node
                    // currentNode.Deactivate();
                }
            }
        }
    }
}
