using GeoViewer.Model.State;
using NUnit.Framework;

namespace GeoViewer.Test.Editor.Model.State
{
    public class ApplicationStateTest
    {
        [Test]
        public void TestRotationCenterVisibility()
        {
            var eventRaised = false;
            ApplicationState.Instance.RotationCenterVisibilityChangedEvent += (_, _) => eventRaised = true;
            ApplicationState.Instance.SwitchRotationCenterVisibility();
            Assert.True(eventRaised);
        }
    }
}