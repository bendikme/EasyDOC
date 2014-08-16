namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IdentityIds : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.TaskLink", "Id", c => c.Int(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TaskLink", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false));
        }
    }
}
