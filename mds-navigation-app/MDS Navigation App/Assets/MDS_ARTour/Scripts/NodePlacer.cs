using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using UnityEngine.Assertions;

namespace ARTour
{
    public class NodePlacer : MonoBehaviour
    {
        public NodeController m_NodeController;

        private bool canPlaceNodes = true;

        // Getters / Setters
        public bool CanPlaceNodes
        {
            get { return canPlaceNodes; }
            set { canPlaceNodes = value; }
        }

        void Awake()
        {
            Assert.IsNotNull(m_NodeController);
        }

        void Update()
        {
            if (canPlaceNodes)
            {
                // Check if screen is touched

                #if UNITY_EDITOR

                // For hit testing in simulation in Unity

                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        // Create node info object
                        NodeInfo nodeInfo = new NodeInfo();
                        nodeInfo.px = hit.point.x;
                        nodeInfo.py = hit.point.y;
                        nodeInfo.pz = hit.point.z;
                        nodeInfo.nodeType = (int) m_NodeController._selectedNodeType;

                        // Add node to scene, and register in NodeManager
                        m_NodeController.AddActiveNode(nodeInfo);
                    }
                }

                #else

                // For hit testing on device

                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);

                    if (touch.phase == TouchPhase.Ended)
                    {
                        // Check if touching a UI button
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                            // If not, add a new shape
                            Vector3 screenPos = Camera.main.ScreenToViewportPoint(touch.position);
                            
                            ARPoint point = new ARPoint
                            {
                                x = screenPos.x,
                                y = screenPos.y
                            };

                            ARHitTestResultType[] resultTypes = 
                            {
                                ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                                ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                            };

                            foreach (ARHitTestResultType resultType in resultTypes)
                            {
                                if (HitTestWithResultType(point, resultType))
                                {
                                    Debug.Log("Found a hit test result");
                                    return;
                                }
                            }
                        }
                    }
                }

                #endif
            }
        }

        private bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultType)
        {
            List<ARHitTestResult> hitResults = 
                UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultType);
            
            if (hitResults.Count > 0)
            {
                foreach (ARHitTestResult hitResult in hitResults)
                {
                    Debug.Log("Got hit!");

                    Vector3 position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    Quaternion rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);

                    // Transform to placenote frame of reference
                    Matrix4x4 worldTransform = Matrix4x4.TRS(position, rotation, Vector3.one);
                    Matrix4x4? placenoteTransform = LibPlacenote.Instance.ProcessPose(worldTransform);

                    Vector3 hitPosition = PNUtility.MatrixOps.GetPosition(placenoteTransform.Value);
                    Quaternion hitRotation = PNUtility.MatrixOps.GetRotation(placenoteTransform.Value);

                    // Create node info object
                    NodeInfo nodeInfo = new NodeInfo();
                    nodeInfo.px = hitPosition.x;
                    nodeInfo.py = hitPosition.y;
                    nodeInfo.pz = hitPosition.z;
                    nodeInfo.nodeType = (int) m_NodeController._selectedNodeType;

                    m_NodeController.AddActiveNode(nodeInfo);

                    return true;
                }
            }

            return false;
        }
    }
}
