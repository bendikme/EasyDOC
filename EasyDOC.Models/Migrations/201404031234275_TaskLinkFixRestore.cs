namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinkFixRestore : DbMigration
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
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => new { t.SourceId, t.TargetId, t.Type })
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Task", t => t.SourceId)
                .ForeignKey("dbo.Task", t => t.TargetId)
                .Index(t => t.SourceId)
                .Index(t => t.TargetId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "SourceId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "TargetId" });
            DropIndex("dbo.TaskLink", new[] { "SourceId" });
            DropTable("dbo.TaskLink");
        }
    }
}
