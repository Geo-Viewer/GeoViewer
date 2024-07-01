using System.Collections;
using System.IO;
using System.Threading.Tasks;
using GeoViewer.Controller.ObjLoading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace GeoViewer.Test.Player.Controller.ObjLoading
{
    public class ObjLoadingTest
    {
        private static readonly string[] _paths = Directory.GetDirectories(Directory.GetCurrentDirectory(), "TestModel", SearchOption.AllDirectories);
        private const string ObjFileName = "textured_mesh.obj";
        private readonly string _validPath = Path.Combine(_paths[0], ObjFileName);
        private const string InvalidPath = "!?=";

        [SetUp]
        public void Setup()
        {
            new GameObject().AddComponent<ObjLoader>();
        }

        [UnityTest]
        public IEnumerator ObjLoadingWithValidObjPath()
        {
            var task = LoadModelTest(_validPath);

            yield return AsCoroutine(task);
            Assert.IsTrue(task.Result);
        }

        [UnityTest]
        public IEnumerator ObjLoadingWithInvalidObjPath()
        {
            var task = LoadModelTest(InvalidPath);

            yield return AsCoroutine(task);
            Assert.IsFalse(task.Result);
        }

        async Task<bool> LoadModelTest(string objPath)
        {
            try
            {
                await ObjLoader.LoadModel(objPath);
                await ObjLoader.LoadModel(null);
            }
            catch (FileNotFoundException e)
            {
                return false;
            }

            return true;
        }

        private static IEnumerator AsCoroutine(Task task)
        {
            while (!task.IsCompleted) yield return null;
            task.GetAwaiter().GetResult();
        }
    }
}
