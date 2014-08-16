namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MasterProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Project", "MasterProjectId", c => c.Int());
            CreateIndex("dbo.Project", "MasterProjectId");
            AddForeignKey("dbo.Project", "MasterProjectId", "dbo.Project", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Project", "MasterProjectId", "dbo.Project");
            DropIndex("dbo.Project", new[] { "MasterProjectId" });
            DropColumn("dbo.Project", "MasterProjectId");
        }
    }
}
