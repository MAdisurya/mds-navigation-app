using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using UnityEngine.XR.iOS; // Import ARKit Library

namespace ARTour
{
    public class MainController : MonoBehaviour, PlacenoteListener
    {
        // Unity ARKit Session Handler
        private UnityARSessionNativeInterface m_Session;
        
        private static MainController _instance;

        // Reference to controllers
        public SaveAndLoadController _saveAndLoadController;

        // Getters
        public static MainController Instance
        {
            get { return _instance; }
        }

        // Private constructor for singleton pattern
        private MainController() { }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(_saveAndLoadController);

            if (_instance == null)
            {
                _instance = this;
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
        }

        // Called when a new pose is received from Placenote
        public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

        // Called when LibPlacenote sends a status change message e.g. Localized!
        public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus) 
        { 
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                _saveAndLoadController.notificationText.text = "Localized!";
            }
        }
    }
}
