using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BackgroundProcessing.Core.Serializers
{
    /// <summary>
    /// <see cref="IBackgroundCommandSerializer"/> that serializes using JSON.NET.
    /// </summary>
    public class JsonNetBackgroundCommandSerializer : IBackgroundCommandSerializer
    {
        /// <summary>
        /// Gets the default <see cref="JsonSerializerSettings"/> that will be used if no custom value is provided.
        /// </summary>
        public static readonly JsonSerializerSettings DefaultSerializerSettings = CreateDefaultSerializerSettings();

        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonNetBackgroundCommandSerializer"/> class.
        /// </summary>
        /// <param name="jsonSerializerSettings">Custom <see cref="JsonSerializerSettings"/> to use if any.</param>
        public JsonNetBackgroundCommandSerializer(JsonSerializerSettings jsonSerializerSettings = null)
        {
            _jsonSerializerSettings = jsonSerializerSettings ?? DefaultSerializerSettings;
        }

        /// <inheritdoc />
        public async Task<string> SerializeAsync(IBackgroundCommand command, CancellationToken cancellationToken = default)
            => JsonConvert.SerializeObject(command, _jsonSerializerSettings);

        /// <inheritdoc />
        public async Task<IBackgroundCommand> DeserializeAsync(string value, CancellationToken cancellationToken = default)
            => JsonConvert.DeserializeObject<IBackgroundCommand>(value, _jsonSerializerSettings);

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
