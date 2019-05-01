using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class ArrowBehaviour : MonoBehaviour
    {
        public int _defaultRotation = 0;    // The default rotation of the arrow sprite

        void Update()
        {
            if (MainController.Instance.GetNodeController().TargetNode != null)
            {
                LookAt(MainController.Instance.GetNodeController().TargetNode.transform);
            }
        }

        private void LookAt(Transform target)
        {
            Vector2 targetVPPos = Camera.main.WorldToViewportPoint(target.position);

            float angle = Mathf.Atan2(targetVPPos.y, targetVPPos.x) * 90;

            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
