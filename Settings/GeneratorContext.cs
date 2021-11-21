namespace SteffBeckers.Abp.Generator.Settings
{
    public class AggregateRoot
    {
        public IList<Entity> Entities { get; set; } = new List<Entity>();

        public string Name { get; set; } = string.Empty;

        public string NamePlural { get; set; } = string.Empty;

        public IList<Property> Properties { get; set; } = new List<Property>();
    }

    public class Entity
    {
        public string Name { get; set; } = string.Empty;

        public string NamePlural { get; set; } = string.Empty;

        public IList<Property> Properties { get; set; } = new List<Property>();
    }

    public class GeneratorContext
    {
        public AggregateRoot AggregateRoot { get; set; } = new AggregateRoot();

        public Project Project { get; set; } = new Project();
    }

    public class Project
    {
        public string CompanyName
        {
            get
            {
                return Name.Split(".").First();
            }
        }

        public string Name { get; set; } = string.Empty;

        public string? ProductName
        {
            get
            {
                return Name.Contains(".") ? Name.Split(".").Last() : null;
            }
        }
    }

    public class Property
    {
        public int? MaxLength { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Required { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}