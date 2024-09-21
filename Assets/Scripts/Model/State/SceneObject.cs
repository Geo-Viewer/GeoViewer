using System;
using GeoViewer.Model.Globe;
using UnityEngine;

namespace GeoViewer.Model.State
{
    public class SceneObject : MonoBehaviour
    {
        public string DisplayName { get; set; } = "Object";
        public bool IsSelected { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool IsUserMovable { get; set; } = true;

        public AttachementMode AttachementMode { get; set; } = AttachementMode.Unattached;
        public GlobePoint? GlobePoint { get; set; }
        public float Height { get; set; } = 0f;

        public event Action<SceneObject>? OnDestroy;

        public void Destroy(float delay = 0f)
        {
            OnDestroy?.Invoke(this);
            Destroy(this, delay);
        }
    }

    public enum AttachementMode
    {
        Unattached,
        RelativeToSurface,
        Absolute
    }
}