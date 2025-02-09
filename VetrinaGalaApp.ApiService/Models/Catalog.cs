namespace VetrinaGalaApp.ApiService.Domain;

public enum CatalogType
{
    Scarpe,
    Pantaloni,
    Giacche
}

public class Catalog
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public CatalogType Type { get; set; }
    public List<Item> Items { get; set; } = new();
}
