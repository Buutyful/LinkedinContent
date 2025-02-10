using Microsoft.AspNetCore.Identity;

namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public enum UserType
{
    User,
    StoreOwner
}

public class User : IdentityUser<Guid>
{
    public UserType UserType { get; set; }

    // Relationships
    public Store? Store { get; set; }
    public List<Swipe> Swipes { get; set; } = new();
}
