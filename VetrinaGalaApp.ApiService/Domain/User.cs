using Microsoft.AspNetCore.Identity;

namespace VetrinaGalaApp.ApiService.Domain;

public enum UserType
{
    User,
    StoreOwner
}

public class User : IdentityUser<Guid>
{
    public UserType UserType { get; set; }
    public Store? Store { get; set; }
}
