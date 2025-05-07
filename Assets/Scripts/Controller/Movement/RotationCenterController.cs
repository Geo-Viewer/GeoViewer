using GeoViewer.Model.State;
using GeoViewer.Model.State.Events;
using UnityEngine;

namespace GeoViewer.Controller.Movement
{
    /// <summary>
    /// Component to control the scale of the rotation center.
    /// </summary>
    public class RotationCenterController : MonoBehaviour
    {
        [SerializeField] private Transform? visuals;

        private void Awake()
        {
            ApplicationState.Instance.RotationCenter = gameObject;
        }

        private void OnEnable()
        {
            ApplicationState.Instance.RotationCenterVisibilityChangedEvent += ChangeVisibility;
        }

        private void Update()
        {
            visuals!.eulerAngles = /*new Vector3(-transform.eulerAngles.x, -transform.eulerAngles.y, -transform.eulerAngles.z);*/
                Vector3.zero;
        }

        private void OnDisable()
        {
            ApplicationState.Instance.RotationCenterVisibilityChangedEvent -= ChangeVisibility;
        }

        private void ChangeVisibility(object sender, RotationCenterVisibilityChangedEventArgs args)
        {
            var rotationCenter = ApplicationState.Instance.RotationCenter;
            if (rotationCenter != null)
            {
                rotationCenter.transform.GetChild(0).gameObject.SetActive(args.RotationCenterVisible);
            }
        }
    }
}