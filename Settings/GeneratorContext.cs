namespace SteffBeckers.Abp.Generator.Settings
{
    public class AggregateRoot
    {
        public IList<Entity> Entities { get; set; } = new List<Entity>();

        public string Name { get; set; } = "Recipe";

        // TODO: Update with getter based on Name
        public string NamePlural { get; set; } = "Recipes";
    }

    public class Entity
    {
        public string Name { get; set; } = "RecipeStep";

        // TODO: Update with getter based on Name
        public string NamePlural { get; set; } = "RecipeSteps";
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

        public string Name { get; set; } = "MyCompany.MyProduct";

        public string? ProductName
        {
            get
            {
                return Name.Contains(".") ? Name.Split(".").Last() : null;
            }
        }
    }
}