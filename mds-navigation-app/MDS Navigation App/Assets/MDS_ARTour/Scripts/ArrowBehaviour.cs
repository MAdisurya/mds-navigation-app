using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class ArrowBehaviour : MonoBehaviour
    {
        public float _defaultRotation = 0f;    // The default rotation of the arrow sprite

        void Update()
        {
            if (MainController.Instance.GetNodeController().TargetNode != null)
            {
                LookAt(MainController.Instance.GetNodeController().TargetNode.transform);
            }
        }

        /// <summary>
        /// Method that uses a transform, converts it into radius, and turns arrow towards that position
        /// The position of target is relative to the main camera position.
        /// </summary>
        private void LookAt(Transform target)
        {
            Vector3 targetPos = MainController.Instance._mainCamera.transform.InverseTransformPoint(target.position);

            float targetAngle = -Mathf.Atan2(targetPos.x, targetPos.z) * Mathf.Rad2Deg + _defaultRotation;

            // If device is facing down, then use targetPos.y instead of targetPos.z as orientation changes
            if (MainController.Instance.GyroOrientation == GyroOrientation.FACE_DOWN)
            {
                targetAngle = -Mathf.Atan2(targetPos.x, targetPos.y) * Mathf.Rad2Deg + _defaultRotation;
            }

            transform.eulerAngles = new Vector3(0, 0, targetAngle);
        }
    }
}
