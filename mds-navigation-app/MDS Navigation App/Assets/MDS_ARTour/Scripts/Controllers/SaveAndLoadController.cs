using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ARTour
{
    public class SaveAndLoadController : MonoBehaviour
    {
        // UI GameObject references
        public GameObject newMapButton;
        public GameObject saveMapButton;
        public GameObject loadMapButton;
        public Text notificationText;
        public Dropdown locationDropdown;

        public NodePlacer _nodePlacer;

        // Holds the last saved map ID
        private string m_SavedMapId;

        private bool isLoaded = false;

        private LibPlacenote.MapMetadata m_DownloadedMetadata;

        // Getters
        public LibPlacenote.MapMetadata DownloadedMetadata
        {
            get { return m_DownloadedMetadata; }
        }

        void Awake()
        {
            // Handle Assertions if references are null
            Assert.IsNotNull(newMapButton);
            Assert.IsNotNull(saveMapButton);
            Assert.IsNotNull(loadMapButton);
            Assert.IsNotNull(notificationText);
            Assert.IsNotNull(locationDropdown);

            Assert.IsNotNull(_nodePlacer);
        }

        void Start()
        {
            saveMapButton.SetActive(false);
            locationDropdown.gameObject.SetActive(false);

            locationDropdown.ClearOptions();
        }
        
        /// <summary>
        /// Handles new map clicked event
        /// </summary>
        public void OnNewMapClick()
        {
            notificationText.text = "Scan and click Save Map when complete!";

            newMapButton.SetActive(false);
            loadMapButton.SetActive(false);
            saveMapButton.SetActive(true);
            locationDropdown.gameObject.SetActive(false);

            // Start the LibPlacenote session
            LibPlacenote.Instance.StartSession();
        }

        /// <summary>
        /// Handles saving of the Placenote Map, and uploading it to Placenote Cloud
        /// </summary>
        public void SaveMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            newMapButton.SetActive(true);
            loadMapButton.SetActive(true);
            saveMapButton.SetActive(false);
            locationDropdown.gameObject.SetActive(false);

            LibPlacenote.Instance.SaveMap(
                (mapId) =>
                {
                    if (!isLoaded)
                    {
                        m_SavedMapId = mapId;

                        LibPlacenote.Instance.StopSession();
                        FeaturesVisualizer.clearPointcloud();

                        WriteMapIDToFile(mapId);
                    }
                },
                (completed, faulted, percentage) =>
                {
                    if (completed)
                    {
                        notificationText.text = "Upload Complete: " + m_SavedMapId;

                        // Upload meta data
                        LibPlacenote.MapMetadataSettable metadata = CreateMetaDataObject();

                        LibPlacenote.Instance.SetMetadata(m_SavedMapId, metadata, 
                            (success) =>
                            {
                                if (success)
                                {
                                    notificationText.text = "Meta data successfully saved";
                                }
                                else
                                {
                                    notificationText.text = "Meta data failed to save";
                                }
                            }
                        );

                        // Clear current nodes after saving
                        MainController.Instance.GetNodeController().ClearNodes();
                    }
                    else if (faulted)
                    {
                        notificationText.text = "Upload of map: " + m_SavedMapId + " failed";
                    }
                    else 
                    {
                        notificationText.text = "Upload Progress: " + percentage.ToString("F2") + "/1.0)";
                    }
                }
            );
        }

        /// <summary>
        /// Handles loading of the Placenote map from the Placenote Cloud, and relocalizing
        /// </summary>
        public void LoadMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            // Read saved map Id from file
            m_SavedMapId = ReadMapIDFromFile();

            if (m_SavedMapId == null)
            {
                notificationText.text = "You haven't saved a map yet!";
                return;
            }

            notificationText.text = "Loading map...";

            // Disable node placement
            _nodePlacer.CanPlaceNodes = false;

            newMapButton.SetActive(false);
            saveMapButton.SetActive(false);

            LibPlacenote.Instance.LoadMap(
                m_SavedMapId,
                (completed, faulted, percentage) =>
                {
                    if (completed)
                    {
                        // Download meta data
                        LibPlacenote.Instance.GetMetadata(m_SavedMapId,
                            (LibPlacenote.MapMetadata result) =>
                            {
                                if (result != null)
                                {
                                    // Store result to downloadedMetaData variable, and use in 
                                    // OnStatusChange() callback in MainController 
                                    m_DownloadedMetadata = result;

                                    // Try to localize the map
                                    LibPlacenote.Instance.StartSession();
                                    
                                    notificationText.text = "Trying to Localize Map: " + m_SavedMapId;

                                    // Manage UI
                                    loadMapButton.SetActive(false);

                                    isLoaded = true;
                                }
                                else
                                {
                                    notificationText.text = "Failed to download meta data";
                                    return;
                                }
                            }
                        );
                    }
                    else if (faulted)
                    {
                        notificationText.text = "Failed to load Map ID: " + m_SavedMapId;
                    }
                    else
                    {
                        notificationText.text = "Download Progress: " + percentage.ToString("F2") + "/1.0)";
                    }
                }
            );
        }

        /// <summary>
        /// Handles loading and storing of the target endpoint nodes names from the node list into memory for re-navigation
        /// </summary>
        public void LoadTargetNames(List<MDSNode> targetNodes)
        {
            List<string> targetNames = new List<string>();

            foreach (MDSNode targetNode in targetNodes)
            {
                if ((MDSNodeType) targetNode.NodeInfo.nodeType == MDSNodeType.ENDPOINT)
                {
                    targetNames.Add(targetNode.NodeInfo.name);
                }
            }

            locationDropdown.AddOptions(targetNames);
        }

        /// <summary>
        /// Handles writing the map id into a file so it can be retrieved later
        /// </summary>
        private void WriteMapIDToFile(string mapId)
        {
            string path = Application.persistentDataPath + "/mapId.txt";
            Debug.Log(path);

            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(mapId);
            writer.Close();
        }

        /// <summary>
        /// Returns a string with the map Id that was saved into the file
        /// </summary>
        private string ReadMapIDFromFile()
        {
            string path = Application.persistentDataPath + "/mapId.txt";
            Debug.Log(path);

            if (System.IO.File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                string returnValue = reader.ReadLine();

                Debug.Log(returnValue);
                reader.Close();

                return returnValue;
            }
            else
            {
                return null;
            }
        }

        private LibPlacenote.MapMetadataSettable CreateMetaDataObject()
        {
            LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();

            metadata.name = "MDS AR tour map";

            // Get GPS location of device to save with map
            bool usingLocation = (Input.location.status == LocationServiceStatus.Running);
            LocationInfo locationInfo = Input.location.lastData;

            if (usingLocation)
            {
                metadata.location = new LibPlacenote.MapLocation();
                metadata.location.latitude = locationInfo.latitude;
                metadata.location.longitude = locationInfo.longitude;
                metadata.location.altitude = locationInfo.altitude;
            }

            JObject userdata = new JObject();
            JObject nodeList = MainController.Instance.GetNodeController().NodesToJSON();
            
            userdata["nodeList"] = nodeList;

            metadata.userdata = userdata;
            
            return metadata;
        }
    }
}
