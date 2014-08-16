namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskEndDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Task", "EndDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Task", "Duration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Task", "Duration", c => c.Int(nullable: false));
            DropColumn("dbo.Task", "EndDate");
        }
    }
}
