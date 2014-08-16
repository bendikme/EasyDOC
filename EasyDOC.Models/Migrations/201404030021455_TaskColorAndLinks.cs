namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskColorAndLinks : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskLink", "CreatorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "EditorId", "dbo.User");
            DropForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project");
            DropForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropIndex("dbo.TaskLink", new[] { "ProjectId" });
            DropIndex("dbo.TaskLink", new[] { "CreatorId" });
            DropIndex("dbo.TaskLink", new[] { "EditorId" });
            DropIndex("dbo.TaskLink", new[] { "SourceTask_Id" });
            DropIndex("dbo.TaskLink", new[] { "TargetTask_Id" });
            DropPrimaryKey("dbo.Task");
            AddColumn("dbo.Task", "Color", c => c.String(maxLength: 7));
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
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
                        ProjectId = c.Int(nullable: false),
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
            
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropPrimaryKey("dbo.Task");
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false));
            DropColumn("dbo.Task", "Color");
            AddPrimaryKey("dbo.Task", "Id");
            CreateIndex("dbo.TaskLink", "TargetTask_Id");
            CreateIndex("dbo.TaskLink", "SourceTask_Id");
            CreateIndex("dbo.TaskLink", "EditorId");
            CreateIndex("dbo.TaskLink", "CreatorId");
            CreateIndex("dbo.TaskLink", "ProjectId");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "TargetTask_Id", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceTask_Id", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "ProjectId", "dbo.Project", "Id");
            AddForeignKey("dbo.TaskLink", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.TaskLink", "CreatorId", "dbo.User", "Id");
        }
    }
}
