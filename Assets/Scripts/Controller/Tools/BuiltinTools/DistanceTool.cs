using System.Globalization;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Util;
using GeoViewer.Model.State;
using GeoViewer.Model.Tools;
using GeoViewer.Model.Tools.Mode;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace GeoViewer.Controller.Tools.BuiltinTools
{
    /// <summary>
    /// A tool which measures distances in the world by clicking on two points.
    /// </summary>
    public class DistanceTool : Tool
    {
        private static readonly string[] MeasureLayers = { "Selectable", "Selected", "Terrain" };
        private readonly LayerMask _measuringLayers;

        private readonly Transform?[] _positions = new Transform[2];
        private bool _first = true;

        // Rendering

        private const int UILayer = 5;

        private readonly int _textLayer = LayerMask.NameToLayer("Text");

        private bool _initialized;

        // Initialized in OnAwake()
        private LineRenderer _lineRenderer = null!;
        private const float LineWidth = 0.1f;
        private const float ScalingFactor = 0.01f;

        // Initialized in OnAwake()
        private Material _pointMaterial = null!;

        private const float TextRaise = 0.2f;

        // Initialized in OnAwake()
        private TextMeshPro _distanceText = null!;
        private float2 _position;

        /// <summary>
        /// Create a new tool for measuring distances between two points.
        /// </summary>
        /// <param name="inputs">An <see cref="Inputs"/> instance which is used to retrieve user input.</param>
        public DistanceTool(Inputs inputs) : base(inputs)
        {
            _measuringLayers = LayerMask.GetMask(MeasureLayers);
        }

        /// <inheritdoc/>
        public override ToolMode Mode { get; } = new ToolMode.Builder()
            .WithFeature(ApplicationFeature.ClickPrimary)
            .Build();

        /// <inheritdoc/>
        public override ToolData Data { get; } = new(
            Resources.Load<VectorImage>("Tools/Ruler"),
            "Measure Distances",
            "Measure distances between two points by clicking on them",
            Color.white,
            1000
        );

        /// <inheritdoc/>
        protected override void OnActivate()
        {
            InitializeDistanceTool();
            Inputs.PrimaryClicked += TrySelect;

            // after the tool was activated, the user always starts a new measurement
            _first = true;
        }

        /// <inheritdoc/>
        protected override void OnDisable()
        {
            Inputs.PrimaryClicked -= TrySelect;
            ClearRendering();
        }

        /// <inheritdoc/>
        public override void OnUpdate()
        {
            if (Camera == null)
            {
                return;
            }

            // update the scale of the position markers and the distance text
            var distance = Vector3.Distance(Camera.transform.position, _distanceText.transform.position) *
                           ScalingFactor;
            for (var index = 0; index < _positions.Length; index++)
            {
                var pos = _positions[index];
                if (pos is not null)
                {
                    pos.localScale = distance * Vector3.one /
                                     (float)ApplicationState.Instance.MapRenderer.CurrentWorldScale;
                    _lineRenderer.SetPosition(index, pos.position);
                }
            }

            _distanceText.transform.localScale = distance * Vector3.one /
                                                 (float)ApplicationState.Instance.MapRenderer.CurrentWorldScale;
            _lineRenderer.widthMultiplier = distance * 0.5f;

            // make the distance text face the camera
            _distanceText.transform.forward = Camera.transform.forward;
        }

        private void TrySelect()
        {
            // If the click was aborted or didn't finish, we don't select anything.
            if (EventSystem.current.IsPointerOverGameObject()
                || !Inputs.MousePosition.HasValue
                || Camera is null)
            {
                return;
            }

            if (Physics.Raycast(
                    Camera.ScreenPointToRay(Inputs.MousePosition.Value),
                    out var hit,
                    float.PositiveInfinity,
                    _measuringLayers))
            {
                if (_first)
                {
                    ClearRendering();
                    _positions[0] = CreatePosition(hit.point);
                }
                else
                {
                    _positions[1] = CreatePosition(hit.point);

                    UpdateDistanceText();
                }

                var position = _positions[_first ? 0 : 1];
                if (position != null)
                {
                    _lineRenderer.SetPosition(_lineRenderer.positionCount++, position.position);
                }

                // toggle the current array
                _first = !_first;
            }
        }

        private void UpdateDistanceText()
        {
            var firstPosition = _positions[0]?.position;
            var secondPosition = _positions[1]?.position;
            if (firstPosition == null || secondPosition == null)
            {
                return;
            }


            _distanceText.text = Vector3.Distance(
                    EcefConverter
                        .GlobePointToEcef(
                            ApplicationState.Instance.MapRenderer.ApplicationPositionToGlobePoint(firstPosition.Value))
                        .ToVector3(),
                    EcefConverter
                        .GlobePointToEcef(
                            ApplicationState.Instance.MapRenderer.ApplicationPositionToGlobePoint(secondPosition.Value))
                        .ToVector3())
                .ToString("#.##m", CultureInfo.InvariantCulture);

            _distanceText.transform.position =
                firstPosition.Value + 0.5f * (secondPosition.Value - firstPosition.Value) + TextRaise * Vector3.up;
            // trigger resize of the canvas
            // unfortunately the property only triggers a rebuild once and only if it is modified.
            // We need to unset it and set it again to trigger a resize.
            _distanceText.autoSizeTextContainer = false;
            _distanceText.autoSizeTextContainer = true;
        }

        /// <summary>
        /// renders a measuring point
        /// </summary>
        /// <param name="point">The position at which the point should be rendered.</param>
        /// <returns>The transform of the <see cref="GameObject"/> rendering the point</returns>
        private Transform CreatePosition(Vector3 point)
        {
            // create a small sphere to visualize the clicked point
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            obj.transform.position = point;
            obj.transform.parent = _lineRenderer.gameObject.transform;
            obj.layer = UILayer;
            obj.GetComponent<MeshRenderer>().material = _pointMaterial;

            return obj.transform;
        }

        /// <summary>
        /// Cleans up anything which is rendered to display measurements
        /// </summary>
        private void ClearRendering()
        {
            // delete measuring points
            _lineRenderer.positionCount = 0;
            for (var i = 0; i < _positions.Length; i++)
            {
                var transform = _positions[i];
                if (transform is not null)
                {
                    Object.Destroy(transform.gameObject);
                    _positions[i] = null;
                }
            }

            // clear text
            _distanceText.text = "";
        }

        /// <summary>
        /// When the tool is activated for the first time, we need to
        /// create the line renderer and the distance text GameObject
        /// </summary>
        private void InitializeDistanceTool()
        {
            if (_initialized)
            {
                return;
            }

            // create line renderer
            _lineRenderer = new GameObject("DistanceLineRenderer", typeof(LineRenderer)).GetComponent<LineRenderer>();
            var transform = _lineRenderer.transform;
            transform.localScale = Vector3.one * (float)ApplicationState.Instance.MapRenderer.CurrentWorldScale;
            ApplicationState.Instance.MapRenderer.AttachToMap(transform);
            _lineRenderer.positionCount = 0;
            _lineRenderer.gameObject.layer = UILayer;
            _lineRenderer.widthMultiplier = LineWidth;

            _lineRenderer.textureMode = LineTextureMode.Tile;
            _lineRenderer.material = (Material)Resources.Load("LineMaterial", typeof(Material));

            _lineRenderer.numCornerVertices = 3;

            // create text displaying distance
            _distanceText = new GameObject("DistanceText", typeof(TextMeshPro)).GetComponent<TextMeshPro>();
            _distanceText.transform.SetParent(_lineRenderer.gameObject.transform);
            _distanceText.gameObject.layer = _textLayer;

            // set style
            _distanceText.renderer.material = (Material)Resources.Load("TextMaterial", typeof(Material));
            _distanceText.fontSize = 30;

            // load point material
            _pointMaterial = (Material)Resources.Load("PointMaterial", typeof(Material));

            _initialized = true;
        }
    }
}