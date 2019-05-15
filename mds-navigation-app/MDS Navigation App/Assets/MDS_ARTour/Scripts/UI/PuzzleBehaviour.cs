using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace ARTour
{
    public class PuzzleBehaviour : PanelBehaviour
    {   
        public Image puzzleImage;

        public Text incorrectLabel;

        private List<Sprite> m_PuzzleSprites = new List<Sprite>();

        private string m_ImageName;

        private int m_Answer;

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

            Debug.Log(answer);

            foreach(Sprite spr in m_PuzzleSprites)
            {
                if (spr.name == m_ImageName)
                {
                    puzzleImage.sprite = spr;
                }
            }
        }

        public override void Awake()
        {
            base.Awake();

            // Assertions
            Assert.IsNotNull(puzzleImage);
            Assert.IsNotNull(incorrectLabel);

            // Pre-allocate memory for the puzzle challenge sprites
            Object[] puzzleSprites = Resources.LoadAll("Sprites/Puzzles/", typeof(Sprite));

            foreach(Object spr in puzzleSprites)
            {
                m_PuzzleSprites.Add((Sprite) spr);
            }

            incorrectLabel.enabled = false;
        }

        /// <summary>
        /// Compares passed answer with PuzzleBehaviour.PuzzleAnswer
        /// </summary>
        public void CompareAnswers(int answer)
        {
            if (answer == m_Answer)
            {
                OnAnswerCorrect();
            }
            else
            {
                OnAnswerIncorrect();
            }
        }

        /// <summary>
        /// Overload that compares passed string answer with PuzzleBehaviour.PuzzleAnswer
        /// </summary>
        public void CompareAnswer(string answer)
        {
            int numAnswer = int.Parse(answer);

            if (numAnswer == m_Answer)
            {
                OnAnswerCorrect();
            }
            else
            {
                OnAnswerIncorrect();
            }
        }

        /// <summary>
        /// Event callback handling correct answers
        /// </summary>
        public void OnAnswerCorrect()
        {   
            AnimationComplete completion = DisablePanel;

            MainController.Instance.GetGUIController().correctPanel.AddAnimationCompleteDelegate(completion);
            MainController.Instance.GetGUIController().correctPanel.EnablePanel();

            incorrectLabel.enabled = false;
        }

        /// <summary>
        /// Event callback handling incorrect answers
        /// </summary>
        public void OnAnswerIncorrect()
        {
            incorrectLabel.enabled = true;
        }
    }
}
