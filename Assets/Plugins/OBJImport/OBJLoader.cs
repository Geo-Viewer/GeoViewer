﻿/*
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

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dummiesman
{
    public enum SplitMode
    {
        None,
        Object,
        Material
    }

    public class OBJLoader
    {
        //options
        /// <summary>
        /// Determines how objects will be created
        /// </summary>
        public SplitMode SplitMode = SplitMode.Object;

        //global lists, accessed by objobjectbuilder
        internal List<Vector3> Vertices = new List<Vector3>();
        internal List<Vector3> Normals = new List<Vector3>();
        internal List<Vector2> UVs = new List<Vector2>();

        //materials, accessed by objobjectbuilder
        internal Dictionary<string, Material> Materials;

        //file info for files loaded from file path, used for GameObject naming and MTL finding
        private FileInfo _objInfo;

        //vector in obj file with the lowest z value
        private Vector3 _lowestVector = Vector3.positiveInfinity;

        /// <summary>
        /// Contains the estimated progress of the ongoing loading process,
        /// ranging from 0.0 to 1.0.
        /// </summary>
        public float Progress { get; private set; }
#if UNITY_EDITOR
        [MenuItem("GameObject/Import From OBJ")]
        static void ObjLoadMenu()
        {
            string pth = EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
            if (!string.IsNullOrEmpty(pth))
            {
                System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
                s.Start();

                var loader = new OBJLoader
                {
                    SplitMode = SplitMode.Object,
                };
                loader.Load(pth);

                Debug.Log($"OBJ import time: {s.ElapsedMilliseconds}ms");
                s.Stop();
            }
        }
#endif

        /// <summary>
        /// Helper function to load mtllib statements
        /// </summary>
        /// <param name="mtlLibPath"></param>
        private async Task LoadMaterialLibrary(string mtlLibPath)
        {
            if (_objInfo != null && File.Exists(Path.Combine(_objInfo.Directory.FullName, mtlLibPath)))
            {
                Materials = await new MTLLoader().Load(Path.Combine(_objInfo.Directory.FullName, mtlLibPath));
                return;
            }

            if (File.Exists(mtlLibPath))
            {
                Materials = await new MTLLoader().Load(mtlLibPath);
                return;
            }
        }

        /// <summary>
        /// Load an OBJ file from a stream. No materials will be loaded, and will instead be supplemented by a blank white material.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public async Task<Dictionary<string, OBJObjectBuilder>> Load(Stream input)
        {
            var reader = new StreamReader(input);

            Dictionary<string, OBJObjectBuilder> builderDict = new Dictionary<string, OBJObjectBuilder>();
            OBJObjectBuilder currentBuilder = null;
            string currentMaterial = "default";

            //lists for face data
            //prevents excess GC
            List<int> vertexIndices = new List<int>();
            List<int> normalIndices = new List<int>();
            List<int> uvIndices = new List<int>();

            //helper func
            Action<string> setCurrentObjectFunc = (string objectName) =>
            {
                if (!builderDict.TryGetValue(objectName, out currentBuilder))
                {
                    currentBuilder = new OBJObjectBuilder(objectName, this);
                    builderDict[objectName] = currentBuilder;
                }
            };

            //create default object
            setCurrentObjectFunc.Invoke("default");

            //var buffer = new DoubleBuffer(reader, 256 * 1024);
            var buffer = new CharWordReader(reader, 4 * 1024);

            //count lines for progress
            var length = input.Length;

            //do the reading
            while (true)
            {
                Progress = (float)input.Position / length;
                buffer.SkipWhitespaces();

                if (buffer.endReached)
                {
                    break;
                }

                buffer.ReadUntilWhiteSpace();

                //comment or blank
                if (buffer.Is("#"))
                {
                    buffer.SkipUntilNewLine();
                    continue;
                }

                if (Materials == null && buffer.Is("mtllib"))
                {
                    buffer.SkipWhitespaces();
                    buffer.ReadUntilNewLine();
                    string mtlLibPath = buffer.GetString();
                    await LoadMaterialLibrary(mtlLibPath);
                    continue;
                }

                if (buffer.Is("v"))
                {
                    var currentVector = buffer.ReadVector(true);
                    if (currentVector.y < _lowestVector.y)
                    {
                        _lowestVector = currentVector;
                    }

                    Vertices.Add(currentVector);
                    continue;
                }

                //normal
                if (buffer.Is("vn"))
                {
                    Normals.Add(buffer.ReadVector());
                    continue;
                }

                //uv
                if (buffer.Is("vt"))
                {
                    UVs.Add(buffer.ReadVector());
                    continue;
                }

                //new material
                if (buffer.Is("usemtl"))
                {
                    buffer.SkipWhitespaces();
                    buffer.ReadUntilNewLine();
                    string materialName = buffer.GetString();
                    currentMaterial = materialName;

                    if (SplitMode == SplitMode.Material)
                    {
                        setCurrentObjectFunc.Invoke(materialName);
                    }

                    continue;
                }

                //new object
                if ((buffer.Is("o") || buffer.Is("g")) && SplitMode == SplitMode.Object)
                {
                    buffer.ReadUntilNewLine();
                    string objectName = buffer.GetString(1);
                    setCurrentObjectFunc.Invoke(objectName);
                    continue;
                }

                //face data (the fun part)
                if (buffer.Is("f"))
                {
                    //loop through indices
                    while (true)
                    {
                        buffer.SkipWhitespaces(out var newLinePassed);
                        if (newLinePassed)
                        {
                            break;
                        }

                        int vertexIndex = int.MinValue;
                        int normalIndex = int.MinValue;
                        int uvIndex = int.MinValue;

                        vertexIndex = buffer.ReadInt();
                        if (buffer.currentChar == '/')
                        {
                            buffer.MoveNext();
                            if (buffer.currentChar != '/')
                            {
                                uvIndex = buffer.ReadInt();
                            }

                            if (buffer.currentChar == '/')
                            {
                                buffer.MoveNext();
                                normalIndex = buffer.ReadInt();
                            }
                        }

                        //"postprocess" indices
                        if (vertexIndex > int.MinValue)
                        {
                            if (vertexIndex < 0)
                                vertexIndex = Vertices.Count - vertexIndex;
                            vertexIndex--;
                        }

                        if (normalIndex > int.MinValue)
                        {
                            if (normalIndex < 0)
                                normalIndex = Normals.Count - normalIndex;
                            normalIndex--;
                        }

                        if (uvIndex > int.MinValue)
                        {
                            if (uvIndex < 0)
                                uvIndex = UVs.Count - uvIndex;
                            uvIndex--;
                        }

                        //set array values
                        vertexIndices.Add(vertexIndex);
                        normalIndices.Add(normalIndex);
                        uvIndices.Add(uvIndex);
                    }

                    //push to builder
                    currentBuilder.PushFace(currentMaterial, vertexIndices, normalIndices, uvIndices);

                    //clear lists
                    vertexIndices.Clear();
                    normalIndices.Clear();
                    uvIndices.Clear();

                    continue;
                }

                buffer.SkipUntilNewLine();
            }

            return builderDict;
        }

        private GameObject BuildObject(Dictionary<string, OBJObjectBuilder> builderDict)
        {
            //finally, put it all together
            GameObject obj =
                new GameObject(_objInfo != null ? Path.GetFileNameWithoutExtension(_objInfo.Name) : "WavefrontObject");

            foreach (var builder in builderDict)
            {
                //empty object
                if (builder.Value.PushedFaceCount == 0)
                    continue;

                var builtObj = builder.Value.Build();
                builtObj.layer = 7;
                builtObj.AddComponent<MeshCollider>();
                builtObj.transform.SetParent(obj.transform, false);
            }

            return obj;
        }

        /// <summary>
        /// Load an OBJ and MTL file from a stream.
        /// </summary>
        /// <param name="input">Input OBJ stream</param>
        /// /// <param name="mtlInput">Input MTL stream</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public async Task<GameObject> Load(Stream input, Stream mtlInput)
        {
            var mtlLoader = new MTLLoader();
            Materials = await mtlLoader.Load(mtlInput);

            return BuildObject(await Task.Run(() => Load(input)));
        }

        /// <summary>
        /// Load an OBJ and MTL file from a file path.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// /// <param name="mtlPath">Input MTL path</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public async Task<GameObject> Load(string path, string mtlPath)
        {
            _objInfo = new FileInfo(path);
            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {
                var mtlLoader = new MTLLoader();

                Materials = await mtlLoader.Load(mtlPath);

                await using var fs = new FileStream(path, FileMode.Open);
                return BuildObject(await Task.Run(() => Load(fs)));
            }
            else
            {
                await using var fs = new FileStream(path, FileMode.Open);
                return BuildObject(await Task.Run(() => Load(fs)));
            }
        }

        /// <summary>
        /// Load an OBJ file from a file path. This function will also attempt to load the MTL defined in the OBJ file.
        /// </summary>
        /// <param name="path">Input OBJ path</param>
        /// <returns>Returns a GameObject represeting the OBJ file, with each imported object as a child.</returns>
        public async Task<GameObject> Load(string path)
        {
            return await Task.Run(() => Load(path, null));
        }

        /// <summary>
        /// Gets the vector of the vertex of the obj model with the lowest height value.
        /// This vector is used to determine the height the obj model GameObject has to be placed in the main scene
        /// so the visual of that GameObject fits in with a plane at height = 0.
        /// </summary>
        /// <returns>the vector with the lowest z value</returns>
        public Vector3 GetLowestVector()
        {
            return _lowestVector;
        }
    }
}