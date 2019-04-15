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
            set { m_InitStatus = value; }
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
            if (m_InitStatus == NavInitStatus.INCOMPLETE)
            {
                Debug.Log("Starting Navigation!");

                List<MDSNode> allNodes = MainController.Instance.GetNodeController().NodeObjList;

                Debug.Log("Nodes: " + allNodes.Count);

                MDSNode closestNode = ReturnClosestNode(allNodes, _referencePoint.position);

                Debug.Log("Closest node: " + closestNode.gameObject.name);

                MDSNode targetNode = MainController.Instance.GetNodeController().TargetNode;

                Debug.Log("Target node: " + targetNode.NodeInfo.name);

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

        /// <summary>
        /// Method that handles re-navigation to a new target using the target name
        /// </summary>
        public void NavigateToTarget(string targetName)
        {
            foreach (MDSNode target in MainController.Instance.GetNodeController().TargetNodeObjList)
            {
                if (targetName == target.NodeInfo.name)
                {
                    MainController.Instance.GetNodeController().TargetNode = target;
                }
            }

            try
            {
                m_InitStatus = NavInitStatus.INCOMPLETE;
                StartNavigation();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Overloaded NavigateToTarget method that handles re-navigation to a new target using int
        /// </summary>
        public void NavigateToTarget(int targetIndex)
        {
            MainController.Instance.GetNodeController().TargetNode = 
                MainController.Instance.GetNodeController().TargetNodeObjList[targetIndex];

            Debug.Log("New target is: " + MainController.Instance.GetNodeController().TargetNode.NodeInfo.name);

            try
            {
                m_InitStatus = NavInitStatus.INCOMPLETE;
                StartNavigation();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
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
