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
    public class RotationToolTest : InputTestFixture
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
        public IEnumerator RotateObjectTest()
        {
            // close welcome screen
            GameObject.Find("Welcomescreen").GetComponent<WelcomeScreen>().Close();

            // add object
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.layer = LayerMask.NameToLayer("Selected");
            ApplicationState.Instance.AddSelectedObject(obj);

            // register tool
            var id = ToolManager.Instance.Registry.RegisterTool(new RotationTool(new Inputs(new InputManager())));
            ToolManager.Instance.Registry.TrySetActiveTool(id);

            // rotation and testing

            yield return new WaitForEndOfFrame();
            // assert initial position
            var transform = obj.transform;
            Assert.True(Vector3.zero.Equals(transform.rotation.eulerAngles));

            Press(_mouse.leftButton);
            yield return new WaitForEndOfFrame();
            Move(_mouse.position, new Vector2(100, 0));
            yield return new WaitForEndOfFrame();
            Release(_mouse.leftButton);
            yield return new WaitForEndOfFrame();

            Assert.True(transform.rotation.eulerAngles.x == 0);
            Assert.True(transform.rotation.eulerAngles.y > 0);
            Assert.True(transform.rotation.eulerAngles.z == 0);
        }
    }
}