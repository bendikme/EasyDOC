namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskLinksAndCustomId : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskLink",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        SourceId = c.Int(nullable: false),
                        TargetId = c.Int(nullable: false),
                        Type = c.String(maxLength: 20),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Name = c.String(),
                        SourceTask_Id = c.Int(),
                        TargetTask_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Task", t => t.SourceTask_Id)
                .ForeignKey("dbo.Task", t => t.TargetTask_Id)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId)
                .Index(t => t.SourceTask_Id)
                .Index(t => t.TargetTask_Id);
            
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropIndex("dbo.TaskLink", new[] { "TargetTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false, identity: true));
            DropTable("dbo.TaskLink");
        }
    }
}
