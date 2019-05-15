using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ARTour
{
    public class PanelBehaviour : MonoBehaviour
    {
        public RectTransform parentTransform;

        public Vector2 slideInXY;
        public Vector2 slideOutXY;

        public delegate void AnimationComplete();

        protected enum AnimatingState
        {
            SLIDE_IN,
            SLIDE_OUT,
            NONE
        }

        protected AnimatingState m_CurrAnimState = AnimatingState.NONE;

        protected Vector2 m_CurrSlideXY = new Vector2();

        protected List<AnimationComplete> m_AnimationCompleters = new List<AnimationComplete>();

        public virtual void Awake()
        {
            // Assertions
            Assert.IsNotNull(parentTransform);

            parentTransform.anchoredPosition = slideOutXY;
        }

        public virtual void Update()
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
        /// Animates panel gameobject slide in transition
        /// </summary>
        public void AnimateSlideIn(Vector2 endPos, float speed)
        {
            Vector2 difference = gameObject.GetComponent<RectTransform>().anchoredPosition - endPos;

            if (difference.x > 0.5f || difference.y > 0.5f)
            {
                parentTransform.anchoredPosition -= difference * Time.deltaTime * speed;
            }
            else
            {
                OnAnimationComplete();
            }
        }

        /// <summary>
        /// Animates panel gameobject slide out transition
        /// </summary>
        public void AnimateSlideOut(Vector2 endPos, float speed)
        {
            Vector2 difference = endPos - gameObject.GetComponent<RectTransform>().anchoredPosition;

            if (difference.x > 0.5f || difference.y > 0.5f)
            {
                parentTransform.anchoredPosition += difference * Time.deltaTime * speed;
            }
        }

        /// <summary>
        /// Enables the panel GUI, will automatically animate in after enable
        /// </summary>
        public void EnablePanel()
        {
            m_CurrSlideXY = slideInXY;
            m_CurrAnimState = AnimatingState.SLIDE_IN;
        }

        /// <summary>
        /// Disables the panel GUI, will automatically animate out after disable
        /// </summary>
        public void DisablePanel()
        {
            m_CurrSlideXY = slideOutXY;
            m_CurrAnimState = AnimatingState.SLIDE_OUT;

            ClearAnimationCompleteDelegates();
        }

        /// <summary>
        /// Event callback for when GUI slide in/out animation is completed
        /// </summary>
        public void AddAnimationCompleteDelegate(AnimationComplete completion)
        {
            m_AnimationCompleters.Add(completion);
        }

        /// <summary>
        /// Clears all the animation complete delegates from memory
        /// </summary>
        public void ClearAnimationCompleteDelegates()
        {
            m_AnimationCompleters.Clear();
        }

        /// <summary>
        /// Event callback for when animation is completed
        /// </summary>
        public void OnAnimationComplete()
        {
            foreach(AnimationComplete completion in m_AnimationCompleters)
            {
                completion();
            }
        }
    }
}
