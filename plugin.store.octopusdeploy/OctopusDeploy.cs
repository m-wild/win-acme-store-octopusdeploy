using System.Runtime.Versioning;
using PKISharp.WACS.DomainObjects;
using PKISharp.WACS.Plugins.Interfaces;
using PKISharp.WACS.Services;
using System.Net.Http.Json;
using PKISharp.WACS.Extensions;
using PKISharp.WACS.Plugins.Base.Capabilities;
using System.Text.Json;

[assembly: SupportedOSPlatform("windows")]

namespace PKISharp.WACS.Plugins.StorePlugins
{
    [IPlugin.Plugin<
        OctopusDeployOptions, OctopusDeployOptionsFactory,
        DefaultCapability, OctopusDeployJson>
        ("5b6edcdf-4f61-420f-91c4-f66dbb264b66",
        Name, "OctopusDeploy Certificate Management")]
    public class OctopusDeploy : IStorePlugin, IDisposable
    {
        public const string Name = "OctopusDeploy";
        private readonly ILogService _log;
        private readonly OctopusDeployOptions _options;
        private readonly HttpClient _http;

        public OctopusDeploy(ILogService log, OctopusDeployOptions options)
        {
            _log = log;
            _options = options;

            var url = options.OctopusUrl ?? throw new ArgumentNullException(nameof(options.OctopusUrl));
            var apiKey = options.OctopusApiKey?.Value ?? throw new ArgumentNullException(nameof(options.OctopusApiKey));

            _http = new HttpClient();
            _http.BaseAddress = new Uri(url);
            _http.DefaultRequestHeaders.Add("X-Octopus-ApiKey", apiKey);
        }

        public async Task<StoreInfo?> Save(ICertificateInfo certificateInfo)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "api/certificates");
            request.Content = JsonContent.Create(new 
            {
                Name = certificateInfo.FriendlyName,
                Notes = "Managed by win-acme on OctopusDeploy server",
                CertificateData = new 
                {
                    HasValue = true,
                    NewValue = Convert.ToBase64String(certificateInfo.PfxBytes()),
                },
                SpaceId = _options.OctopusSpaceId,
                EnvironmentIds = _options.OctopusEnvironmentId == null ? Array.Empty<string>() : new[] { _options.OctopusEnvironmentId },
                Password = new 
                {
                    HasValue = false,
                },
                TenantedDeploymentParticipation = "Untenanted",
                TenantIds = Array.Empty<string>(),
                TenantTags = Array.Empty<string>(),
            });

            try 
            {
                var response = await _http.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();
                _log.Debug($"Octopus responded Status={(int)response.StatusCode} Content={content}");

                response.EnsureSuccessStatusCode();

                var responseModel = JsonSerializer.Deserialize<OctopusCertificate>(content);

                return new StoreInfo
                {
                    Path = responseModel?.Id,
                    Name = responseModel?.Name,
                };
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error importing certificate into OctopusDeploy");
            }

            return null;
        }

        public async Task Delete(ICertificateInfo certificateInfo)
        {
            var certs = await _http.GetFromJsonAsync<SearchCertificate>($"api/certificates?search={certificateInfo.Thumbprint}");
            if (certs?.Items == null || certs.Items.Count == 0)
                return;

            var id = certs.Items.First()?.Id;
            if (id == null)
                return;

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/certificates/{id}/archive");
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _http?.Dispose();
        }
    }

    public class SearchCertificate
    {
        public List<OctopusCertificate?>? Items { get; set; }
    }

    public class OctopusCertificate 
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
