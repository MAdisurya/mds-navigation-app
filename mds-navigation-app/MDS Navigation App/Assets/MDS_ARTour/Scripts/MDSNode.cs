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

    [RequireComponent(typeof(Collider))]

    public class MDSNode : MonoBehaviour
    {
        public List<MDSNode> neighbors = new List<MDSNode>();

        public MDSNodeInput nameInputBehaviour;
        public MDSNodeInput answerInputBehaviour;
        
        private float m_HCost;
        private float m_GCost;
        private float m_FCost;
        private float m_Cost;
        
        private NodeInfo m_NodeInfo;

        private MDSNode m_Parent;
        private MDSNode m_NextInList;

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

        public MDSNode NextInList
        {
            get { return m_NextInList; }
            set { m_NextInList = value; }
        }

        // Methods

        void Awake()
        {
            // Disable the node child
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// Method that finds the closest neighbor (node) to this node
        /// </summary>
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

        /// <summary>
        /// Helper method that enables/renders the child node of this node
        /// </summary>
        public void Activate()
        {
            transform.GetChild(0).gameObject.SetActive(true);

            MainController.Instance.GetNodeController().ActiveNodeObjList.Add(this);

            if ((MDSNodeType) m_NodeInfo.nodeType == MDSNodeType.ENDPOINT && m_Parent != null)
            {
                transform.LookAt(m_Parent.transform);
            } 
            else if (NextInList != null)
            {
                transform.LookAt(NextInList.transform);
            }
        }
        
        /// <summary>
        /// Helper method that disables the child node of this node
        /// </summary>
        public void Deactivate()
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        /// Delegate that handles when MDSNode is touched / tapped on
        /// </summary>
        public void OnTouch()
        {
            GUIController guiController = MainController.Instance.GetGUIController();

            // Disable GUI
            guiController.DisableGUI();
            // Show puzzle challenge GUI
            guiController.puzzlePanel.EnablePuzzle();

            // Set image name and answer for puzzle challenge
            guiController.puzzlePanel.PuzzleImageName = m_NodeInfo.name;
            guiController.puzzlePanel.PuzzleAnswer = m_NodeInfo.answer;
            
        }
    }
}