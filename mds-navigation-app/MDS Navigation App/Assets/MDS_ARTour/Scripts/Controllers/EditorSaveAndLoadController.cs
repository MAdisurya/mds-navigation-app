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
    public class EditorSaveAndLoadController : SaveAndLoadController
    {
        // UI GameObject references
        public GameObject newMapButton;
        public GameObject saveMapButton;

        public NodePlacer _nodePlacer;

        void Awake()
        {
            // Handle Assertions if references are null
            Assert.IsNotNull(newMapButton);
            Assert.IsNotNull(saveMapButton);
            Assert.IsNotNull(loadMapButton);
            Assert.IsNotNull(notificationText);
            Assert.IsNotNull(locationDropdown);
            Assert.IsNotNull(mapListView);

            Assert.IsNotNull(_nodePlacer);
        }

        void Start()
        {
            ActivateSaveButton(false);
            ActivateLocationDropdown(false);
            ActivateMapList(false);

            locationDropdown.ClearOptions();
        }
        
        /// <summary>
        /// Handles new map clicked event
        /// </summary>
        public override void OnNewMapClick()
        {
            notificationText.text = "Scan and click Save Map when complete!";

            ActivateNewButton(false);
            ActivateLoadButton(false);
            ActivateSaveButton(true);
            ActivateLocationDropdown(false);

            // Start the LibPlacenote session
            LibPlacenote.Instance.StartSession();
        }

        /// <summary>
        /// Handles loading of the map list
        /// </summary>
        public override void LoadMapList()
        {
            base.LoadMapList();
        }

        /// <summary>
        /// Handles saving of the Placenote Map, and uploading it to Placenote Cloud
        /// </summary>
        public override void SaveMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            ActivateNewButton(true);
            ActivateLoadButton(true);
            ActivateSaveButton(false);
            ActivateLocationDropdown(false);

            LibPlacenote.Instance.SaveMap(
                (mapId) =>
                {
                    if (isLoaded)
                    {
                        DeleteMap(m_SavedMapId);
                    }

                    m_SavedMapId = mapId;

                    WriteMapIDToFile(mapId);

                    LibPlacenote.Instance.StopSession();
                    FeaturesVisualizer.clearPointcloud();
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
        public override void LoadMap()
        {
            if (!LibPlacenote.Instance.Initialized())
            {
                notificationText.text = "SDK has not initialized yet!";
                return;
            }

            // Read saved map Id from file
            // m_SavedMapId = ReadMapIDFromFile();
            m_SavedMapId = mapListView.CurrentMapId;

            if (m_SavedMapId == null)
            {
                notificationText.text = "You haven't saved a map yet!";
                return;
            }

            notificationText.text = "Loading map...";

            // Disable node placement
            _nodePlacer.CanPlaceNodes = false;

            // Manage UI
            ActivateNewButton(false);
            ActivateSaveButton(false);
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
                                    LibPlacenote.Instance.StartSession();
                                    
                                    notificationText.text = "Trying to Localize Map: " + m_SavedMapId;

                                    // Manage UI
                                    ActivateLoadButton(false);
                                    locationDropdown.ClearOptions();

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
        /// Helper method that handles deleting of maps from the cloud
        /// </summary>
        public override void DeleteMap(string mapID)
        {
            base.DeleteMap(mapID);
        }

        /// <summary>
        /// Handles loading and storing of the target endpoint nodes names from the node list into memory for re-navigation
        /// </summary>
        public override void LoadTargetNames(List<MDSNode> targetNodes)
        {
            base.LoadTargetNames(targetNodes);
        }

        public override void SetCanPlaceNodes(bool set)
        {
            _nodePlacer.CanPlaceNodes = set;
        }

        public override void ActivateSaveButton(bool set)
        {
            saveMapButton.SetActive(set);
        }

        public override void ActivateNewButton(bool set)
        {
            newMapButton.SetActive(set);
        }
    }
}
