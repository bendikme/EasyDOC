namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GanttTaskTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Task", "Duration", c => c.Int(nullable: false));
            AddColumn("dbo.Task", "SortOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Task", "Progress", c => c.Single(nullable: false));
            DropColumn("dbo.Task", "EndDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Task", "EndDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Task", "Progress");
            DropColumn("dbo.Task", "SortOrder");
            DropColumn("dbo.Task", "Duration");
        }
    }
}
