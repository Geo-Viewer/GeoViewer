using System;
using System.IO;
using System.Threading.Tasks;
using SFB;
using UnityEngine;

namespace GeoViewer.Controller.Files
{
    /// <summary>
    /// A class for saving files to a subfolder on the system
    /// </summary>
    public class FileSaver
    {
        /// <summary>
        /// Stores the path to the save location
        /// </summary>
        private string _saveLocation = "empty";

        /// <summary>
        /// Creates a new <see cref="FileSaver"/> with a given subfolder name, by asking the user for a path to save to
        /// </summary>
        /// <exception cref="ArgumentException">Throws an exception if the selection of a folder path gets cancelled</exception>
        public FileSaver()
        {
            var path =
                StandaloneFileBrowser.OpenFolderPanel("Choose save directory", Application.dataPath, false);

            if (path.Length == 0)
            {
                throw new ArgumentException("Tried to set empty folder path");
            }

            Init(path[0]);
        }

        /// <summary>
        /// Creates a new <see cref="FileSaver"/> with a given subfolder name, by asking the user for a path to save to
        /// </summary>
        /// <param name="path">The path to the save location</param>
        public FileSaver(string path)
        {
            Init(path);
        }

        /// <summary>
        /// Initiates and checks the folder structure
        /// </summary>
        /// <param name="path">The path to the save location</param>
        private void Init(string path)
        {
            if (!Directory.Exists(path))
            {
                Debug.LogWarning($"The directory {path} does not exist. Creating directory....");
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Debug.LogWarning("Creating directory failed.");
                    throw;
                }
            }

            _saveLocation = path;
        }

        /// <summary>
        /// Saves a byte array as a file into the folder
        /// </summary>
        /// <param name="bytes">the byte array to save</param>
        /// <param name="fileName">the name of the file to save</param>
        /// <param name="format">file format ("png" for a .png file)</param>
        /// <exception cref="ArgumentException">Throws an exception if the path does not exist</exception>
        public async Task SaveBytes(byte[] bytes, string fileName, string format)
        {
            if (!Directory.Exists(_saveLocation))
            {
                throw new ArgumentException($"Tried to save at invalid path: {_saveLocation}");
            }

            await File.WriteAllBytesAsync(Path.Combine(_saveLocation, $"{fileName}.{format}"), bytes);
        }
    }
}