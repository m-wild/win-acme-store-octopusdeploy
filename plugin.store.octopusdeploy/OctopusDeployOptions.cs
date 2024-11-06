using PKISharp.WACS.Configuration;
using PKISharp.WACS.Configuration.Arguments;
using PKISharp.WACS.Plugins.Base.Factories;
using PKISharp.WACS.Plugins.Base.Options;
using PKISharp.WACS.Services;
using PKISharp.WACS.Services.Serialization;
using System.Text.Json.Serialization;

namespace PKISharp.WACS.Plugins.StorePlugins
{

    [JsonSerializable(typeof(OctopusDeployOptions))]
    public partial class OctopusDeployJson : JsonSerializerContext 
    {
        public OctopusDeployJson(WacsJsonPluginsOptionsFactory optionsFactory) : base(optionsFactory.Options) { }
    }

    public class OctopusDeployOptions : StorePluginOptions
    {
        public string? OctopusUrl { get; set; }
        public string? OctopusSpaceId { get; set; }
        public string? OctopusEnvironmentId { get; set; }

        [JsonPropertyName("OctopusApiKeySafe")]
        public ProtectedString? OctopusApiKey { get; set; }
    }

    public class OctopusDeployArguments : BaseArguments
    {
        public override string Name { get; } = "OctopusDeploy";
        public override string Group { get; } = "Store";
        public override string Condition { get; } = "--store octopusdeploy";

        [CommandLine(Description = "The URL of the OctopusDeploy server")]
        public string? OctopusUrl { get; set; }

        [CommandLine(Description = "The API Key to access OctopusDeploy API")]
        public string? OctopusApiKey { get; set; }

        [CommandLine(Description = "The ID of the space to import the cert to")]
        public string? OctopusSpaceId { get; set; }

        [CommandLine(Description = "The ID of the environment to make the certificate available to")]
        public string? OctopusEnvironmentId { get; set; }
    }

    public class OctopusDeployOptionsFactory : PluginOptionsFactory<OctopusDeployOptions>
    {
        private readonly ArgumentsInputService _arguments;

        public OctopusDeployOptionsFactory(ArgumentsInputService arguments)
        {
            _arguments = arguments;
        }

        private ArgumentResult<string?> OctopusUrl => _arguments
            .GetString<OctopusDeployArguments>(a => a.OctopusUrl)
            .Required();

        private ArgumentResult<ProtectedString?> OctopusApiKey => _arguments
            .GetProtectedString<OctopusDeployArguments>(a => a.OctopusApiKey)
            .Required();

        private ArgumentResult<string?> OctopusSpaceId => _arguments
            .GetString<OctopusDeployArguments>(a => a.OctopusSpaceId)
            .DefaultAsNull();

        private ArgumentResult<string?> OctopusEnvironmentId => _arguments
            .GetString<OctopusDeployArguments>(a => a.OctopusEnvironmentId)
            .DefaultAsNull();


        public override async Task<OctopusDeployOptions?> Aquire(IInputService input, RunLevel runLevel)
        {
            var options = new OctopusDeployOptions
            {
                OctopusUrl = await OctopusUrl.Interactive(input, "Octopus URL").GetValue(),
                OctopusApiKey = await OctopusApiKey.Interactive(input, "Octopus API Key").GetValue(),
                OctopusSpaceId = await OctopusSpaceId.Interactive(input, "Octopus Space ID").GetValue(),
                OctopusEnvironmentId = await OctopusEnvironmentId.Interactive(input, "Octopus Environment ID").GetValue(),
            };

            return options;
        }

        public override async Task<OctopusDeployOptions?> Default()
        {
            var options = new OctopusDeployOptions
            {
                OctopusUrl = await OctopusUrl.GetValue(),
                OctopusApiKey = await OctopusApiKey.GetValue(),
                OctopusSpaceId = await OctopusSpaceId.GetValue(),
                OctopusEnvironmentId = await OctopusEnvironmentId.GetValue(),
            };

            return options;
        }

        public override IEnumerable<(CommandLineAttribute, object?)> Describe(OctopusDeployOptions options)
        {
            yield return (OctopusUrl.Meta, options.OctopusUrl);
            yield return (OctopusApiKey.Meta, options.OctopusApiKey);
            yield return (OctopusSpaceId.Meta, options.OctopusSpaceId);
            yield return (OctopusEnvironmentId.Meta, options.OctopusEnvironmentId);
        }
    }
}