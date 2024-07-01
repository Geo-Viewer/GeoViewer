using System.Collections;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Tools;
using GeoViewer.Controller.Tools.BuiltinTools;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GeoViewer.Test.Player.Controller.Tools
{
    public class ToolManagerTest
    {
        [SetUp]
        public void Setup()
        {
            // Load the main scene
            SceneManager.LoadScene(0);
        }

        [UnityTest]
        public IEnumerator CorrectToolOnStart()
        {
            var manager = ToolManager.Instance;
            Assert.True(manager.DefaultTool.Equals(manager.Registry.ActiveTool));

            yield break;
        }

        [UnityTest]
        public IEnumerator ResetTool()
        {
            var manager = ToolManager.Instance;
            var id = manager.Registry.RegisterTool(new ScaleTool(new Inputs(new InputManager())));

            // activate new tool
            Assert.True(manager.Registry.TrySetActiveTool(id));
            Assert.True(id.Equals(manager.Registry.ActiveTool));
            Assert.False(manager.DefaultTool.Equals(manager.Registry.ActiveTool));

            // reset tool
            manager.ResetActiveTool();

            Assert.True(manager.DefaultTool.Equals(manager.Registry.ActiveTool));

            yield break;
        }
    }
}