using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class NavigationController : MonoBehaviour
    {
        private bool _initialized = false;
        private bool _initializedComplete = false;

        private float maxDistance = 1.1f;

        private int currNodeIndex = 0;

        private AStar m_AStar = new AStar();

        private List<MDSNode> path = new List<MDSNode>();

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

        private void StartNavigation()
        {
            if (!_initialized)
            {
                _initialized = true;

                Debug.Log("Starting Navigation!");

                List<MDSNode> allNodes = MainController.Instance.GetNodeController().NodeObjList;

                Debug.Log("Nodes: " + allNodes.Count);

                MDSNode closestNode = ReturnClosestNode(allNodes, transform.position);

                Debug.Log("Closest node: " + closestNode.gameObject.name);

                MDSNode targetNode = MainController.Instance.GetNodeController().TargetNode;

                Debug.Log("Target node: " + targetNode.gameObject.name);

                // Set neighbor nodes for all nodes
                foreach (MDSNode node in allNodes)
                {
                    node.FindNeighbors(maxDistance);
                }

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
                for (int i = 0; i < path.Count; i++)
                {
                    path[i].NextInList = path[i + 1];
                }

                // Activate the first node
                path[0].Activate();
                _initializedComplete = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_initializedComplete && other.gameObject.tag == "Waypoint")
            {
                currNodeIndex = path.IndexOf(other.GetComponent<MDSNode>());

                if (currNodeIndex < path.Count - 1)
                {
                    // Activate the next node
                    path[currNodeIndex].NextInList.Activate();
                }
            }
        }
    }
}
