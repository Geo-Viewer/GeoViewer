using System;
using System.Globalization;
using System.IO;
using Unity.Mathematics;

namespace GeoViewer.Controller.Util
{
    /// <summary>
    /// A utility class for reading the ecef data out of a text file
    /// </summary>
    public static class EcefReader
    {
        /// <summary>
        /// The character indicating a line is a comment
        /// </summary>
        private const char CommentIndicator = '#';

        /// <summary>
        /// The character between the values
        /// </summary>
        private const char ValueSeparator = ' ';

        /// <summary>
        /// Reads the Ecef coordinates from a text file in a certain format.
        /// </summary>
        /// <param name="filePath">The path to the file to read from</param>
        /// <returns>The Ecef coordinates as a <see cref="double3"/></returns>
        /// <exception cref="ArgumentException">Thrown if there's no file at the given path</exception>
        /// <exception cref="FormatException">Thrown if the file does not have the expected format</exception>
        public static double3 ReadEcefFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException("There's no file at the given Path");
            }

            return ReadEcefFromFile(File.OpenRead(filePath));
        }

        /// <summary>
        /// Reads the Ecef coordinates from a text file in a certain format.
        /// </summary>
        /// <param name="file">The stream of the file to read</param>
        /// <returns>The Ecef coordinates as a <see cref="double3"/></returns>
        /// <exception cref="FormatException">Thrown if the file does not have the expected format</exception>
        public static double3 ReadEcefFromFile(Stream file)
        {
            using var reader = new StreamReader(file);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line?.StartsWith(CommentIndicator) != false)
                {
                    //skip line
                    continue;
                }

                //Read Values
                var values = line.Split(ValueSeparator);

                try
                {
                    var x = double.Parse(values[0], CultureInfo.InvariantCulture);
                    var y = double.Parse(values[1], CultureInfo.InvariantCulture);
                    var z = double.Parse(values[2], CultureInfo.InvariantCulture);

                    return new double3(x, y, z);
                }
                catch (Exception)
                {
                    throw new FormatException("File does not have the expected format");
                }
            }

            throw new FormatException("File does not have the expected format");
        }
    }
}