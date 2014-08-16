namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExtendedAccessControl2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RolePermission", "Create", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermission", "Read", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermission", "Update", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermission", "Delete", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.RolePermission");
            AddPrimaryKey("dbo.RolePermission", new[] { "RoleId", "PermissionId" });
            DropColumn("dbo.RolePermission", "Action");
            DropColumn("dbo.RolePermission", "Scope");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RolePermission", "Scope", c => c.Int(nullable: false));
            AddColumn("dbo.RolePermission", "Action", c => c.Int(nullable: false));
            DropPrimaryKey("dbo.RolePermission");
            AddPrimaryKey("dbo.RolePermission", new[] { "RoleId", "PermissionId", "Action" });
            DropColumn("dbo.RolePermission", "Delete");
            DropColumn("dbo.RolePermission", "Update");
            DropColumn("dbo.RolePermission", "Read");
            DropColumn("dbo.RolePermission", "Create");
        }
    }
}
