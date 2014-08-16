namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinkProject : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project");
            DropIndex("dbo.TaskLink", new[] { "ProjectId" });
            DropColumn("dbo.TaskLink", "ProjectId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskLink", "ProjectId", c => c.Int(nullable: false));
            CreateIndex("dbo.TaskLink", "ProjectId");
            AddForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project", "Id");
        }
    }
}
