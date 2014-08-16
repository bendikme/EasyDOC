namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinkFixDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropIndex("dbo.TaskLink", new[] { "TargetId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropTable("dbo.TaskLink");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TaskLink",
                c => new
                    {
                        SourceId = c.Int(nullable: false),
                        TargetId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Name = c.String(),
                        Id = c.Int(nullable: false),
                        SourceTask_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.SourceId, t.TargetId, t.Type });
            
            CreateIndex("dbo.TaskLink", "SourceTask_Id");
            CreateIndex("dbo.TaskLink", "EditorId");
            CreateIndex("dbo.TaskLink", "CreatorId");
            CreateIndex("dbo.TaskLink", "TargetId");
            AddForeignKey("dbo.TaskLink", "TargetId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskLink", "CreatorId", "dbo.User", "Id");
        }
    }
}
