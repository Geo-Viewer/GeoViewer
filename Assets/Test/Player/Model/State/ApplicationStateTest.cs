using System.Collections;
using System.Linq;
using GeoViewer.Model.State;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GeoViewer.Test.Player.Model.State
{
    public class ApplicationStateTest
    {
        [SetUp]
        public void Setup()
        {
            // Load the main scene
            SceneManager.LoadScene(0);
        }

        [UnityTest]
        public IEnumerator AddSelected()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sceneobj = cube.AddComponent<SceneObject>();

            Assert.False(ApplicationState.Instance.SelectedObjects.Contains(sceneobj));

            sceneobj.IsSelected = true;
            Assert.True(ApplicationState.Instance.SelectedObjects.Contains(sceneobj));

            yield break;
        }

        [UnityTest]
        public IEnumerator RemoveSelected()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var sceneobj = cube.AddComponent<SceneObject>();
            
            sceneobj.IsSelected = true;
            Assert.True(ApplicationState.Instance.SelectedObjects.Contains(sceneobj));
            sceneobj.IsSelected = false;
            Assert.False(ApplicationState.Instance.SelectedObjects.Contains(sceneobj));

            yield break;
        }
    }
}