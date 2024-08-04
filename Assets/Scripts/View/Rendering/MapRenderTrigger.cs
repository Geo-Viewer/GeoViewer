using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.View.Rendering
{
    public class MapRenderTrigger : MonoBehaviour
    {
        private MapRenderer mapRenderer;
        [SerializeField] private float updateDistance;
        [SerializeField] private float updateTimer;
        private float _timer;

        private Vector3 _lastPosition;

        private void Start()
        {
            mapRenderer = ApplicationState.Instance.MapRenderer;
            _lastPosition = transform.position;
            _timer = 0;

            mapRenderer.UpdateMap();
        }

        private void LateUpdate()
        {
            _timer += Time.deltaTime;
            if (_timer < updateTimer)
            {
                return;
            }

            _timer -= updateTimer;
            if (_lastPosition == transform.position) return;

            _lastPosition = transform.position;
            mapRenderer.UpdateMap();
        }
    }
}