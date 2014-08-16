namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskCheckPoint : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TaskCheckPoint",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Info = c.String(),
                        Completed = c.Boolean(nullable: false),
                        Deadline = c.DateTime(nullable: false),
                        EmployeeId = c.Int(),
                        TaskId = c.Int(nullable: false),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Task", t => t.TaskId)
                .ForeignKey("dbo.Employee", t => t.EmployeeId)
                .Index(t => t.EmployeeId)
                .Index(t => t.TaskId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskCheckPoint", "EmployeeId", "dbo.Employee");
            DropForeignKey("dbo.TaskCheckPoint", "TaskId", "dbo.Task");
            DropForeignKey("dbo.TaskCheckPoint", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskCheckPoint", "CreatorId", "dbo.User");
            DropIndex("dbo.TaskCheckPoint", new[] { "EditorId" });
            DropIndex("dbo.TaskCheckPoint", new[] { "CreatorId" });
            DropIndex("dbo.TaskCheckPoint", new[] { "TaskId" });
            DropIndex("dbo.TaskCheckPoint", new[] { "EmployeeId" });
            DropTable("dbo.TaskCheckPoint");
        }
    }
}
