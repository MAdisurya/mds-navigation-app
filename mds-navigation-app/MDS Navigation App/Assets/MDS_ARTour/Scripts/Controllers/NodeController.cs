using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ARTour
{
    // Classes to hold model information
    [System.Serializable]
    public class NodeInfo
    {
        public float px; // position.x
        public float py; // position.y
        public float pz; // position.z

        public int nodeType; // mds node type stored using its raw value - int
    }

    [System.Serializable]
    public class NodeList
    {
        public NodeInfo[] nodes;
    }

    public class NodeController: MonoBehaviour
    {   
        public MDSNode m_WPNodePrefab; // Waypoint node prefab
        public MDSNode m_EPNodePrefab; // Endpoint node prefab

        public MDSNodeType _selectedNodeType = MDSNodeType.WAYPOINT; // Current selected node type, controlled by UI buttons

        private List<MDSNode> m_NodeObjList = new List<MDSNode>();

        // Getters
        public List<MDSNode> NodeObjList
        {
            get { return m_NodeObjList; }
        }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(m_WPNodePrefab);
            Assert.IsNotNull(m_EPNodePrefab);
        }

        /// <summary>
        /// Helper function that changes the selected node type for node placement
        /// </summary>
        public void ChangeSelectedNodeType(MDSNodeType nodeType)
        {
            _selectedNodeType = nodeType;
        }

        /// <summary>
        /// Helper function that changes the selected node type using its raw value - int
        /// </summary>
        public void ChangeSelectedNodeType(int value)
        {
            _selectedNodeType = (MDSNodeType) value;
        }
        
        /// <summary>
        /// Adds a node into the scene
        /// </summary>
        public void AddNode(NodeInfo nodeInfo)
        {
            MDSNode newNode = m_WPNodePrefab;

            MDSNodeType nodeType = (MDSNodeType) nodeInfo.nodeType; // Cast stored node type to MDSNodeType enum
            
            if (nodeType == MDSNodeType.WAYPOINT)
            {
                newNode = Instantiate(m_WPNodePrefab);
            }
            else
            {
                newNode = Instantiate(m_EPNodePrefab);
            }

            newNode.NodeInfo = nodeInfo;
            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            m_NodeObjList.Add(newNode);
        }

        /// <summary>
        /// Clears all nodes in the scene
        /// </summary>
        public void ClearNodes()
        {
            foreach (MDSNode obj in m_NodeObjList)
            {
                Destroy(obj.gameObject);
            }

            m_NodeObjList.Clear();
        }

        /// <summary>
        /// Helper function to convert node to JSON
        /// </summary>
        public JObject NodesToJSON()
        {
            NodeList nodeList = new NodeList();

            nodeList.nodes = new NodeInfo[m_NodeObjList.Count];

            for (int i = 0; i < m_NodeObjList.Count; i++)
            {
                nodeList.nodes[i] = m_NodeObjList[i].NodeInfo;
            }
            return JObject.FromObject(nodeList);
        }     

        /// <summary>
        /// Helper function to convert JSON to node
        /// </summary>
        public void LoadNodesFromJSON(JToken mapMetadata)
        {
            ClearNodes();

            if (mapMetadata is JObject && mapMetadata["nodeList"] is JObject)
            {
                NodeList nodeList = mapMetadata["nodeList"].ToObject<NodeList>();
                
                if (nodeList.nodes == null)
                {
                    Debug.Log("No nodes added.");
                    return;
                }

                foreach (NodeInfo nodeInfo in nodeList.nodes)
                {
                    AddNode(nodeInfo);
                }
            }
        }   
    }
}
