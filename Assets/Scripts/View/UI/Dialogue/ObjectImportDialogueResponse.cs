using GeoViewer.Model.Globe;

namespace GeoViewer.View.UI.Dialogue
{
    /// <summary>
    /// A collection of user-selected paths which point at the data required to import a model
    /// </summary>
    public record ObjectImportDialogueResponse
    {
        /// <summary>
        /// The file path where the model can be found.
        /// </summary>
        public string? ModelPath { get; }

        /// <summary>
        /// The file path where the coordinates of the model can be found.
        /// </summary>
        public GlobePoint? GlobePoint { get; }

        /// <summary>
        /// Standard constructor which sets the model file path and the coordinates file path.
        /// </summary>
        /// <param name="modelPath">the file path where the model is located</param>
        /// <param name="globePoint">the <see cref="GlobePoint"/> selected by the user</param>
        public ObjectImportDialogueResponse(string? modelPath, GlobePoint? globePoint)
        {
            ModelPath = modelPath;
            GlobePoint = globePoint;
        }
    }
}