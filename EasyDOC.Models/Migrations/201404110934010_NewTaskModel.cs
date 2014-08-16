namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewTaskModel : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee");
            DropForeignKey("dbo.Task", "CreatorId", "dbo.User");
            DropForeignKey("dbo.Task", "EditorId", "dbo.User");
            DropForeignKey("dbo.Task", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "SourceId", "dbo.Task");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropIndex("dbo.EmployeeTask", new[] { "EmployeeId" });
            DropIndex("dbo.EmployeeTask", new[] { "TaskId" });
            DropIndex("dbo.Task", new[] { "ParentTaskId" });
            DropIndex("dbo.Task", new[] { "ProjectId" });
            DropIndex("dbo.Task", new[] { "CreatorId" });
            DropIndex("dbo.Task", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "SourceId" });
            DropIndex("dbo.TaskLink", new[] { "TargetId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
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
                .PrimaryKey(t => new { t.SourceId, t.TargetId, t.Type });
            
            CreateTable(
                "dbo.Task",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Color = c.String(maxLength: 7),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Progress = c.Single(nullable: false),
                        Type = c.Int(nullable: false),
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
            
            CreateIndex("dbo.TaskLink", "EditorId");
            CreateIndex("dbo.TaskLink", "CreatorId");
            CreateIndex("dbo.TaskLink", "TargetId");
            CreateIndex("dbo.TaskLink", "SourceId");
            CreateIndex("dbo.Task", "EditorId");
            CreateIndex("dbo.Task", "CreatorId");
            CreateIndex("dbo.Task", "ProjectId");
            CreateIndex("dbo.Task", "ParentTaskId");
            CreateIndex("dbo.EmployeeTask", "TaskId");
            CreateIndex("dbo.EmployeeTask", "EmployeeId");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "TargetId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskLink", "CreatorId", "dbo.User", "Id");
            AddForeignKey("dbo.Task", "ProjectId", "dbo.Project", "Id");
            AddForeignKey("dbo.Task", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.Task", "CreatorId", "dbo.User", "Id");
            AddForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee", "Id");
        }
    }
}
