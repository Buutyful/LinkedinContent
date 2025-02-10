namespace VetrinaGalaApp.ApiService.Infrastructure.Models;

public enum CatalogType
{
    Scarpe,
    Pantaloni,
    Giacche,
    Altro
}

public class Catalog
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public CatalogType Type { get; set; }

    //Navigations
    public List<Item> Items { get; set; } = new();
}
