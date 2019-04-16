using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ARTour
{
    public class ListItem: MonoBehaviour
    {
        public Text itemLabel;

        void Start()
        {
            if (GetComponent<Toggle>() != null)
            {
                GetComponent<Toggle>().onValueChanged.AddListener(delegate { 
                    OnToggleChanged(GetComponent<Toggle>().isOn);
                });
            }
        }

        /// <summary>
        /// Delegate that handles on toggle event changes
        /// </summary>
        public void OnToggleChanged(bool toggle)
        {
            if (toggle)
            {
                MainController.Instance.GetSaveAndLoadController().mapListView.CurrentMapId = itemLabel.text;
                
                MainController.Instance.GetSaveAndLoadController().LoadMap();
            }
        }
    }
}
