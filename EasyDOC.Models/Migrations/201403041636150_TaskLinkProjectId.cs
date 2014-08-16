namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinkProjectId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskLink", "ProjectId", c => c.Int(nullable: false));
            CreateIndex("dbo.TaskLink", "ProjectId");
            AddForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project");
            DropIndex("dbo.TaskLink", new[] { "ProjectId" });
            DropColumn("dbo.TaskLink", "ProjectId");
        }
    }
}
