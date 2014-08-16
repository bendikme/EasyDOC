namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewTaskModel2 : DbMigration
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
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        Color = c.String(maxLength: 7),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        ConstraintDate = c.DateTime(nullable: false),
                        Duration = c.String(),
                        ConstraintType = c.Int(nullable: false),
                        CalendarType = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        PercentComplete = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        IsAutoScheduled = c.Boolean(nullable: false),
                        IsInactive = c.Boolean(nullable: false),
                        IsMilestone = c.Boolean(nullable: false),
                        ParentTaskId = c.Int(),
                        ProjectId = c.Int(),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Task", t => t.ParentTaskId)
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Project", t => t.ProjectId)
                .Index(t => t.ParentTaskId)
                .Index(t => t.ProjectId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId);
            
            CreateTable(
                "dbo.TaskLink",
                c => new
                    {
                        FromId = c.Int(nullable: false),
                        ToId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Lag = c.String(),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => new { t.FromId, t.ToId, t.Type })
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .ForeignKey("dbo.Task", t => t.FromId)
                .ForeignKey("dbo.Task", t => t.ToId)
                .Index(t => t.FromId)
                .Index(t => t.ToId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "ToId", "dbo.Task");
            DropForeignKey("dbo.Task", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.TaskLink", "FromId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.Task", "EditorId", "dbo.User");
            DropForeignKey("dbo.Task", "CreatorId", "dbo.User");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.EmployeeTask", "EmployeeId", "dbo.Employee");
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "ToId" });
            DropIndex("dbo.TaskLink", new[] { "FromId" });
            DropIndex("dbo.Task", new[] { "EditorId" });
            DropIndex("dbo.Task", new[] { "CreatorId" });
            DropIndex("dbo.Task", new[] { "ProjectId" });
            DropIndex("dbo.Task", new[] { "ParentTaskId" });
            DropIndex("dbo.EmployeeTask", new[] { "TaskId" });
            DropIndex("dbo.EmployeeTask", new[] { "EmployeeId" });
            DropTable("dbo.TaskLink");
            DropTable("dbo.Task");
            DropTable("dbo.EmployeeTask");
        }
    }
}
