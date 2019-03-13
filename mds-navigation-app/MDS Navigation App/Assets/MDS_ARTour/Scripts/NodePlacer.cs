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
        public NodeManager m_NodeManager;

        void Awake()
        {
            Assert.IsNotNull(m_NodeManager);
        }

        void Update()
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

                    // Add node to scene, and register in NodeManager
                    m_NodeManager.AddNode(nodeInfo);
                }
            }

            #else

            // For hit testing on device

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == Touch.Ended)
                {
                    // Check if touching a UI button
                    if (EventSystem.current.currentSelectedGameObject == null)
                    {
                        // If not, add a new shape
                        Vector3 screenPos = Camera.main.ScreenToViewPortPoint(touch.position);
                        
                        ARPoint point = new ARPoint
                        {
                            x = screenPos.x;
                            y = screenPos.y;
                        };

                        ARHitTestResultType[] resultTypes = 
                        {
                            ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                            ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                        };
                    }
                }
            }

            #endif
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

                    m_NodeManager.AddNode(nodeInfo);

                    return true;
                }
            }

            return false;
        }
    }
}
