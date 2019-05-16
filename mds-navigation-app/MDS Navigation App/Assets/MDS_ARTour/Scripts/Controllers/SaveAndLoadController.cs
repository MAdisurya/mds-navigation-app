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
        public GameObject loadMapButton;
        public Text notificationText;
        public Dropdown locationDropdown;
        public ListView mapListView;

        // Holds the last saved map ID
        protected string m_SavedMapId;

        protected List<string> m_MapListNames = new List<string>();

        protected bool isLoaded = false;

        protected LibPlacenote.MapMetadata m_DownloadedMetadata;

        // Getters
        public LibPlacenote.MapMetadata DownloadedMetadata
        {
            get { return m_DownloadedMetadata; }
        }

        void Awake()
        {
            // Handle Assertions if references are null
            Assert.IsNotNull(loadMapButton);
            Assert.IsNotNull(notificationText);
            Assert.IsNotNull(locationDropdown);
            Assert.IsNotNull(mapListView);
        }

        void Start()
        {
            ActivateLocationDropdown(false);
            ActivateMapList(false);

            locationDropdown.ClearOptions();
        }
        
        /// <summary>
        /// Handles new map clicked event
        /// </summary>
        public virtual void OnNewMapClick()
        {
            notificationText.text = "Scan and click Save Map when complete!";

            ActivateLoadButton(false);
            ActivateLocationDropdown(false);

            // Start the LibPlacenote session
            LibPlacenote.Instance.StartSession();
        }

        /// <summary>
        /// Handles loading of the map list
        /// </summary>
        public virtual void LoadMapList()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            m_MapListNames.Clear();

            mapListView.ClearItems();
            ActivateMapList(true);

            LibPlacenote.Instance.ListMaps(
                (mapList) =>
                {
                    foreach (LibPlacenote.MapInfo mapInfoItem in mapList)
                    {
                        m_MapListNames.Add(mapInfoItem.placeId);

                        mapListView.AddItem(mapInfoItem.placeId);
                    }
                }
            );
        }

        /// <summary>
        /// Handles saving of the Placenote Map, and uploading it to Placenote Cloud
        /// </summary>
        public virtual void SaveMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            ActivateLoadButton(true);
            ActivateLocationDropdown(false);

            LibPlacenote.Instance.SaveMap(
                (mapId) =>
                {
                    if (isLoaded)
                    {
                        DeleteMap(m_SavedMapId);
                        isLoaded = false;
                    }

                    m_SavedMapId = mapId;

                    WriteMapIDToFile(mapId);

                    LibPlacenote.Instance.StopSession();
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
        public virtual void LoadMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            // Read saved map Id from file
            m_SavedMapId = ReadMapIDFromFile();
            
            if (mapListView.CurrentMapId != m_SavedMapId)
            {
                m_SavedMapId = mapListView.CurrentMapId;
            }

            // if (m_SavedMapId == null)
            // {
            //     notificationText.text = "You haven't saved a map yet!";
            //     return;
            // }

            notificationText.text = "Loading map...";

            // Manage UI
            ActivateMapList(false);

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
                                    if (MainController.Instance._mode == Mode.EDITOR_MODE)
                                    {
                                        LibPlacenote.Instance.StartSession(true);
                                    }
                                    else
                                    {
                                        LibPlacenote.Instance.StartSession();
                                    }
                                    
                                    notificationText.text = "Trying to Localize Map: " + m_SavedMapId;

                                    // Manage UI
                                    ActivateLoadButton(false);
                                    locationDropdown.ClearOptions();
                                    
                                    // Enable the scan panel UI
                                    MainController.Instance.GetGUIController().scanPanel.EnablePanel();

                                    isLoaded = true;

                                    FeaturesVisualizer.clearPointcloud();
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
        /// LoadMap() overload handles loading a map using string mapID
        /// </summary>
        public void LoadMap(string mapId)
        {
            m_SavedMapId = mapId;

            LoadMap();
        }

        /// <summary>
        /// LoadMap() overload handles loading a map using int id index
        /// </summary>
        public void LoadMap(int mapIndex)
        {
            m_SavedMapId = m_MapListNames[mapIndex];

            LoadMap();
        }

        /// <summary>
        /// Helper method that handles deleting of maps from the cloud
        /// </summary>
        public virtual void DeleteMap(string mapID)
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            LibPlacenote.Instance.DeleteMap(
                mapID, 
                (deleted, errMsg) =>
                {
                    if (deleted)
                    {
                        notificationText.text = "Map: " + mapID + " successfully deleted.";
                    }
                    else
                    {
                        notificationText.text = "Failed to delete map: " + errMsg;
                    }
                });
        }

        /// <summary>
        /// Handles loading and storing of the target endpoint nodes names from the node list into memory for re-navigation
        /// </summary>
        public virtual void LoadTargetNames(List<MDSNode> targetNodes)
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
        /// Helper method that sets NodePlacer.CanPlaceNodes
        /// </summary>
        public virtual void SetCanPlaceNodes(bool set) {}

        public virtual void ActivateSaveButton(bool set) {}

        public virtual void ActivateLoadButton(bool set) 
        {
            loadMapButton.SetActive(set);
        }

        public virtual void ActivateNewButton(bool set) {}

        public virtual void ActivateLocationDropdown(bool set) 
        {
            locationDropdown.gameObject.SetActive(set);
        }

        public virtual void ActivateMapList(bool set)
        {
            mapListView.gameObject.SetActive(set);
        }

        /// <summary>
        /// Handles writing the map id into a file so it can be retrieved later
        /// </summary>
        protected void WriteMapIDToFile(string mapId)
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
        protected string ReadMapIDFromFile()
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

        protected LibPlacenote.MapMetadataSettable CreateMetaDataObject()
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
