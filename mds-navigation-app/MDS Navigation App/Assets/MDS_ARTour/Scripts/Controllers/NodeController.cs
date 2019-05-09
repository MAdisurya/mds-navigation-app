using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
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

        private List<MDSNode> m_ActiveNodeObjList = new List<MDSNode>();

        private List<MDSNode> m_TargetNodeObjList = new List<MDSNode>();

        private MDSNode m_TargetNode; // Target node for passing into AStar pathfinding algorithm

        private bool _loadCompleted = false;

        private bool _eraseMode = false;

        // Getters
        public List<MDSNode> NodeObjList
        {
            get { return m_NodeObjList; }
        }

        public List<MDSNode> ActiveNodeObjList
        {
            get { return m_ActiveNodeObjList; }
        }

        public List<MDSNode> TargetNodeObjList
        {
            get { return m_TargetNodeObjList; }
        }

        public MDSNode TargetNode
        {
            get { return m_TargetNode; }
            set { m_TargetNode = value; }
        }

        public bool LoadCompleted
        {
            get { return _loadCompleted; }
        }

        public bool EraseMode
        {
            get { return _eraseMode; }
        }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(m_WPNodePrefab);
            Assert.IsNotNull(m_EPNodePrefab);
        }

        void Update()
        {
            #if UNITY_EDITOR

            // For hit testing in Unity editor simulation
            if (Input.GetMouseButtonDown(0))
            {
                MDSNode node = TouchedEPNode(Input.mousePosition);

                if (node != null)
                {
                    node.OnTouch();
                }
            }

            #else

            // For hit testing on device
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Ended)
                {
                    MDSNode node = TouchedEPNode(touch.position);

                    if (node != null)
                    {
                        node.OnTouch();
                    }
                }
            }

            #endif
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
        /// Helper method that sets the target node from the targetNodeObjList using an int
        /// </summary>
        public void SetTargetNode(int targetIndex)
        {
            m_TargetNode = m_TargetNodeObjList[targetIndex];
        }

        /// <summary>
        /// Helper method that toggles eraser mode
        /// </summary>
        public void ToggleEraserMode(bool toggle)
        {
            _eraseMode = toggle;
        }

        /// <summary>
        /// Helper method that determines whether an EP node has been tapped on, and returns that node
        /// </summary>
        public MDSNode TouchedEPNode(Vector3 touchPos)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(touchPos);

            if (Physics.Raycast(ray, out hit))
            {
                Collider[] colliders = Physics.OverlapSphere(hit.point, 2.0f);

                for (int i = 0; i < colliders.Length; i++)
                {
                    MDSNode node = colliders[i].gameObject.GetComponent<MDSNode>();

                    if (node != null && node.NodeInfo.nodeType == (int) MDSNodeType.ENDPOINT)
                    {
                        return node;
                    }
                }
            }

            return null;
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

            if (nodeType == MDSNodeType.ENDPOINT)
            {
                m_TargetNodeObjList.Add(newNode);
            }
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

            newNode.NodeInfo = nodeInfo;
            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            newNode.Activate();

            if (newNode.inputBehaviour != null)
            {
                newNode.inputBehaviour.SetInputText(nodeInfo.name);
            }

            m_NodeObjList.Add(newNode);

            if (nodeType == MDSNodeType.ENDPOINT)
            {
                m_TargetNodeObjList.Add(newNode);
            }
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

            if (nodeType == MDSNodeType.ENDPOINT)
            {
                m_TargetNodeObjList.Add(newNode);
            }
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
            m_TargetNodeObjList.Clear();
            m_ActiveNodeObjList.Clear();
        }

        /// <summary>
        /// Clears the passed node object from memory
        /// </summary>
        public void ClearNode(MDSNode node)
        {
            if (node == null)
            {
                Debug.Log("Cannot clear node as node is null");
                return;
            }

            m_NodeObjList.Remove(node);
            
            if ((MDSNodeType) node.NodeInfo.nodeType == MDSNodeType.ENDPOINT)
            {
                m_TargetNodeObjList.Remove(node);
            }

            Destroy(node.gameObject);
        }

        /// <summary>
        /// Helper method that deactivates all current active nodes
        /// </summary>
        public void DeactivateNodes()
        {
            foreach (MDSNode obj in m_ActiveNodeObjList)
            {
                obj.Deactivate();
            }

            m_ActiveNodeObjList.Clear();
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

                // Call LoadTargetNames() after Adding all the nodes into memory!
                MainController.Instance.GetSaveAndLoadController().LoadTargetNames(m_TargetNodeObjList);
            }
        }
    }
}
