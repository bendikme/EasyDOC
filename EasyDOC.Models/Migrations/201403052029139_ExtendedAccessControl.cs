namespace EasyDOC.Models.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ExtendedAccessControl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RolePermission", "Action", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermission", "Scope", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.RolePermission");
            AddPrimaryKey("dbo.RolePermission", new[] { "RoleId", "PermissionId", "Action" });
            DropColumn("dbo.RolePermission", "View");
            DropColumn("dbo.RolePermission", "Create");
            DropColumn("dbo.RolePermission", "Modify");
            DropColumn("dbo.RolePermission", "Delete");
            DropColumn("dbo.RolePermission", "Own");
            DropColumn("dbo.RolePermission", "Delegated");
            DropColumn("dbo.RolePermission", "All");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RolePermission", "All", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "Delegated", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "Own", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "Delete", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "Modify", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "Create", c => c.Boolean(nullable: false));
            AddColumn("dbo.RolePermission", "View", c => c.Boolean(nullable: false));
            DropPrimaryKey("dbo.RolePermission");
            AddPrimaryKey("dbo.RolePermission", new[] { "RoleId", "PermissionId" });
            DropColumn("dbo.RolePermission", "Scope");
            DropColumn("dbo.RolePermission", "Action");
        }
    }
}
