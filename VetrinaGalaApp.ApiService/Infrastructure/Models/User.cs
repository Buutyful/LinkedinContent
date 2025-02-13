using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Domain.UserDomain;

namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public class User : IdentityUser<Guid>
{
    public UserType UserType { get; set; }

    // Relationships
    public Store? Store { get; set; }
    public List<Swipe> Swipes { get; set; } = new();
}
