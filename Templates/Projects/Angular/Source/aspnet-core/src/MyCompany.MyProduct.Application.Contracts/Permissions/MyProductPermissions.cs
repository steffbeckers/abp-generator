namespace MyCompany.MyProduct.Permissions
{
    public static class MyProductPermissions
    {
        public const string GroupName = nameof(MyProduct);
        public const string Permission = nameof(Permission);

        // Add your own permission names. Example:
        // public const string MyPermission1 = GroupName + ".MyPermission1";
        public class Samples
        {
            public const string Create = Default + $".{nameof(Create)}";
            public const string Default = GroupName + $".{nameof(Samples)}";
            public const string Delete = Default + $".{nameof(Delete)}";
            public const string Update = Default + $".{nameof(Update)}";
        }
    }
}