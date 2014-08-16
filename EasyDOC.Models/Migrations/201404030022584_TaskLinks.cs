namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskLink",
                c => new
                    {
                        SourceId = c.Int(nullable: false),
                        TargetId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        ProjectId = c.Int(nullable: false),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Name = c.String(),
                        Id = c.Int(nullable: false),
                        SourceTask_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.SourceId, t.TargetId, t.Type })
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Project", t => t.ProjectId)
                .ForeignKey("dbo.Task", t => t.SourceTask_Id)
                .ForeignKey("dbo.Task", t => t.TargetId)
                .Index(t => t.TargetId)
                .Index(t => t.ProjectId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId)
                .Index(t => t.SourceTask_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "ProjectId" });
            DropIndex("dbo.TaskLink", new[] { "TargetId" });
            DropTable("dbo.TaskLink");
        }
    }
}
