namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskTypeEnums : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskType", "CreatorId", "dbo.User");
            DropForeignKey("dbo.TaskType", "EditorId", "dbo.User");
            DropForeignKey("dbo.Task", "TypeId", "dbo.TaskType");
            DropIndex("dbo.TaskType", new[] { "CreatorId" });
            DropIndex("dbo.TaskType", new[] { "EditorId" });
            DropIndex("dbo.Task", new[] { "TypeId" });
            AddColumn("dbo.Task", "Type", c => c.Int(nullable: false));
            DropColumn("dbo.Task", "TypeId");
            DropTable("dbo.TaskType");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TaskType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Task", "TypeId", c => c.Int());
            DropColumn("dbo.Task", "Type");
            CreateIndex("dbo.Task", "TypeId");
            CreateIndex("dbo.TaskType", "EditorId");
            CreateIndex("dbo.TaskType", "CreatorId");
            AddForeignKey("dbo.Task", "TypeId", "dbo.TaskType", "Id");
            AddForeignKey("dbo.TaskType", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskType", "CreatorId", "dbo.User", "Id");
        }
    }
}
