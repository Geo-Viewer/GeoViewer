using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace GeoViewer.Test.Player.View.UI.InformationBox
{
    public class InformationBoxTest
    {
        [SetUp]
        public void Setup()
        {
            // Load the main scene
            SceneManager.LoadScene(0);
        }

        [UnityTest]
        public IEnumerator TestAddElement()
        {
            yield return new EnterPlayMode();

            var informationBox = GameObject.Find("UI").GetComponent<GeoViewer.View.UI.InformationBox.InformationBox>();
            var testVisualElement = new VisualElement();
            testVisualElement.name = "Test";
            var count = informationBox.AddElement(testVisualElement);
            Assert.True(informationBox.GetElement(count)?.name == testVisualElement.name);
        }

        [UnityTest]
        public IEnumerator TestGetElementIdNull()
        {
            yield return new EnterPlayMode();

            var informationBox = GameObject.Find("UI").GetComponent<GeoViewer.View.UI.InformationBox.InformationBox>();
            Assert.IsNull(informationBox.GetElement(10));
        }
    }
}