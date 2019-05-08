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
        public Camera _camera;
        
        // Unity ARKit Session Handler
        private UnityARSessionNativeInterface m_Session;
        private ARKitWorldTrackingSessionConfiguration m_SessionConfig;
        
        private static MainController _instance;

        // Reference to controllers
        [SerializeField]
        private SaveAndLoadController _saveAndLoadController;

        [SerializeField]
        private NodeController _nodeController;

        [SerializeField]
        private NavigationController _navigationController;

        [SerializeField]
        private GUIController _guiController;

        private bool _sessionRunning = false;

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

        public GUIController GetGUIController()
        {
            return _guiController;
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

        void Update()
        {
            #if UNITY_EDITOR

            Quaternion cameraRotation = _camera.transform.rotation;

            // Modify GUI based on orientation.x
            if (cameraRotation.x > 0.3)
            {
                // Enlarge the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(4000, 8000), 4.0f);

                // Enlarge arrow, and move arrow up
                _guiController.AnimateArrowSize(new Vector2(800, 800), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 1200), 4.0f);

                // Pause the AR session
                PauseSession();
            }
            else
            {
                // Shrink the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(2500, 1800), 4.0f);

                // Shrink arrow, and move arrow down
                _guiController.AnimateArrowSize(new Vector2(400, 400), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 400), 4.0f);
                
                // Start the AR session
                StartSession();
            }

            #else

            // Get the current device orientation using gyro
            Quaternion orientation = Input.gyro.attitude;

            // Modify GUI based on orientation.x
            if (orientation.x < 0.3)
            {
                // Enlarge the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(4000, 8000), 4.0f);

                // Enlarge arrow, and move arrow up
                _guiController.AnimateArrowSize(new Vector2(800, 800), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 1200), 4.0f);

                // Pause the AR session
                PauseSession();
            }
            else
            {
                // Shrink the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(2500, 1800), 4.0f);

                // Shrink arrow, and move arrow down
                _guiController.AnimateArrowSize(new Vector2(400, 400), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 400), 4.0f);
                
                // Start the AR session
                StartSession();
            }

            #endif
        }

        void OnGUI()
        {
            GUI.skin.label.fontSize = Screen.width / 20;

            GUILayout.BeginArea(new Rect(20, 600, 400, 400));

            // Label for the gyro attitude
            GUILayout.Label("Gyro Attitude: " + Input.gyro.attitude);

            GUILayout.EndArea();
        }

        /// <summary>
        /// ARKit Initialize function
        /// </summary>
        private void StartARKit()
        {
            Application.targetFrameRate = 60;

            m_SessionConfig = new ARKitWorldTrackingSessionConfiguration();
            m_SessionConfig.planeDetection = UnityARPlaneDetection.Horizontal;
            m_SessionConfig.alignment = UnityARAlignment.UnityARAlignmentGravity;
            m_SessionConfig.getPointCloudData = true;
            m_SessionConfig.enableLightEstimation = true;
            
            m_Session.RunWithConfig(m_SessionConfig);
            _sessionRunning = true;

            Debug.Log("ARKit enabled");
        }

        /// <summary>
        /// Starts the ARKit Session
        /// </summary>
        public void StartSession()
        {  
            if (!_sessionRunning)
            {
                m_Session.RunWithConfig(m_SessionConfig);

                _sessionRunning = true;
            }
        }

        /// <summary>
        /// Pauses the current ARKit Session
        /// </summary>
        public void PauseSession()
        {
            if (_sessionRunning)
            {
                m_Session.Pause();

                // Destroy all ARKitRemoteConnections
                foreach (ARKitRemoteConnection connection in Object.FindObjectsOfType<ARKitRemoteConnection>())
                {
                    Destroy(connection.gameObject);
                }

                _sessionRunning = false;
            }
        }

        // Called when a new pose is received from Placenote
        public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

        // Called when LibPlacenote sends a status change message e.g. Localized!
        public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus) 
        { 
            if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
            {
                _saveAndLoadController.notificationText.text = "Localized!";

                LibPlacenote.Instance.StopSession();

                _saveAndLoadController.SetCanPlaceNodes(true);

                // Manage UI
                _saveAndLoadController.ActivateSaveButton(true);
                _saveAndLoadController.ActivateNewButton(true);
                _saveAndLoadController.locationDropdown.gameObject.SetActive(true);

                if (_saveAndLoadController.DownloadedMetadata != null)
                {
                    JToken metadata = _saveAndLoadController.DownloadedMetadata.userdata;

                    _nodeController.LoadNodesFromJSON(metadata);
                }

                if (_navigationController == null)
                {
                    int pTargetOption = _saveAndLoadController.locationDropdown.value;

                    _nodeController.SetTargetNode(pTargetOption);
                }
                else
                {
                    try
                    {
                        int pTargetOption = _saveAndLoadController.locationDropdown.value;

                        _navigationController.InitStatus = NavInitStatus.INCOMPLETE;
                        _navigationController.NavigateToTarget(pTargetOption);
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
