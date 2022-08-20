namespace LunchBot;

internal interface IAuthenticationData
{
    string TenantId { get; }
    string ClientId { get; }
}