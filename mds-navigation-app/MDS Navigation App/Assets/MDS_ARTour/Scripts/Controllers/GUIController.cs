using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ARTour
{
    public class GUIController : MonoBehaviour
    {
        public RectTransform arrowPanel;    // The background panel for the arrow pointer

        private bool m_CanAnimate = true;

        public void Awake()
        {
            Assert.IsNotNull(arrowPanel);
        }

        /// <summary>
        /// Helper method that changes the width and height of the arrow panel
        /// </summary>
        public void ChangeArrowPanelSize(Vector2 size)
        {
            arrowPanel.sizeDelta = size;
        }

        /// <summary>
        /// Animates the arrow panel size to desired size in desired speed. Must be called in Update()!
        /// </summary>
        public void AnimateArrowPanelSize(Vector2 size, float speed)
        {
            Vector2 sizeDifference = size - arrowPanel.sizeDelta;

            if (sizeDifference.x > 0 && sizeDifference.y > 0)
            {
                // Enlarge
                if (sizeDifference.x >= 0.5f || sizeDifference.y >= 0.5f)
                {
                    arrowPanel.sizeDelta += sizeDifference * Time.deltaTime * speed;
                }
            }
            else
            {
                // Shrink
                if (sizeDifference.x <= -0.5f || sizeDifference.y <= -0.5f)
                {
                    arrowPanel.sizeDelta += sizeDifference * Time.deltaTime * speed;
                }
            }
        }
    }
}
