using GeoViewer.Controller.Commands;
using NUnit.Framework;
using UnityEngine;

namespace GeoViewer.Test.Editor.Controller.Commands
{
    public class CommandHandlerTest
    {
        private CommandHandler _commandHandler = new();

        [Test]
        public void UndoRedoTransformTest()
        {
            GameObject obj = new();
            _commandHandler.Execute(new TransformSelected(new[]
                { (obj.transform, Vector3.one, Quaternion.Euler(10, 20, 30), Vector3.one) }));
            CheckValues(obj.transform, Vector3.one, new Vector3(10, 20, 30), Vector3.one + Vector3.one);
            _commandHandler.Undo();
            _commandHandler.Undo();
            CheckValues(obj.transform, Vector3.zero, Vector3.zero, Vector3.one);
            _commandHandler.Redo();
            CheckValues(obj.transform, Vector3.one, new Vector3(10, 20, 30), Vector3.one + Vector3.one);
        }

        private const float Epsilon = 0.0001f;

        private void CheckValues(Transform transform, Vector3 pos, Vector3 euler, Vector3 scale)
        {
            Assert.GreaterOrEqual(Epsilon, Vector3.Distance(pos, transform.position));
            Assert.GreaterOrEqual(Epsilon, Vector3.Distance(euler, transform.eulerAngles));
            Assert.GreaterOrEqual(Epsilon, Vector3.Distance(scale, transform.localScale));
        }
    }
}