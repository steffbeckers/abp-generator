using MyCompany.MyProduct.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace MyCompany.MyProduct.Permissions
{
    public class MyProductPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            PermissionGroupDefinition myProductGroup = context.AddGroup(MyProductPermissions.GroupName);

            // Define your own permissions here. Example:
            // myProductGroup.AddPermission(MyProductPermissions.MyPermission1, L("Permission:MyPermission1"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<MyProductResource>(name);
        }
    }
}
