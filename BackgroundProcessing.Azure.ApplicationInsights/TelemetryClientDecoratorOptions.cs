using System;
using System.Collections.Generic;
using BackgroundProcessing.Core;
using Microsoft.ApplicationInsights;

namespace BackgroundProcessing.Azure.ApplicationInsights
{
    /// <summary>
    /// Options for the <see cref="TelemetryClient"/> decorators.
    /// </summary>
    public class TelemetryClientDecoratorOptions
    {
        /// <summary>
        /// Gets or sets the delegate that allows to add additional event properties when processing or dispatching.
        /// The <see cref="IBackgroundCommand.Id"/> and <see cref="IBackgroundCommand.Timestamp"/> properties are already added.
        /// </summary>
        public Action<IBackgroundCommand, IDictionary<string, string>> AdditionalProperties { get; set; }
    }
}
