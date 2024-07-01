using System;
using GeoViewer.Controller.Input;
using GeoViewer.Controller.Tools.BuiltinTools;
using GeoViewer.Model.Tools;
using NUnit.Framework;

namespace GeoViewer.Test.Editor.Model.Tools
{
    public class ToolRegistryTest
    {
        private ToolRegistry _registry;

        [SetUp]
        public void Init()
        {
            _registry = new ToolRegistry();
        }

        [Test]
        public void RegisterToolTwice()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            Assert.DoesNotThrow(() => _registry.RegisterTool(tool));
            Assert.Throws<ArgumentException>(() => _registry.RegisterTool(tool));
        }

        [Test]
        public void ActivateInWrongRegistry()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            var registry2 = new ToolRegistry();

            var id = _registry.RegisterTool(tool);
            Assert.False(registry2.TrySetActiveTool(id));
        }

        [Test]
        public void RegisterTool()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            Assert.DoesNotThrow(() => _registry.RegisterTool(tool));
        }

        [Test]
        public void SetActiveTool()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            var id = _registry.RegisterTool(tool);
            Assert.True(_registry.TrySetActiveTool(id));
        }

        [Test]
        public void SetActiveToolNotRegistered()
        {
            var id = new ToolID(new DistanceTool(new Inputs(new InputManager())), 0, _registry);
            Assert.False(_registry.TrySetActiveTool(id));
        }

        [Test]
        public void RaiseActiveToolEvent()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            var eventRaised = false;

            var id = _registry.RegisterTool(tool);

            _registry.ActiveToolChangedEvent += (_, _) => eventRaised = true;

            Assert.True(_registry.TrySetActiveTool(id));

            Assert.True(eventRaised);
        }

        [Test]
        public void DontRaiseActiveToolEvent()
        {
            var tool = new SelectionTool(new Inputs(new InputManager()));

            var eventRaised = false;

            var id = _registry.RegisterTool(tool);

            Assert.True(_registry.TrySetActiveTool(id));

            // dont raise an event if nothing changes
            _registry.ActiveToolChangedEvent += (_, _) => eventRaised = true;
            Assert.True(_registry.TrySetActiveTool(id));
            Assert.False(eventRaised, nameof(eventRaised));
        }
    }
}