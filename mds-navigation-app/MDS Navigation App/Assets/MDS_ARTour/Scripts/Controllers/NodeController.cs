using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public string name = "node"; // the name of the node - defaults to "node"
    }

    [System.Serializable]
    public class NodeList
    {
        public NodeInfo[] nodes;
    }

    public class NodeController : MonoBehaviour
    {   
        public MDSNode m_WPNodePrefab; // Waypoint node prefab
        public MDSNode m_EPNodePrefab; // Endpoint node prefab

        public MDSNodeType _selectedNodeType = MDSNodeType.WAYPOINT; // Current selected node type, controlled by UI buttons

        private List<MDSNode> m_NodeObjList = new List<MDSNode>();

        private MDSNode m_TargetNode; // Target node for passing into AStar pathfinding algorithm

        private bool _loadCompleted = false;

        // Getters
        public List<MDSNode> NodeObjList
        {
            get { return m_NodeObjList; }
        }

        public MDSNode TargetNode
        {
            get { return m_TargetNode; }
        }

        public bool LoadCompleted
        {
            get { return _loadCompleted; }
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
        /// Helper function that sets the passed nodes name
        /// </summary>
        public void SetNodeName(MDSNode node, string name)
        {
            if (name.Length <= 0)
            {
                MainController.Instance.GetSaveAndLoadController().notificationText.text = "Please enter something in the text field!";
                return;
            }

            node.NodeInfo.name = name;
        }

        /// <summary>
        /// Adds a node into the scene
        /// </summary>
        public void AddNode(NodeInfo nodeInfo)
        {
            MDSNode newNode = m_WPNodePrefab;

            MDSNodeType nodeType = (MDSNodeType) nodeInfo.nodeType;

            if (nodeType == MDSNodeType.WAYPOINT)
            {
                newNode = Instantiate(m_WPNodePrefab);
            }
            else
            {
                newNode = Instantiate(m_EPNodePrefab);

                m_TargetNode = newNode;
            }

            newNode.NodeInfo = nodeInfo;
            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            if (newNode.inputBehaviour != null)
            {
                newNode.inputBehaviour.SetInputText(nodeInfo.name);
            }

            m_NodeObjList.Add(newNode);
        }
        
        /// <summary>
        /// Adds an active node into the scene
        /// </summary>
        public void AddActiveNode(NodeInfo nodeInfo)
        {
            MDSNode newNode = m_WPNodePrefab;

            MDSNodeType nodeType = (MDSNodeType) nodeInfo.nodeType; // Cast stored node typefrom int to MDSNodeType enum
            
            if (nodeType == MDSNodeType.WAYPOINT)
            {
                newNode = Instantiate(m_WPNodePrefab);
            }
            else
            {
                newNode = Instantiate(m_EPNodePrefab);

                m_TargetNode = newNode;
            }

            newNode.Activate();

            newNode.NodeInfo = nodeInfo;
            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            if (newNode.inputBehaviour != null)
            {
                newNode.inputBehaviour.SetInputText(nodeInfo.name);
            }

            m_NodeObjList.Add(newNode);
        }

        /// <summary>
        /// Adds a deactivated node into the scene
        public void AddDeactiveNode(NodeInfo nodeInfo)
        {
            MDSNode newNode = m_WPNodePrefab;

            MDSNodeType nodeType = (MDSNodeType) nodeInfo.nodeType; // Cast stored node type from int to MDSNodeType

            if (nodeType == MDSNodeType.WAYPOINT)
            {
                newNode = Instantiate(m_WPNodePrefab);
            }
            else
            {
                newNode = Instantiate(m_EPNodePrefab);

                m_TargetNode = newNode;
            }

            newNode.Deactivate();

            newNode.NodeInfo = nodeInfo;
            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            if (newNode.inputBehaviour != null)
            {
                newNode.inputBehaviour.SetInputText(nodeInfo.name);
            }

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

                MainController.Instance.GetSaveAndLoadController().LoadTargetNames(nodeList);

                foreach (NodeInfo nodeInfo in nodeList.nodes)
                {
                    AddNode(nodeInfo);
                }
            }
        }
    }
}
