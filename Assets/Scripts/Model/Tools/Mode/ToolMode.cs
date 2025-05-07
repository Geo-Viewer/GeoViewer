using System.Collections.Generic;

namespace GeoViewer.Model.Tools.Mode
{
    /// <summary>
    /// Contains a set of application features which can be reserved by a tool.
    /// </summary>
    public class ToolMode
    {
        private readonly HashSet<ApplicationFeature> _guarantees;

        /// <summary>
        /// Constructs a new ToolMode which doesn't reserve any features.
        /// </summary>
        public ToolMode()
        {
            _guarantees = new HashSet<ApplicationFeature>();
        }

        /// <summary>
        /// Constructs a new ToolMode which reserved the given <paramref name="features"/>.
        /// </summary>
        /// <param name="features">The features the ToolMode should reserve.</param>
        public ToolMode(ICollection<ApplicationFeature> features)
        {
            _guarantees = new HashSet<ApplicationFeature>(features);
        }

        /// <summary>
        /// Returns true if and only if the ToolMode doesn't reserve the given <see cref="ApplicationFeature"/>.
        /// </summary>
        /// <param name="feature">The feature which should be checked.</param>
        public bool CanAppUse(ApplicationFeature feature)
        {
            return !_guarantees.Contains(feature);
        }

        /// <summary>
        /// A Builder class to generate <see cref="ToolMode"/>s.
        /// </summary>
        public class Builder
        {
            private readonly List<ApplicationFeature> _features = new();

            /// <summary>
            /// Add a feature to the built <see cref="ToolMode"/>.
            /// </summary>
            /// <param name="feature">The feature to add.</param>
            /// <returns>the builder</returns>
            public Builder WithFeature(ApplicationFeature feature)
            {
                _features.Add(feature);
                return this;
            }

            /// <summary>
            /// Builds the tool mode from the builder.
            /// </summary>
            /// <returns>A new <see cref="ToolMode"/> with the features given to the builder.</returns>
            public ToolMode Build()
            {
                return new ToolMode(_features);
            }
        }
    }
}