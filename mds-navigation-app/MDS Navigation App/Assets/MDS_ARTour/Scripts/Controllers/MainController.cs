using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.iOS; // Import ARKit Library
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ARTour
{
    public class MainController : MonoBehaviour, PlacenoteListener
    {
        // Unity ARKit Session Handler
        private UnityARSessionNativeInterface m_Session;
        
        private static MainController _instance;

        // Reference to controllers
        [SerializeField]
        private SaveAndLoadController _saveAndLoadController;

        [SerializeField]
        private NodeController _nodeController;

        [SerializeField]
        private NavigationController _navigationController;

        // Getters
        public static MainController Instance
        {
            get { return _instance; }
        }

        public SaveAndLoadController GetSaveAndLoadController()
        {
            return _saveAndLoadController;
        }

        public NodeController GetNodeController()
        {
            return _nodeController;
        }

        public NavigationController GetNavigationController()
        {
            return _navigationController;
        }

        // Private constructor for singleton pattern
        private MainController() { }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(_saveAndLoadController);
            Assert.IsNotNull(_nodeController);

            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
            {
                Destroy(this);
            }
        }

        void Start()
        {
            // Initialize ARKit session
            m_Session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
            StartARKit();

            // Initialize Placenote
            FeaturesVisualizer.EnablePointcloud(); // For debugging - seeing the tracked points in the camera
            LibPlacenote.Instance.RegisterListener(this);
        }

        /// <summary>
        /// ARKit Initialize function
        /// </summary>
        private void StartARKit()
        {
            Application.targetFrameRate = 60;

            ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
            config.planeDetection = UnityARPlaneDetection.Horizontal;
            config.alignment = UnityARAlignment.UnityARAlignmentGravity;
            config.getPointCloudData = true;
            config.enableLightEstimation = true;
            
            m_Session.RunWithConfig(config);

            Debug.Log("ARKit enabled");
        }

        // Called when a new pose is received from Placenote
        public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

        // Called when LibPlacenote sends a status change message e.g. Localized!
        public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus) 
        { 
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                _saveAndLoadController.notificationText.text = "Localized!";

                if (_saveAndLoadController.DownloadedMetadata != null)
                {
                    JToken metadata = _saveAndLoadController.DownloadedMetadata.userdata;

                    _nodeController.LoadNodesFromJSON(metadata);

                    try 
                    {
                        // _navigationController.StartNavigation();
                        StartCoroutine(_navigationController.DelayedStartNavigation(4f)); // Not working with delay, still disapearing
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
