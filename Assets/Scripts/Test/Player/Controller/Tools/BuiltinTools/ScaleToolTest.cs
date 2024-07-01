using System.Collections;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Tools;
using GeoViewer.Controller.Tools.BuiltinTools;
using GeoViewer.Model.State;
using GeoViewer.View.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GeoViewer.Test.Player.Controller.Tools.BuiltinTools
{
    public class ScaleToolTest : InputTestFixture
    {
        private Mouse _mouse;

        public override void Setup()
        {
            base.Setup();

            SceneManager.LoadScene(0);

            _mouse = InputSystem.AddDevice<Mouse>();
        }

#if !CI_TEST
        [UnityTest]
#endif
        public IEnumerator UpscaleTest()
        {
            yield return new EnterPlayMode();
            // close welcome screen
            GameObject.Find("Welcomescreen").GetComponent<WelcomeScreen>().Close();

            // add object
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.layer = LayerMask.NameToLayer("Selected");
            ApplicationState.Instance.AddSelectedObject(obj);

            // register tool
            var id = ToolManager.Instance.Registry.RegisterTool(new ScaleTool(new Inputs(new InputManager())));
            ToolManager.Instance.Registry.TrySetActiveTool(id);

            // movement and testing

            yield return new WaitForEndOfFrame();
            // assert initial scale
            Assert.True(Vector3.one.Equals(obj.transform.localScale));

            Move(_mouse.position, Camera.main.WorldToScreenPoint(obj.transform.position));
            Press(_mouse.leftButton);
            yield return new WaitForEndOfFrame();
            Move(_mouse.position, new Vector2(100, 100));
            yield return new WaitForEndOfFrame();
            Release(_mouse.leftButton);
            yield return new WaitForEndOfFrame();

            Assert.True(obj.transform.localScale.x > 1, "obj.transform.localScale.x > 1");
            Assert.True(obj.transform.localScale.y > 1, "obj.transform.localScale.y > 1");
            Assert.True(obj.transform.localScale.z > 1, "obj.transform.localScale.z > 1");
        }

#if !CI_TEST
        [UnityTest]
#endif
        public IEnumerator DownscaleTest()
        {
            yield return new EnterPlayMode();
            yield return new WaitForEndOfFrame();
            // close welcome screen
            GameObject.Find("Welcomescreen").GetComponent<WelcomeScreen>().Close();

            // add object
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.layer = LayerMask.NameToLayer("Selected");
            ApplicationState.Instance.AddSelectedObject(obj);

            // register tool
            var id = ToolManager.Instance.Registry.RegisterTool(new ScaleTool(new Inputs(new InputManager())));
            ToolManager.Instance.Registry.TrySetActiveTool(id);

            // movement and testing

            yield return new WaitForEndOfFrame();
            // assert initial position
            Assert.True(Vector3.zero.Equals(obj.transform.position));

            Press(_mouse.leftButton);
            yield return new WaitForEndOfFrame();
            Move(_mouse.position, ApplicationState.Instance.Camera!.WorldToScreenPoint(obj.transform.position));
            yield return new WaitForEndOfFrame();
            Release(_mouse.leftButton);
            yield return new WaitForEndOfFrame();

            Assert.True(obj.transform.localScale.x < 1);
            Assert.True(obj.transform.localScale.y < 1);
            Assert.True(obj.transform.localScale.z < 1);
        }
    }
}