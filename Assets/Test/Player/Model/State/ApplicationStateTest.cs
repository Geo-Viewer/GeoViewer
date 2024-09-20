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

            Assert.False(ApplicationState.Instance.SelectedObjects.Contains(cube));

            ApplicationState.Instance.AddSelectedObject(cube);
            Assert.True(ApplicationState.Instance.SelectedObjects.Contains(cube));

            yield break;
        }

        [UnityTest]
        public IEnumerator RemoveSelected()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            ApplicationState.Instance.AddSelectedObject(cube);
            Assert.True(ApplicationState.Instance.SelectedObjects.Contains(cube));
            ApplicationState.Instance.RemoveSelectedObject(cube);
            Assert.False(ApplicationState.Instance.SelectedObjects.Contains(cube));

            yield break;
        }
    }
}