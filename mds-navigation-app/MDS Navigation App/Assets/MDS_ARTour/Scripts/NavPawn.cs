using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class NavPawn : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (MainController.Instance.GetNodeController().EraseMode)
            {
                if (other.GetComponent<MDSNode>() != null)
                {
                    MainController.Instance.GetNodeController().ClearNode(other.GetComponent<MDSNode>());
                }
            }

            MainController.Instance.GetNavigationController().OnTrigger(other);
        }
    }
}
