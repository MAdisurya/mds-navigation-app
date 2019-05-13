using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ARTour
{
    public class GUIController : MonoBehaviour
    {
        public RectTransform arrowPanel;    // The background panel for the arrow pointer
        public RectTransform arrow;     // The arrow for navigation
        
        public PuzzleBehaviour puzzlePanel; // The puzzle challenge GUI parent

        private List<RectTransform> m_AllGUI = new List<RectTransform>();   // A list that holds all the GUI elements

        public void Awake()
        {
            // Assertions
            Assert.IsNotNull(arrowPanel);
            Assert.IsNotNull(arrow);
        }

        public void Start()
        {
            // Add all gui elements into gui list
            m_AllGUI.Add(arrowPanel);
            m_AllGUI.Add(arrow);
        }

        /// <summary>
        /// Turns on all GUI elements registered in GUI controller
        /// </summary>
        public void EnableGUI()
        {
            foreach (RectTransform gui in m_AllGUI)
            {
                gui.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Turns off all GUI elements registered in GUI controller
        /// </summary>
        public void DisableGUI()
        {
            foreach (RectTransform gui in m_AllGUI)
            {
                gui.gameObject.SetActive(false);
            }
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

            if (sizeDifference.x >= 0 && sizeDifference.y >= 0)
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

        /// <summary>
        /// Animates the arrow's size to desired size. Higher speed value = faster animation
        /// </summary>
        public void AnimateArrowSize(Vector2 size, float speed)
        {
            Vector2 sizeDifference = size - arrow.sizeDelta;

            if (sizeDifference.x >= 0 && sizeDifference.y >= 0)
            {
                // Enlarge
                if (sizeDifference.x >= 0.5f || sizeDifference.y >= 0.5f)
                {
                    arrow.sizeDelta += sizeDifference * Time.deltaTime * speed;
                }
            }
            else
            {
                // Shrink
                if (sizeDifference.x <= 0.5f || sizeDifference.y <= 0.5f)
                {
                    arrow.sizeDelta += sizeDifference * Time.deltaTime * speed;
                }
            }
        }

        /// <summary>
        /// Animates the arrow's position to desired pos. Higher speed value = faster animation
        /// </summary>
        public void AnimateArrowPos(Vector2 pos, float speed)
        {
            Vector2 posDifference = pos - arrow.anchoredPosition;

            if (posDifference.x >= 0 && posDifference.y >= 0)
            {
                // Move up
                if (posDifference.x >= 0.5f || posDifference.y >= 0.5f)
                {
                    arrow.anchoredPosition += posDifference * Time.deltaTime * speed;
                }
            }
            else
            {
                // Move down
                if (posDifference.x <= -0.5f || posDifference.y <= -0.5f)
                {
                    arrow.anchoredPosition += posDifference * Time.deltaTime * speed;
                }
            }
        }
    }
}
