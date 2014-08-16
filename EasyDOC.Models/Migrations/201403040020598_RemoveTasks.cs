namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTasks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee");
            DropForeignKey("dbo.Task", "CreatorId", "dbo.User");
            DropForeignKey("dbo.Task", "EditorId", "dbo.User");
            DropForeignKey("dbo.Task", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.Task", "TypeId", "dbo.TaskType");
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task");
            DropIndex("dbo.EmployeeTask", new[] { "EmployeeId" });
            DropIndex("dbo.Task", new[] { "CreatorId" });
            DropIndex("dbo.Task", new[] { "EditorId" });
            DropIndex("dbo.Task", new[] { "ProjectId" });
            DropIndex("dbo.Task", new[] { "ParentTaskId" });
            DropIndex("dbo.Task", new[] { "TypeId" });
            DropIndex("dbo.EmployeeTask", new[] { "TaskId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "TargetTask_Id" });
            DropTable("dbo.EmployeeTask");
            DropTable("dbo.Task");
            DropTable("dbo.TaskLink");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EmployeeTask",
                c => new
                    {
                        EmployeeId = c.Int(nullable: false),
                        TaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EmployeeId, t.TaskId });
            
            CreateIndex("dbo.TaskLink", "TargetTask_Id");
            CreateIndex("dbo.TaskLink", "SourceTask_Id");
            CreateIndex("dbo.TaskLink", "EditorId");
            CreateIndex("dbo.TaskLink", "CreatorId");
            CreateIndex("dbo.EmployeeTask", "TaskId");
            CreateIndex("dbo.Task", "TypeId");
            CreateIndex("dbo.Task", "ParentTaskId");
            CreateIndex("dbo.Task", "ProjectId");
            CreateIndex("dbo.Task", "EditorId");
            CreateIndex("dbo.Task", "CreatorId");
            CreateIndex("dbo.EmployeeTask", "EmployeeId");
            AddForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskLink", "CreatorId", "dbo.User", "Id");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "TypeId", "dbo.TaskType", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ProjectId", "dbo.Project", "Id");
            AddForeignKey("dbo.Task", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.Task", "CreatorId", "dbo.User", "Id");
            AddForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee", "Id");
        }
    }
}
