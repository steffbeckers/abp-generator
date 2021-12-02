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
            PermissionDefinition samplesPermission = myProductGroup.AddPermission(MyProductPermissions.Samples.Default, L($"{nameof(MyProductPermissions.Permission)}:{nameof(MyProductPermissions.Samples)}"));
            samplesPermission.AddChild(MyProductPermissions.Samples.Create, L($"{nameof(MyProductPermissions.Permission)}:{nameof(MyProductPermissions.Samples.Create)}"));
            samplesPermission.AddChild(MyProductPermissions.Samples.Update, L($"{nameof(MyProductPermissions.Permission)}:{nameof(MyProductPermissions.Samples.Update)}"));
            samplesPermission.AddChild(MyProductPermissions.Samples.Delete, L($"{nameof(MyProductPermissions.Permission)}:{nameof(MyProductPermissions.Samples.Delete)}"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<MyProductResource>(name);
        }
    }
}