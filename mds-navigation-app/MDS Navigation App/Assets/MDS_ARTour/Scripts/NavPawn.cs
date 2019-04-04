using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public class NavPawn : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            MainController.Instance.GetNavigationController().OnTrigger(other);
        }
    }
}
