using System;
using System.Linq;
using GeoViewer.Controller.DataLayers;
using GeoViewer.Controller.Util;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.View.Rendering
{
    public class TilingDebug : MonoBehaviour
    {
        private readonly LayerManager _layerManager = ApplicationState.Instance.LayerManager;
        private readonly MapRenderer _mapRenderer = ApplicationState.Instance.MapRenderer;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            DrawGlobeMask(_mapRenderer.GetCameraGlobeFrustum());
            if (_mapRenderer.CurrentRequestArea != null)
            {
                Gizmos.color = Color.blue;
                DrawGlobeMask(_mapRenderer.CurrentRequestArea);
            }

            Gizmos.color = Color.red;
            foreach (var tileId in _mapRenderer.CalculateSegmentation(_mapRenderer.CurrentRequestArea))
            {
                DrawGlobeMask(_layerManager.CurrentSegmentationSettings.Projection.TileToGlobeArea(tileId));
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 50, 50), "UpdateMap"))
            {
                _mapRenderer.UpdateMap();
            }
        }

        private void DrawGlobeMask(IGlobeMask mask)
        {
            Vector3? previous = null;
            foreach (var point in mask.Points)
            {
                var current = _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(point,
                    _mapRenderer.Origin.Altitude));
                if (previous != null)
                {
                    Gizmos.DrawLine(previous.Value, current);
                }

                previous = current;
            }

            Gizmos.DrawLine(_mapRenderer.GlobePointToApplicationPosition(new GlobePoint(mask.Points[0],
                _mapRenderer.Origin.Altitude)), _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(
                mask.Points[^1],
                _mapRenderer.Origin.Altitude)));
        }
    }
}