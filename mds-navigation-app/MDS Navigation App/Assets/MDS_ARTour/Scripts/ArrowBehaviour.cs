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
        /// Takes in a transform, and calculates the angle for arrow to look at target position
        /// </summary>
        private void LookAt(Transform target)
        {
            Vector3 targetPos = Camera.main.transform.InverseTransformPoint(target.position);

            float targetAngle = -Mathf.Atan2(targetPos.x, targetPos.z) * Mathf.Rad2Deg + _defaultRotation;

            transform.eulerAngles = new Vector3(0, 0, targetAngle);
        }
    }
}
