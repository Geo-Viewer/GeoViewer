/*
 * Copyright (c) 2019 Dummiesman
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Dummiesman;
using GeoViewer.Model.State;
using UnityEngine;

namespace GeoViewer.Controller.ObjLoading
{
    /// <summary>
    /// Class responsible for loading objects via an .obj file into the scene at runtime.
    /// IMPORTANT: User needs to have all relevant files (obj file, mtl file, jpg file) in a common directory.
    /// </summary>
    public class ObjLoader : MonoBehaviour
    {
        private const string LayerDefaultName = "Default";
        private const string LayerSelectableName = "Selectable";
        private static int _layerDefault;
        private static int _layerSelectable;
        public static GameObject? LoadedObject;
        private static OBJLoader? _objLoader;

        /// <summary>
        /// Contains the estimated progress of the ongoing loading process,
        /// ranging from 0.0 to 1.0.
        /// If no object is being loaded, this returns 0.
        /// </summary>
        public static float Progress => _objLoader?.Progress ?? 0;

        private void Awake()
        {
            _layerDefault = LayerMask.NameToLayer(LayerDefaultName);
            _layerSelectable = LayerMask.NameToLayer(LayerSelectableName);
        }

        /// <summary>
        /// Gets the path of a mtl file of a given obj file path.
        /// IMPORTANT: The mtl file has to be in the same directory as the obj file.
        /// </summary>
        /// <param name="objPath">A file path to an obj file.</param>
        /// <returns>A file path to the mtl file linked to the obj file.</returns>
        /// <exception cref="FileNotFoundException">File was not found at the given obj file path.</exception>
        /// <exception cref="DirectoryNotFoundException">Directory was not found at the given path.</exception>
        private static async Task<string?> GetMtlPathFromObjFile(string objPath)
        {
            if (!File.Exists(objPath))
            {
                throw new FileNotFoundException($"OBJ File not found at: {objPath}");
            }

            var mtlFilePath = string.Empty;
            var objFileDirectory = Path.GetDirectoryName(objPath);

            if (!Directory.Exists(objFileDirectory))
            {
                throw new DirectoryNotFoundException($"Directory not found: {objFileDirectory}");
            }

            var reader = new StreamReader(objPath);
            while (!reader.EndOfStream)
            {
                // cannot be null as we haven't reached end of stream
                var line = await reader.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                if (!line.StartsWith("mtllib"))
                {
                    continue;
                }

                var arguments = line.Split();
                var mtlFileName = arguments[^1];
                mtlFilePath = Path.Combine(objFileDirectory, mtlFileName);
                break;
            }

            reader.Close();
            return !File.Exists(mtlFilePath) ? null : mtlFilePath;
        }

        /// <summary>
        /// <para>
        /// Combines the meshes of the children object to a single mesh and assigns it to the parent object.
        /// Additionally the parent object gets a mesh renderer, a mesh collider and a standard material.
        /// </para>
        /// <para>See also: https://docs.unity3d.com/ScriptReference/Mesh.CombineMeshes.html</para>
        /// </summary>
        private static void CombineChildMeshes()
        {
            // if no object is loaded, we can't combine any meshes
            if (LoadedObject is null)
            {
                return;
            }

            var meshFilters = LoadedObject.GetComponentsInChildren<MeshFilter>();
            var combineInstances = new CombineInstance[meshFilters.Length];

            for (var i = 0; i < meshFilters.Length; i++)
            {
                combineInstances[i].mesh = meshFilters[i].sharedMesh;
                combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
            }

            var combinedMesh = new Mesh
            {
                name = "combinedMesh"
            };
            combinedMesh.CombineMeshes(combineInstances, false, false);
            var meshFilter = LoadedObject.AddComponent<MeshFilter>();
            meshFilter.mesh = combinedMesh;
            var meshCollider = LoadedObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = combinedMesh;
            var meshRenderer = LoadedObject.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        /// <summary>
        /// Iterates trough all of the child GameObject of a given parent GameObject and
        /// sets their layer to the given layer parameter.
        /// </summary>
        /// <param name="parent">The parent GameObject.</param>
        /// <param name="layer">The layer the child GameObjects should get. </param>
        private static void SetChildObjectsLayer(GameObject parent, int layer)
        {
            foreach (Transform child in parent.transform)
            {
                child.gameObject.layer = layer;
            }
        }

        /// <summary>
        /// Loads a 3d model from a given obj file.
        /// The loaded model will have a standard white material.
        /// If a jpg file is found, the model will have a texture.
        /// </summary>
        /// <param name="objPath">A file path to an obj file.</param>
        /// <exception cref="FileNotFoundException">Thrown when theres no file at the given path</exception>
        public static async Task LoadModel(string? objPath)
        {
            _objLoader = new OBJLoader();
            //Clear old model
            if (LoadedObject != null)
            {
                LoadedObject.layer = _layerDefault;
                SetChildObjectsLayer(LoadedObject, _layerDefault);
                LoadedObject.GetComponent<SceneObject>().Destroy();
            }

            if (objPath == null)
            {
                return;
            }

            if (!File.Exists(objPath))
            {
                throw new FileNotFoundException($"{objPath} is not a valid path");
            }

            var mtlPath = await GetMtlPathFromObjFile(objPath);
            LoadedObject = await _objLoader.Load(objPath, mtlPath);
            var sceneObject = LoadedObject.AddComponent<SceneObject>();
            ApplicationState.Instance.AddSceneObject(sceneObject);

            if (LoadedObject.transform.childCount > 1)
            {
                SetChildObjectsLayer(LoadedObject, _layerDefault);
                CombineChildMeshes();
                LoadedObject.layer = _layerSelectable;
            }

            //lowest point of the loaded object's mesh.
            //this point helps adjusting the height of the model so that this point is at height 0 in the scene.
            var lowestYValue = _objLoader.GetLowestVector().y;
            ApplicationState.Instance.MapRenderer.AttachToMap(sceneObject, 
                AttachmentMode.RelativeToSurface, 
                ApplicationState.Instance.MapRenderer.Origin, 
                -lowestYValue);
        }
    }
}