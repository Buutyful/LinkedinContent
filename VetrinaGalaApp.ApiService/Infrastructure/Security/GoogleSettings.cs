namespace VetrinaGalaApp.ApiService.Infrastructure.Security;

public class GoogleSettings
{
    public string Section => "Google";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string UserInfoEndpoint { get; set; } = string.Empty;
}
