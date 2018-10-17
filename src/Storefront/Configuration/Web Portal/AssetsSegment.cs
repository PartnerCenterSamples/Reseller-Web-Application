// -----------------------------------------------------------------------
// <copyright file="AssetsSegment.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.WebPortal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an assets segment in the portal configuration. This segment may hold more than one versioned asset sets.
    /// </summary>
    public class AssetsSegment : ICloneable
    {
        /// <summary>
        /// Gets or sets the segment name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the default assets version to use. If not specified then the first asset in the collection will be used as the default.
        /// </summary>
        public string DefaultAssetVersion { get; set; }

        /// <summary>
        /// Gets or sets a collection of asset sets.
        /// </summary>
        public IList<Assets> Assets { get; set; }

        /// <summary>
        /// Ensures the assets segment is valid.
        /// </summary>
        /// <exception cref="InvalidOperationException">If validation fails.</exception>
        public void Validate()
        {
            // ensure there are assets specified
            if (this.Assets == null || this.Assets.Count <= 0)
            {
                throw new InvalidOperationException("Assets not set. Please specify at least one assets version.");
            }

            bool isDefaultAssetVersionValid = false;

            if (string.IsNullOrWhiteSpace(this.DefaultAssetVersion))
            {
                // if no default asset version is specified, set it to the first assets set
                this.DefaultAssetVersion = this.Assets[0].Version;
                isDefaultAssetVersionValid = true;
            }

            // validate these assets
            foreach (Assets assetSet in this.Assets)
            {
                assetSet.Validate();

                if (!isDefaultAssetVersionValid && assetSet.Version == this.DefaultAssetVersion)
                {
                    isDefaultAssetVersionValid = true;
                }
            }

            // ensure the given default asset version is valid
            if (!isDefaultAssetVersionValid)
            {
                throw new InvalidOperationException("Invalid default asset version.");
            }
        }

        /// <summary>
        /// Clones an <see cref="AssetsSegment"/> instance.
        /// </summary>
        /// <returns>A deep copy of the <see cref="AssetsSegment"/> object.</returns>
        public object Clone()
        {
            return new AssetsSegment() { Name = this.Name, DefaultAssetVersion = this.DefaultAssetVersion, Assets = this.Assets.Clone() };
        }
    }
}