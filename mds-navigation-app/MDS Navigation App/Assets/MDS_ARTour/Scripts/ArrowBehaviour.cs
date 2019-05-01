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

        private void LookAt(Transform target)
        {
            Vector3 targetVPPos = Camera.main.WorldToScreenPoint(target.position);

            if (targetVPPos.z < Camera.main.nearClipPlane)
            {
                // Handles situation where object is behind the camera
                // Need to use this as rotation is not handled well when object behind camera
            }

            float angle = Mathf.Atan2(targetVPPos.y, targetVPPos.x) * 90.0f;

            Debug.Log("angle is: " + angle);

            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
