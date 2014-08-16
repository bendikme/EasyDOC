namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTasks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmployeeTask",
                c => new
                    {
                        EmployeeId = c.Int(nullable: false),
                        TaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EmployeeId, t.TaskId })
                .ForeignKey("dbo.Employee", t => t.EmployeeId)
                .ForeignKey("dbo.Task", t => t.TaskId)
                .Index(t => t.EmployeeId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.Task",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        StartDate = c.DateTime(nullable: false),
                        Duration = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Progress = c.Single(nullable: false),
                        TypeId = c.Int(),
                        ParentTaskId = c.Int(),
                        ProjectId = c.Int(),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Project", t => t.ProjectId)
                .ForeignKey("dbo.Task", t => t.ParentTaskId)
                .ForeignKey("dbo.TaskType", t => t.TypeId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId)
                .Index(t => t.ProjectId)
                .Index(t => t.ParentTaskId)
                .Index(t => t.TypeId);
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropForeignKey("dbo.Task", "TypeId", "dbo.TaskType");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.Task", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.Task", "EditorId", "dbo.User");
            DropForeignKey("dbo.Task", "CreatorId", "dbo.User");
            DropForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee");
            DropIndex("dbo.TaskLink", new[] { "TargetTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.EmployeeTask", new[] { "TaskId" });
            DropIndex("dbo.Task", new[] { "TypeId" });
            DropIndex("dbo.Task", new[] { "ParentTaskId" });
            DropIndex("dbo.Task", new[] { "ProjectId" });
            DropIndex("dbo.Task", new[] { "EditorId" });
            DropIndex("dbo.Task", new[] { "CreatorId" });
            DropIndex("dbo.EmployeeTask", new[] { "EmployeeId" });
            DropTable("dbo.TaskLink");
            DropTable("dbo.Task");
            DropTable("dbo.EmployeeTask");
        }
    }
}
