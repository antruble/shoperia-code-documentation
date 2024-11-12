namespace ShoperiaDocumentation.Models.ViewModels
{
    public class EntityViewModel
    {
        // Az entitás azonosítója
        public int Id { get; set; }

        // Az entitás neve
        public string Name { get; set; } = string.Empty;

        // Az entitás leírása (opcionális)
        public string? Description { get; set; }

        // Az entitáshoz tartozó mezők listája
        public List<FieldViewModel> Fields { get; set; } = new();

        // Az entitás státusza (új vagy módosított)
        public string Status { get; set; } = "new";
    }

    public class FieldViewModel
    {
        // A mező azonosítója
        public int Id { get; set; }

        // A mező neve
        public string Name { get; set; } = string.Empty;

        // A mező típusa (pl. string, int)
        public string Type { get; set; } = string.Empty;

        // A mező leírása
        public string? Description { get; set; }

        // Az alapértelmezett érték
        public string? DefaultValue { get; set; }

        // Null érték megengedett-e
        public bool IsNullable { get; set; }

        // Elsődleges kulcs-e
        public bool IsPrimaryKey { get; set; }

        // Külső kulcs-e
        public bool IsForeignKey { get; set; }

        // Külső kulcshoz tartozó tábla neve
        public string? ForeignTable { get; set; }

        // Komment a mezőhöz
        public string? Comment { get; set; }
    }
}
