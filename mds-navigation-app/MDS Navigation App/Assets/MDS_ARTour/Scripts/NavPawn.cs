﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class NavPawn : MonoBehaviour
    {
        void OnTriggerStay(Collider other)
        {
            if (MainController.Instance.GetNodeController().EraseMode)
            {
                if (other.GetComponent<MDSNode>() != null)
                {
                    MainController.Instance.GetNodeController().ClearNode(other.GetComponent<MDSNode>());
                }
            }

            if (MainController.Instance.GetNavigationController() != null)
            {
                MainController.Instance.GetNavigationController().OnTrigger(other);
            }
            else
            {
                if (other.GetComponent<MDSNode>() != null && 
                    other.GetComponent<MDSNode>() == MainController.Instance.GetNodeController().TargetNode)
                {
                    other.GetComponent<MDSNode>().Activate();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<MDSNode>() != null)
            {
                other.GetComponent<MDSNode>().Deactivate();
            }
        }
    }
}
