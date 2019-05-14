using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class ArrowBehaviour : MonoBehaviour
    {
        public Transform arrowTransformRef;     // A transform reference point

        public float _defaultRotation = 0f;    // The default rotation of the arrow sprite

        void Update()
        {
            // Get reference of main camera transform
            Transform cameraTransform = MainController.Instance._mainCamera.transform;

            // Update the arrow transform reference position and rotation to the main cameras every frame
            arrowTransformRef.position = cameraTransform.position;
            arrowTransformRef.rotation = Quaternion.Euler(0, cameraTransform.localEulerAngles.y, 
                cameraTransform.localEulerAngles.z);

            if (MainController.Instance.GetNodeController().TargetNode != null)
            {
                LookAt(MainController.Instance.GetNodeController().TargetNode.transform);
            }
        }

        /// <summary>
        /// Method that uses a transform, converts it into radius, and turns arrow towards that position
        /// </summary>
        private void LookAt(Transform target)
        {
            Vector3 targetPos = arrowTransformRef.InverseTransformPoint(target.position);

            float targetAngle = -Mathf.Atan2(targetPos.x, targetPos.z) * Mathf.Rad2Deg + _defaultRotation;

            transform.eulerAngles = new Vector3(0, 0, targetAngle);
        }
    }
}
