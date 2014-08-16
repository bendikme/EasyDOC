namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskWBS : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Task", "WbsCode", c => c.String());
            DropColumn("dbo.Task", "SortOrder");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Task", "SortOrder", c => c.Int(nullable: false));
            DropColumn("dbo.Task", "WbsCode");
        }
    }
}
