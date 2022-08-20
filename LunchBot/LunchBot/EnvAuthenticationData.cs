using Microsoft.Extensions.Configuration;

namespace LunchBot;

internal class EnvAuthenticationData : IAuthenticationData
{
    public string TenantId { get; }
    public string ClientId { get; }

    public EnvAuthenticationData(IConfigurationRoot configuration)
    {
        string tenantIdEvnKey = configuration.GetValue<string>("TenantIdEnvKey");
        string clientIdEnvKey = configuration.GetValue<string>("ClientIdEnvKey");

        TenantId = Environment.GetEnvironmentVariable(tenantIdEvnKey, EnvironmentVariableTarget.User);
        ClientId = Environment.GetEnvironmentVariable(clientIdEnvKey, EnvironmentVariableTarget.User);
    }
}