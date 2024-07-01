using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
    public class MovementToolTest : InputTestFixture
    {
        private Mouse _mouse;
        private Keyboard _keyboard;

        public override void Setup()
        {
            base.Setup();

            SceneManager.LoadScene(0);

            _mouse = InputSystem.AddDevice<Mouse>();
            _keyboard = InputSystem.AddDevice<Keyboard>();
        }

#if !CI_TEST
        [UnityTest]
#endif
        public IEnumerator MoveObjectTest()
        {
            // close welcome screen
            GameObject.Find("Welcomescreen").GetComponent<WelcomeScreen>().Close();

            // add object
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.layer = LayerMask.NameToLayer("Selected");
            ApplicationState.Instance.AddSelectedObject(obj);

            // register tool
            var id = ToolManager.Instance.Registry.RegisterTool(new MovementTool(new Inputs(new InputManager())));
            ToolManager.Instance.Registry.TrySetActiveTool(id);

            // movement and testing

            yield return new WaitForEndOfFrame();
            // assert initial position
            var transform = obj.transform;
            Assert.True(Vector3.zero.Equals(transform.position));

            Press(_mouse.leftButton);
            yield return new WaitForEndOfFrame();
            Move(_mouse.position, new Vector2(100, 0));
            yield return new WaitForEndOfFrame();
            Release(_mouse.leftButton);
            yield return new WaitForEndOfFrame();

            Assert.False(Vector3.zero.Equals(transform.position));
        }

#if !CI_TEST
        [UnityTest]
#endif
        [SuppressMessage("ReSharper", "Unity.InefficientPropertyAccess")]
        public IEnumerator MoveVerticalTest()
        {
            // close welcome screen
            GameObject.Find("Welcomescreen").GetComponent<WelcomeScreen>().Close();

            // add object
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.layer = LayerMask.NameToLayer("Selected");
            ApplicationState.Instance.AddSelectedObject(obj);

            // register tool
            var id = ToolManager.Instance.Registry.RegisterTool(new MovementTool(new Inputs(new InputManager())));
            ToolManager.Instance.Registry.TrySetActiveTool(id);

            // movement and testing

            yield return new WaitForEndOfFrame();
            // assert initial position
            var transform = obj.transform;
            Assert.True(Vector3.zero.Equals(transform.position));

            Press(_mouse.leftButton);
            Press(_keyboard.altKey);

            yield return new WaitForEndOfFrame();
            Move(_mouse.position, new Vector2(42, 42));
            yield return new WaitForEndOfFrame();
            Release(_mouse.leftButton);
            Release(_keyboard.altKey);
            yield return new WaitForEndOfFrame();

            // only z should change
            Assert.True(transform.position.x == 0);
            Assert.True(transform.position.y > 0);
            Assert.True(transform.position.z == 0);
        }
    }
}