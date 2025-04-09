namespace VetrinaGalaApp.ApiService.Infrastructure.Security;

public class GoogleSettings
{
    public static string Section => "Google";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
