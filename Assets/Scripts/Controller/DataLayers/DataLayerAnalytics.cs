using System;
using GeoViewer.Controller.Util;

namespace GeoViewer.Controller.DataLayers
{
    public class DataLayerAnalytics
    {
        private const int AnalyticsTimeBufferSize = 100;

        private readonly AvgBufferInt _requestTimeBuffer = new(AnalyticsTimeBufferSize);
        public float AverageRequestTime => _requestTimeBuffer.Average;

        public void AddRequestTime(int milliseconds)
            => _requestTimeBuffer.Add(milliseconds);

        private readonly AvgBufferInt _renderTimeBuffer = new(AnalyticsTimeBufferSize);
        public float AverageRenderTime => _renderTimeBuffer.Average;

        public void AddRenderTime(int milliseconds)
            => _renderTimeBuffer.Add(milliseconds);

        public override string ToString()
        {
            return $"Average Render Time: {AverageRenderTime}ms {Environment.NewLine}" +
                   $"Average Request Time: {AverageRequestTime}ms";
        }
    }
}