using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace ARTour
{
    public enum NodeInputType: uint
    {
        NAME = 0,
        ANSWER
    }

    public class MDSNodeInput : MonoBehaviour
    {
        public NodeInputType _nodeInputType;

        public MDSNode m_NodeParent;

        public InputField m_InputField;

        void Awake()
        {
            Assert.IsNotNull(m_NodeParent);
            Assert.IsNotNull(m_InputField);
        }

        void Start()
        {
            try
            {
                if (_nodeInputType == NodeInputType.NAME)
                {
                    // Set node name
                    m_InputField.onEndEdit.AddListener(
                        delegate { MainController.Instance.GetNodeController().SetNodeName(m_NodeParent, m_InputField.text); }
                    );
                }
                else
                {
                    // Set node answer
                    m_InputField.onEndEdit.AddListener(
                        delegate { MainController.Instance.GetNodeController().SetNodeAnswer(m_NodeParent, int.Parse(m_InputField.text)); }
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Helper method that sets the input fields text
        /// </summary>
        public void SetInputText(string text)
        {
            if (m_InputField == null)
            {
                Debug.Log("Input field is null!");
                return;
            }

            m_InputField.text = text;
        }
    }
}
