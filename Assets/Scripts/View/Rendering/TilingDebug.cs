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
            if (_mapRenderer.CurrentRequestArea != null)
            {
                Gizmos.color = Color.blue;
                DrawGlobeArea(_mapRenderer.CurrentRequestArea);
            }

            Gizmos.color = Color.red;
            foreach (var tileId in _mapRenderer.CalculateSegmentation(_mapRenderer.CurrentRequestArea))
            {
                DrawGlobeArea(_layerManager.CurrentSegmentationSettings.Projection.TileToGlobeArea(tileId));
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 50, 50), "UpdateMap"))
            {
                _mapRenderer.UpdateMap();
            }
        }

        private void DrawGlobeArea(GlobeArea area)
        {
            var ne = _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(area.NorthEastPoint,
                _mapRenderer.Origin.Altitude));
            var se = _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(area.SouthEastPoint,
                _mapRenderer.Origin.Altitude));
            var sw = _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(area.SouthWestPoint,
                _mapRenderer.Origin.Altitude));
            var nw = _mapRenderer.GlobePointToApplicationPosition(new GlobePoint(area.NorthWestPoint,
                _mapRenderer.Origin.Altitude));

            Gizmos.DrawLine(ne, se);
            Gizmos.DrawLine(se, sw);
            Gizmos.DrawLine(sw, nw);
            Gizmos.DrawLine(nw, ne);
        }
    }
}