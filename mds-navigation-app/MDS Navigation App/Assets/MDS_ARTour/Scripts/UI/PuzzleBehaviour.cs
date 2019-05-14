using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace ARTour
{
    public class PuzzleBehaviour : MonoBehaviour
    {   
        public RectTransform puzzleParentTransform;

        public Image puzzleImage;
        
        public Vector2 slideInXY;
        public Vector2 slideOutXY;

        private string m_ImageName;

        private int m_Answer;

        private enum AnimatingState
        {
            SLIDE_IN,
            SLIDE_OUT,
            NONE
        }

        private AnimatingState m_CurrAnimState = AnimatingState.NONE;

        private Vector2 m_CurrSlideXY = new Vector2();

        // Getters / Setters
        public string PuzzleImageName
        {
            get { return m_ImageName; }
            set { m_ImageName = value; }
        }

        public int PuzzleAnswer
        {
            get { return m_Answer; }
            set { m_Answer = value; }
        }

        /// <summary>
        /// Helper method that sets the image name and answer
        /// </summary>
        public void SetImageAndAnswer(string name, int answer)
        {
            m_ImageName = name;
            m_Answer = answer;
        }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(puzzleParentTransform);
            Assert.IsNotNull(puzzleImage);

            puzzleParentTransform.anchoredPosition = slideOutXY;
        }
        
        void Update()
        {
            if (m_CurrAnimState == AnimatingState.SLIDE_IN)
            {
                AnimateSlideIn(m_CurrSlideXY, 6.0f);
            }
            else if (m_CurrAnimState == AnimatingState.SLIDE_OUT)
            {
                AnimateSlideOut(m_CurrSlideXY, 6.0f);
            }
        }

        /// <summary>
        /// Animates puzzle gameobject slide in transition
        /// </summary>
        public void AnimateSlideIn(Vector2 endPos, float speed)
        {
            Vector2 difference = gameObject.GetComponent<RectTransform>().anchoredPosition - endPos;

            if (difference.x > 0.5f || difference.y > 0.5f)
            {
                puzzleParentTransform.anchoredPosition -= difference * Time.deltaTime * speed;
            }
        }

        /// <summary>
        /// Animates puzzle gameobject slide out transition
        /// </summary>
        public void AnimateSlideOut(Vector2 endPos, float speed)
        {
            Vector2 difference = endPos - gameObject.GetComponent<RectTransform>().anchoredPosition;

            if (difference.x > 0.5f || difference.y > 0.5f)
            {
                puzzleParentTransform.anchoredPosition += difference * Time.deltaTime * speed;
            }
        }

        /// <summary>
        /// Enables the puzzle GUI, will automatically animate in after enable
        /// </summary>
        public void EnablePuzzle()
        {
            m_CurrSlideXY = slideInXY;
            m_CurrAnimState = AnimatingState.SLIDE_IN;
        }

        /// <summary>
        /// Disables the puzzle GUI, will automatically animate out after disable
        /// </summary>
        public void DisablePuzzle()
        {
            m_CurrSlideXY = slideOutXY;
            m_CurrAnimState = AnimatingState.SLIDE_OUT;
        }
    }
}
