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

    public class NodeManager: MonoBehaviour
    {
        public GameObject nodePrefab;

        private List<NodeInfo> m_NodeInfoList = new List<NodeInfo>();
        private List<GameObject> m_NodeObjList = new List<GameObject>();

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(nodePrefab);
        }
        
        /// <summary>
        /// Adds a node into the scene
        /// </summary>
        public void AddNode(NodeInfo nodeInfo)
        {
            m_NodeInfoList.Add(nodeInfo);

            GameObject newNode = Instantiate(nodePrefab);

            newNode.transform.position = new Vector3(nodeInfo.px, nodeInfo.py, nodeInfo.pz);

            m_NodeObjList.Add(newNode);
        }

        /// <summary>
        /// Clears all nodes in the scene
        /// </summary>
        public void ClearNodes()
        {
            foreach (GameObject obj in m_NodeObjList)
            {
                Destroy(obj);
            }

            m_NodeObjList.Clear();
            m_NodeInfoList.Clear();
        }

        /// <summary>
        /// Helper function to convert node to JSON
        /// </summary>
        public JObject Nodes2JSON()
        {
            NodeList nodeList = new NodeList();

            nodeList.nodes = new NodeInfo[m_NodeInfoList.Count];

            for (int i = 0; i < m_NodeInfoList.Count; i++)
            {
                nodeList.nodes[i] = m_NodeInfoList[i];
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
                    Debug.Log("No models added.");
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
