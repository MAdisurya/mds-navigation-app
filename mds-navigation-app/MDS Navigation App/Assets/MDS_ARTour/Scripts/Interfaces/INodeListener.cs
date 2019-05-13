using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARTour
{
    public interface INodeListener
    {
        void OnTargetNodeChanged(MDSNode newTargetNode);     // Callback for when target node has been changed
    } 
}