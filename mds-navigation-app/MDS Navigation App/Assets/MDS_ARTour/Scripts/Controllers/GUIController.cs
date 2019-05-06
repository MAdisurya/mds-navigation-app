using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ARTour
{
    public class GUIController : MonoBehaviour
    {
        public RectTransform arrowPanel;    // The background panel for the arrow pointer

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
    }
}
