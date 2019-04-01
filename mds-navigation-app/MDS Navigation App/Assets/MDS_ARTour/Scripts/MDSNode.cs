using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    [System.Serializable]

    // Enums for the different MDS node types
    public enum MDSNodeType : int
    {
        WAYPOINT = 0,
        ENDPOINT
    }

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Collider))]

    public class MDSNode : MonoBehaviour
    {
        public List<MDSNode> neighbors = new List<MDSNode>();
        
        private float m_HCost;
        private float m_GCost;
        private float m_FCost;
        private float m_Cost;
        
        private NodeInfo m_NodeInfo;

        private MDSNode m_Parent;

        // Getters & Setters
        public float HCost
        {
            get { return m_HCost; }
            set { m_HCost = value; }
        }

        public float GCost 
        {
            get { return m_GCost; }
            set { m_GCost = value; }
        }

        public float FCost
        {
            get { return m_GCost + m_HCost; }
        }

        public float Cost
        {
            get { return m_Cost; }
            set { m_Cost = value; }
        }

        public NodeInfo NodeInfo
        {
            get { return m_NodeInfo; }
            set { m_NodeInfo = value; }
        }

        public MDSNode Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        // Methods

        public void FindNeighbors(float maxDistance)
        {
            foreach (MDSNode node in MainController.Instance.GetNodeController().NodeObjList)
            {
                if (Vector3.Distance(node.transform.position, this.transform.position) < maxDistance)
                {
                    neighbors.Add(node);
                }
            }
        }
    }
}