using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.iOS; // Import ARKit Library
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ARTour
{
    public enum Mode: uint
    {
        PLAY_MODE,
        EDITOR_MODE
    }

    // Gyro orientation enum for gyro orientation states
    public enum GyroOrientation: uint
    {
        INITIALIZING = 0,
        FACE_UP,
        FACE_DOWN
    }

    public class MainController : MonoBehaviour, PlacenoteListener
    {   
        public Mode _mode = Mode.PLAY_MODE;

        public Camera _mainCamera;
        public Camera _dummyCamera;
        
        // Unity ARKit Session Handler
        private UnityARSessionNativeInterface m_Session;
        private ARKitWorldTrackingSessionConfiguration m_SessionConfig;
        
        private static MainController _instance;

        private GyroOrientation m_CurrGyroOrientation = GyroOrientation.INITIALIZING;

        // Reference to controllers
        [SerializeField]
        private SaveAndLoadController _saveAndLoadController;

        [SerializeField]
        private NodeController _nodeController;

        [SerializeField]
        private NavigationController _navigationController;

        [SerializeField]
        private GUIController _guiController;

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

        public GyroOrientation GyroOrientation
        {
            get { return m_CurrGyroOrientation; }
        }

        // Private constructor for singleton pattern
        private MainController() { }

        void Awake()
        {
            // Assertions
            Assert.IsNotNull(_saveAndLoadController);
            Assert.IsNotNull(_nodeController);
            Assert.IsNotNull(_mainCamera);
            Assert.IsNotNull(_dummyCamera);

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

            if (_mode == Mode.EDITOR_MODE)
            {
                return;
            }

            Quaternion cameraRotation = _mainCamera.transform.rotation;

            // Modify GUI based on orientation.x
            if (cameraRotation.x > 0.3)
            {
                // Enlarge the arrowPanel
                // _guiController.AnimateArrowPanelSize(new Vector2(4000, 8000), 4.0f);

                // Enlarge arrow, and move arrow up
                _guiController.AnimateArrowSize(new Vector2(800, 800), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 1200), 4.0f);

                // Disable the camera
                DisableCamera();
            }
            else
            {
                // Shrink the arrowPanel
                // _guiController.AnimateArrowPanelSize(new Vector2(2500, 1800), 4.0f);

                // Shrink arrow, and move arrow down
                _guiController.AnimateArrowSize(new Vector2(400, 400), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 400), 4.0f);

                // Enable camera
                EnableCamera();
            }

            #else

            // Calculate the gyro orientation
            GetGyroOrientation();

            if (m_CurrGyroOrientation == GyroOrientation.FACE_DOWN)
            {
                // Enlarge the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(4000, 8000), 4.0f);

                // Enlarge arrow, and move arrow up
                _guiController.AnimateArrowSize(new Vector2(800, 800), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 1200), 4.0f);

                // Disable the camera
                DisableCamera();
            }
            else if (m_CurrGyroOrientation == GyroOrientation.FACE_UP)
            {
                // Shrink the arrowPanel
                _guiController.AnimateArrowPanelSize(new Vector2(2500, 1400), 4.0f);

                // Shrink arrow, and move arrow down
                _guiController.AnimateArrowSize(new Vector2(300, 300), 4.0f);
                _guiController.AnimateArrowPos(new Vector2(0, 300), 4.0f);

                // Enable the camera
                EnableCamera();
            }

            #endif
        }

        void OnGUI()
        {
            // GUI.skin.label.fontSize = Screen.width / 20;

            // GUILayout.BeginArea(new Rect(20, 600, 400, 400));

            // // Label for the gyro attitude
            // GUILayout.Label("Gyro Attitude: " + Input.gyro.attitude);

            // GUILayout.EndArea();
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

            Debug.Log("ARKit enabled");
        }

        /// <summary>
        /// Disables the main camera
        /// </summary>
        public void DisableCamera()
        {
            _mainCamera.enabled = false;
            _dummyCamera.enabled = true;
        }

        /// <summary>
        /// Enables the main camera
        /// </summary>
        public void EnableCamera()
        {
            _dummyCamera.enabled = false;
            _mainCamera.enabled = true;
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

                // Disable the scan panel UI
                _guiController.scanPanel.DisablePanel();

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

        /// <summary>
        /// Calculates orientation of the gyro
        /// </summary>
        private void GetGyroOrientation()
        {
            Quaternion gyroRef = Input.gyro.attitude;
            Quaternion gyroCopy = new Quaternion(gyroRef.x, gyroRef.y, gyroRef.z, gyroRef.w);

            // If gyro.x, gyro.y, and gyro.w are negative, then convert to positive floats
            if (gyroCopy.x < 0) { gyroCopy.x *= -1; }
            if (gyroCopy.y < 0) { gyroCopy.y *= -1; }
            if (gyroCopy.w < 0) { gyroCopy.w *= -1; }

            if (gyroCopy.w > 0.5f)
            {
                // If gyro.w is > than 0.5, then listen to gyro.x
                if (gyroCopy.x > 0.2f)
                {
                    m_CurrGyroOrientation = GyroOrientation.FACE_UP;
                }
                else
                {
                    m_CurrGyroOrientation = GyroOrientation.FACE_DOWN;
                }
            }
            else
            {
                // Otherwise, listen to gyro.y
                if (gyroCopy.y > 0.2f)
                {
                    m_CurrGyroOrientation = GyroOrientation.FACE_UP;
                }
                else
                {
                    m_CurrGyroOrientation = GyroOrientation.FACE_DOWN;
                }
            }
        }
    }
}
