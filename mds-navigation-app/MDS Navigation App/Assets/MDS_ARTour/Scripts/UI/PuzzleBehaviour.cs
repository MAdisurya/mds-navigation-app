using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class PuzzleBehaviour : MonoBehaviour
    {   
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
                gameObject.GetComponent<RectTransform>().anchoredPosition -= difference * Time.deltaTime * speed;
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
                gameObject.GetComponent<RectTransform>().anchoredPosition += difference * Time.deltaTime * speed;
            }
        }

        public void EnablePuzzle()
        {
            m_CurrSlideXY = slideInXY;
            m_CurrAnimState = AnimatingState.SLIDE_IN;
        }

        public void DisablePuzzle()
        {
            m_CurrSlideXY = slideOutXY;
            m_CurrAnimState = AnimatingState.SLIDE_OUT;
        }
    }
}
