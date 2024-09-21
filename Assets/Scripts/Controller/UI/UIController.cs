using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Movement;
using GeoViewer.Controller.ObjLoading;
using GeoViewer.Controller.Tools;
using GeoViewer.Controller.Util;
using GeoViewer.Model.State;
using GeoViewer.View.UI;
using GeoViewer.View.UI.Dialogue;
using GeoViewer.View.UI.InformationBox;
using GeoViewer.View.UI.Menubar;
using UnityEngine;

namespace GeoViewer.Controller.UI
{
    /// <summary>
    /// The class is the connection between the view and the controller.
    /// It creates the menus in the menubar and manages the elements of the UI.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        private const float CoordinatesUpdateTime = 0.3f;
        private const string TerrainLayerName = "Terrain";
        private const string SelectableLayerName = "Selectable";
        private const string SelectedLayerName = "Selected";

        private int _terrainLayer;
        private int _selectableLayer;
        private int _selectedLayer;

#pragma warning disable RCS1169
        // Values are set from editor
        [SerializeField] private WelcomeScreen welcomeScreen = null!;
        [SerializeField] private LoadingScreen loadingScreen = null!;
#pragma warning restore RCS1169

        private readonly List<int> _layers = new();

        private float _coordinatesUpdateTimer;

        // Value is set in Start()
        private CoordinateDisplay _coordinateDisplay = null!;
        private InformationBox _informationBox = null!;

        private void Awake()
        {
            _terrainLayer = LayerMask.NameToLayer(TerrainLayerName);
            _selectableLayer = LayerMask.NameToLayer(SelectableLayerName);
            _selectedLayer = LayerMask.NameToLayer(SelectedLayerName);
            _layers.Add(_terrainLayer);
            _layers.Add(_selectableLayer);
            _layers.Add(_selectedLayer);
            CreateMenus();
        }

        private void Start()
        {
            SetupInformationBox();
            welcomeScreen.Open();
        }

        private void Update()
        {
            //calls the method to change the coordinates only if the CoordinatesUpdateTime has passed
            //because the method which is called is expensive
            if (_coordinatesUpdateTimer >= CoordinatesUpdateTime)
            {
                UpdateInformationBoxCoordinates();
                _coordinatesUpdateTimer -= CoordinatesUpdateTime;
            }

            _coordinatesUpdateTimer += Time.deltaTime;
        }

        /// <summary>
        /// A method which is a toggle deciding whether the rotation center is visible or invisible.
        /// </summary>
        public void ToggleRotationCenter()
        {
            ApplicationState.Instance.SwitchRotationCenterVisibility();
        }

        /// <summary>
        /// A method which is a toggle deciding whether the height data is visible or invisible.
        /// </summary>
        public void ToggleRenderHeightData()
        {
            var layerManager = ApplicationState.Instance.LayerManager;
            layerManager.SetLayersActive(typeof(IMeshLayer), !layerManager.GetLayersActive(typeof(IMeshLayer)));
        }

        private void CreateMenus()
        {
            var menubar = GetComponent<Menubar>();
            var about = new MenuEntry(welcomeScreen.Open, "About");
            var close = new MenuEntry(Application.Quit, "Close");
            var geoViewerEntries = new List<MenuEntry> { about, close };
            menubar.AddMenu("GeoViewer", geoViewerEntries, 5000);
            var importObject = new MenuEntry(ImportObject, "Import Object");
            var objectsEntries = new List<MenuEntry> { importObject };
            menubar.AddMenu("Objects", objectsEntries, 3000);
        }

        private void SetupInformationBox()
        {
            _informationBox = GetComponent<InformationBox>();
            _coordinateDisplay = new CoordinateDisplay();
            _informationBox.AddElement(_coordinateDisplay);
        }

        private void UpdateInformationBoxCoordinates()
        {
            RaycastHit hit;
            if (RaycastUtil.GetCursorRaycastHit(_layers, out hit))
            {
                DisplayRaycastHitCoordinates(hit);
            }
        }

        private void DisplayRaycastHitCoordinates(RaycastHit hit)
        {
            var globePoint = ApplicationState.Instance.MapRenderer.ApplicationPositionToGlobePoint(hit.point);
            _coordinateDisplay.SetCoordinates(globePoint);
        }

        /// <summary>
        /// Calls a method which sets the rotation center to the origin.
        /// </summary>
        public void ResetCamera(bool resetToObject = true)
        {
            if (ApplicationState.Instance.Camera == null) return;

            if (resetToObject && ObjLoader.LoadedObject != null 
                              && ObjLoader.LoadedObject.TryGetComponent(out SceneObject sceneObject))
            {
                ApplicationState.Instance.MapRenderer.MoveOrigin(sceneObject);
            }

            ApplicationState.Instance.Camera.GetComponent<CameraController>().SetPosition(Vector3.zero);
        }

        private async void ImportObject()
        {
            var result = await GetComponent<ObjectImportDialogue>().Open();
            if (result.Status != DialogueResponseStatus.Success)
            {
                return;
            }

            //Clear history and map
            ApplicationState.Instance.CommandHandler.Clear();
            ApplicationState.Instance.MapRenderer.ClearMap();

            if (result.Response?.GlobePoint != null)
            {
                ApplicationState.Instance.MapRenderer.Enabled = true;
                ApplicationState.Instance.MapRenderer.MoveOrigin(result.Response.GlobePoint);
            }
            else
            {
                ApplicationState.Instance.MapRenderer.Enabled = false;
            }

            ResetCamera(false);

            //Create and show map
            if (result.Response?.GlobePoint != null)
            {
                ApplicationState.Instance.MapRenderer.UpdateMap();
            }

            //show coordinate display
            _informationBox.SetVisible(result.Response?.GlobePoint != null);

            //Load Model at (0, 0, 0)
            try
            {
                loadingScreen.Open();
                await ObjLoader.LoadModel(result.Response?.ModelPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                ToolManager.Instance.ResetActiveTool();
                loadingScreen.Close();
            }
        }
    }
}