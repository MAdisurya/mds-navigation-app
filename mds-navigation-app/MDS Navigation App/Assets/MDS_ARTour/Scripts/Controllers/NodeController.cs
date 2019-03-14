using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }

    [System.Serializable]
    public class NodeList
    {
        public NodeInfo[] nodes;
    }

    public class NodeController: MonoBehaviour
    {
        public MDSNode m_NodePrefab;

        private List<MDSNode> m_NodeObjList = new List<MDSNode>();

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(m_NodePrefab);
        }
        
        /// <summary>
        /// Adds a node into the scene
        /// </summary>
        public void AddNode(NodeInfo nodeInfo)
        {
            MDSNode newNode = Instantiate(m_NodePrefab);

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
