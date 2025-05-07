using System;
using System.Collections.Generic;
using GeoViewer.Controller.Util;
using GeoViewer.Model.Globe;
using GeoViewer.Model.Grid;
using JsonSubTypes;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

namespace GeoViewer.Controller.Map.Projection
{
    /// <summary>
    /// An interface defining methods to
    /// - project points from <see cref="GlobePoint"/>s (lla) to positions on a flat plane
    /// - getting the absolute tile coordinates of a tile at a given <see cref="GlobePoint"/>
    /// and their respective inverse.
    /// Also provides a Method for getting the factor by what objects at a given <see cref="GlobePoint"/>
    /// have to be scaled to match the surroundings, because most projections (most notably the Mercator-Projection)
    /// do not sustain scale.
    /// </summary>
    public interface IProjection
    {
        /// <summary>
        /// Projects a <see cref="GlobePoint"/> onto a flat plane.
        /// The altitude is the new y-Coordinate and will be scaled, based on the given <see cref="GlobePoint"/>.
        /// </summary>
        /// <param name="globePoint">the <see cref="GlobePoint"/> to project</param>
        /// <returns>A position as a <see cref="double3"/> with the altitude as the y-Coordinate</returns>
        public double3 GlobePointToPosition(GlobePoint globePoint);

        /// <summary>
        /// Calculates the <see cref="GlobePoint"/> matching the given position. This assumes that the Altitude is scaled
        /// based on the <see cref="GlobePoint"/>.
        /// </summary>
        /// <param name="position">The position to calculate the <see cref="GlobePoint"/> for as a <see cref="double3"/></param>
        /// <returns>A <see cref="GlobePoint"/> matching the given position</returns>
        public GlobePoint PositionToGlobePoint(double3 position);

        /// <summary>
        /// Calculates the factor, by which an object would need to get scaled, to match the surroundings.
        /// This is used for non-scale sustaining projections (e.g. the Mercator-Projection).
        /// </summary>
        /// <param name="globePoint">The <see cref="GlobePoint"/> to calculate the scale factor at</param>
        /// <returns>A scale factor as a double. If the argument given to this function is null, it returns 1</returns>
        public double GetScaleFactor(GlobePoint? globePoint);
    }
}