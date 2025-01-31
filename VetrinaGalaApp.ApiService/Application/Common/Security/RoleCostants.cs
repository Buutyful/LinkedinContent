using System.Reflection;

namespace VetrinaGalaApp.ApiService.Application.Common.Security;

public class RoleConstants
{
    public const string Admin = "Admin";
    public const string StoreOwner = "StoreOwner";

    public IEnumerable<string> GetConstantRoles() => GetType()
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!);

}