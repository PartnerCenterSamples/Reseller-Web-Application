// -----------------------------------------------------------------------
// <copyright file="ConfigurationExtensions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.WebPortal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Contains useful extension methods for configuration classes.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Clones a collection of cloneable objects.
        /// </summary>
        /// <typeparam name="T">The list object type.</typeparam>
        /// <param name="collection">The list to clone.</param>
        /// <returns>A deep copy of the collection.</returns>
        public static IList<T> Clone<T>(this IEnumerable<T> collection)
            where T : ICloneable
        {
            IList<T> cloneCollection = new List<T>();

            foreach (var item in collection)
            {
                cloneCollection.Add((T)item.Clone());
            }

            return cloneCollection;
        }

        /// <summary>
        /// Retrieves the assets from a list using its version.
        /// </summary>
        /// <param name="assetsList">The assets list to search.</param>
        /// <param name="version">The version to look for.</param>
        /// <returns>The matching assets or empty assets object in case it was not found.</returns>
        public static Assets GetAssetsByVersion(this IList<Assets> assetsList, string version)
        {
            Assets assetsMatch = null;

            if (assetsList != null && assetsList.Count > 0)
            {
                // search for the asset version
                foreach (Assets assets in assetsList)
                {
                    if (assets.Version == version)
                    {
                        assetsMatch = assets;
                        break;
                    }
                }

                if (assetsMatch == null)
                {
                    // set if to the first entry if not found
                    assetsMatch = assetsList[0];
                }
            }
            else
            {
                // no valid assets, return an empty asset set
                assetsMatch = new Assets();
            }

            return assetsMatch;
        }

        /// <summary>
        /// Aggregates an assets segment assets. Uses default asset versions.
        /// </summary>
        /// <param name="assetSegmentList">The assets segment list.</param>
        /// <returns>Aggregated assets.</returns>
        public static Assets AggregateAssets(this IEnumerable<AssetsSegment> assetSegmentList)
        {
            Assets combinedSegmentListAssets = new Assets();

            if (assetSegmentList != null)
            {
                foreach (AssetsSegment segmentAssets in assetSegmentList)
                {
                    combinedSegmentListAssets += segmentAssets.Assets.GetAssetsByVersion(segmentAssets.DefaultAssetVersion);
                }
            }

            return combinedSegmentListAssets;
        }
    }
}