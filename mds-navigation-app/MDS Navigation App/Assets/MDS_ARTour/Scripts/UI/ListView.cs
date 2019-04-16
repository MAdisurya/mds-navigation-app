using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARTour
{
    public class ListView : MonoBehaviour
    {
        public GameObject listContent;
        
        public ListItem contentItemPrefab;

        private string m_CurrentMapId;

        // Getters / Setters
        
        public string CurrentMapId
        {
            get { return m_CurrentMapId; }
            set { m_CurrentMapId = value; }
        }

        /// <summary>
        /// Handles adding a list item into the list using the item name
        /// </summary>
        public void AddItem(string itemName)
        {
            ListItem newItem = Instantiate(contentItemPrefab);

            newItem.itemLabel.text = itemName;
            newItem.transform.parent = listContent.transform;
        }
    }
}
