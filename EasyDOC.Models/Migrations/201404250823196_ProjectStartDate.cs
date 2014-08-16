namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProjectStartDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Project", "StartDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Project", "StartDate");
        }
    }
}
