using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackgroundProcessing.Core.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BackgroundProcessing.Caching
{
    /// <summary>
    /// <see cref="IBackgroundCommandEventsSerializer"/> implementation that uses JSON.NET.
    /// </summary>
    public class JsonNetBackgroundCommandEventSerializer : IBackgroundCommandEventsSerializer
    {
        /// <summary>
        /// Gets the default <see cref="JsonSerializerSettings"/> that will be used if no custom value is provided.
        /// </summary>
        public static readonly JsonSerializerSettings DefaultSerializerSettings = CreateDefaultSerializerSettings();

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetBackgroundCommandEventSerializer"/> class.
        /// </summary>
        /// <param name="jsonSerializerSettings">Custom <see cref="JsonSerializerSettings"/> to use if any.</param>
        public JsonNetBackgroundCommandEventSerializer(JsonSerializerSettings jsonSerializerSettings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings ?? DefaultSerializerSettings;
        }

        /// <inheritdoc />
        public async Task<string> SerializeAsync(IEnumerable<BackgroundCommandEvent> events, CancellationToken cancellationToken = default)
            => JsonConvert.SerializeObject(events, _jsonSerializerSettings);

        /// <inheritdoc />
        public async Task<IList<BackgroundCommandEvent>> DeserializeAsync(string value, CancellationToken cancellationToken = default)
            => JsonConvert.DeserializeObject<List<BackgroundCommandEvent>>(value, _jsonSerializerSettings);

        private static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            };
            settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            return settings;
        }
    }
}
