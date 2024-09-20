using System.Collections.Generic;
using GeoViewer.Model.Tools.Mode;
using NUnit.Framework;

namespace GeoViewer.Test.Editor.Model.Tools.Mode
{
    public class ToolModeTest
    {
        [Test]
        public void TestUsable()
        {
            var mode = new ToolMode();
            Assert.True(mode.CanAppUse(ApplicationFeature.ClickPrimary));
            Assert.True(mode.CanAppUse(ApplicationFeature.HoldAlt));
            Assert.True(mode.CanAppUse(ApplicationFeature.HoldSecondary));
        }

        [Test]
        public void TestNotUsable()
        {
            var mode = new ToolMode(new List<ApplicationFeature> { ApplicationFeature.ClickPrimary });
            Assert.False(mode.CanAppUse(ApplicationFeature.ClickPrimary));
        }
    }
}