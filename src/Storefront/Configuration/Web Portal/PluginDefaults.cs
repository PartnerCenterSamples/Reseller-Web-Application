// -----------------------------------------------------------------------
// <copyright file="PluginDefaults.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Configuration.WebPortal
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Holds default property values for the web portal plugins.
    /// </summary>
    public class PluginDefaults : ICloneable
    {
        /// <summary>
        /// Gets or sets the plugin display name.
        /// </summary>
        [JsonProperty]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the plugin's tile icon.
        /// </summary>
        [JsonProperty("Tile")]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the plugin's theme color.
        /// </summary>
        [JsonProperty]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is hidden or not.
        /// </summary>
        [JsonProperty]
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the alternate color of the plugin's theme.
        /// </summary>
        [JsonProperty]
        public string AlternateColor { get; set; }

        /// <summary>
        /// Checks if the plugin defaults are properly set or not.
        /// </summary>
        /// <param name="featureHashtable">A feature hash table useful for cross referencing duplications in other plugins.</param>
        /// <exception cref="InvalidOperationException">If validation fails.</exception>
        public virtual void Validate(IDictionary<string, int> featureHashtable = null)
        {
            if (string.IsNullOrWhiteSpace(this.DisplayName))
            {
                throw new InvalidOperationException("DisplayName not set");
            }

            if (string.IsNullOrWhiteSpace(this.Image))
            {
                throw new InvalidOperationException("Tile not set");
            }

            if (string.IsNullOrWhiteSpace(this.Color))
            {
                throw new InvalidOperationException("Color not set");
            }

            if (string.IsNullOrWhiteSpace(this.AlternateColor))
            {
                throw new InvalidOperationException("AlternateColor not set");
            }
        }

        /// <summary>
        /// Clones the plugin defaults object.
        /// </summary>
        /// <returns>A deep clone of the plugin defaults object.</returns>
        public object Clone()
        {
            return new PluginDefaults()
            {
                AlternateColor = this.AlternateColor,
                Color = this.Color,
                DisplayName = this.DisplayName,
                Hidden = this.Hidden,
                Image = this.Image
            };
        }
    }
}