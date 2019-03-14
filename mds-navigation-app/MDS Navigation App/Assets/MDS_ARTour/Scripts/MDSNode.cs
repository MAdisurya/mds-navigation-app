using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Collider))]
    public class MDSNode : MonoBehaviour
    {
        private NodeInfo m_NodeInfo;

        // Getters
        public NodeInfo NodeInfo
        {
            get { return m_NodeInfo; }
            set { m_NodeInfo = value; }
        }
    }
}