using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Serilog;
using TextCopy;
using Process = System.Diagnostics.Process;

namespace LunchBot;

internal class GraphServiceClientFactory
{
    private readonly HttpProvider _httpProvider;
    private readonly IAuthenticationData _authenticationData;
    private readonly IConfigurationRoot _configuration;
    private readonly ILogger _logger;

    public GraphServiceClientFactory(HttpProvider httpProvider, IAuthenticationData authenticationData,
        IConfigurationRoot configuration, ILogger logger)
    {
        _httpProvider = httpProvider;
        _authenticationData = authenticationData;
        _configuration = configuration;
        _logger = logger;
    }

    public GraphServiceClient Create()
    {
        string[] scopes = _configuration.GetSection("Scopes").Get<string[]>();

        // using Azure.Identity;
        TokenCredentialOptions options = new() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };

        DeviceCodeCredential deviceCodeCredential;

        if (string.IsNullOrEmpty(_authenticationData.TenantId) || string.IsNullOrEmpty(_authenticationData.ClientId))
        {
            throw new Exception("TenantId or ClientId isn't set");
        }
        
        try
        {
            // https://docs.microsoft.com/dotnet/api/azure.identity.devicecodecredential
            deviceCodeCredential = new DeviceCodeCredential(OnDeviceCode, _authenticationData.TenantId,
                _authenticationData.ClientId, options);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Device code exception");
            throw;
        }

        return new GraphServiceClient(deviceCodeCredential, scopes, _httpProvider);
    }

    private Task OnDeviceCode(DeviceCodeInfo codeInfo, CancellationToken _)
    {
        Console.WriteLine(codeInfo.Message);

        if (_configuration.GetValue<bool>("OpenBrowserOnAuthentication"))
        {
            OpenBrowser(codeInfo.UserCode, codeInfo.VerificationUri.OriginalString);
        }

        return Task.FromResult(0);
    }

    private static void OpenBrowser(string deviceCode, string url)
    {
        ClipboardService.SetText(deviceCode);

        Process process = new();
        process.StartInfo.FileName = url;
        process.StartInfo.UseShellExecute = true;
        process.Start();
    }
}