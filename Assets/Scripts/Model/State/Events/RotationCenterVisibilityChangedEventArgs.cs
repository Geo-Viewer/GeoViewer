using System;

namespace GeoViewer.Model.State.Events
{
    /// <summary>
    /// Arguments for the rotation center visibility changed event.
    /// The event is raised when the visibility of the rotation center is changed.
    /// </summary>
    public class RotationCenterVisibilityChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The property is true if the rotation center is visible and false otherwise.
        /// </summary>
        public bool RotationCenterVisible { get; }

        /// <summary>
        /// A standard constructor which sets the value of RotationCenterVisible.
        /// </summary>
        /// <param name="rotationCenterVisible">the value on which the property should be set</param>
        public RotationCenterVisibilityChangedEventArgs(bool rotationCenterVisible)
        {
            RotationCenterVisible = rotationCenterVisible;
        }
    }
}