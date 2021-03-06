using Newtonsoft.Json;

namespace SteffBeckers.Abp.Generator.Settings
{
    public class AggregateRoot
    {
        public IList<Entity> Entities { get; set; } = new List<Entity>();

        public string LookupPropertyName { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string NamePlural { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Property> OptionalProperties => Properties.Where(x => x.Optional).ToList();

        public string OrderByPropertyName { get; set; } = string.Empty;

        public IList<Property> Properties { get; set; } = new List<Property>();

        [JsonIgnore]
        public List<Property> PropertiesOrderByName => Properties.OrderBy(x => x.Name).ToList();

        [JsonIgnore]
        public List<Property> RequiredProperties => Properties.Where(x => x.Required).ToList();
    }

    public class Entity
    {
        public string AggregateRootPropertyName { get; set; } = string.Empty;

        public string AggregateRootPropertyNamePlural { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string NamePlural { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Property> OptionalProperties => Properties.Where(x => x.Optional).ToList();

        public IList<Property> Properties { get; set; } = new List<Property>();

        [JsonIgnore]
        public List<Property> PropertiesOrderByName => Properties.OrderBy(x => x.Name).ToList();

        [JsonIgnore]
        public List<Property> RequiredProperties => Properties.Where(x => x.Required).ToList();
    }

    public class GeneratorContext
    {
        public AggregateRoot AggregateRoot { get; set; } = new AggregateRoot();

        [JsonIgnore]
        public Entity Entity { get; set; } = new Entity();

        public Project Project { get; set; } = new Project();
    }

    public class Project
    {
        [JsonIgnore]
        public string? CompanyName => Name.Contains('.') ? Name.Split('.').FirstOrDefault() : null;

        public bool IsModule { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string? ProductName => Name.Contains('.') ? Name.Split('.').LastOrDefault() : null;
    }

    public class Property
    {
        [JsonIgnore]
        public bool IsGuid => Type.Equals("Guid");

        [JsonIgnore]
        public bool IsString => Type.Equals("string");

        public int? MaxLength { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public bool Optional => !Required;

        public bool Required { get; set; }

        [JsonIgnore]
        public bool StringAsFullProperty => IsString && (Required || MaxLength != null);

        public string Type { get; set; } = string.Empty;
    }
}