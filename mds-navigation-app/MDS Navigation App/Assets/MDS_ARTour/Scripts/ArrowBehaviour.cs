using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class ArrowBehaviour : MonoBehaviour, INodeListener
    {
        public float _defaultRotation = 0f;    // The default rotation of the arrow sprite

        private Vector3 m_TargetPos;     // The target node transform

        void Awake()
        {
            // Register self into the node listener list
            MainController.Instance.GetNodeController().RegisterNodeListener(this);
        }

        void Update()
        {
            if (m_TargetPos != null)
            {
                LookAt(m_TargetPos);
            }
        }

        /// <summary>
        /// Takes in a transform, and calculates the angle for arrow to look at target position
        /// </summary>
        private void LookAt(Vector3 targetPos)
        {
            float targetAngle = -Mathf.Atan2(targetPos.x, targetPos.z) * Mathf.Rad2Deg + _defaultRotation;

            transform.eulerAngles = new Vector3(0, 0, targetAngle);
        }

        /// <summary>
        /// Callback method for when target node has changed.
        /// Implemented from INodeListener interface
        /// </summary>
        public void OnTargetNodeChanged(MDSNode newTargetNode)
        {
            if (newTargetNode.transform != null)
            {
                // Set the target position
                m_TargetPos = Camera.main.transform.InverseTransformDirection(newTargetNode.transform.position);
            }
        }
    }
}
