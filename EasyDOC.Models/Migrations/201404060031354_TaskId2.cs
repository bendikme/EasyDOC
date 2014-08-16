namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskId2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskLink", "SourceId", "dbo.Task");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropPrimaryKey("dbo.Task");
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "TargetId", "dbo.Task", "Id");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "TargetId", "dbo.Task");
            DropForeignKey("dbo.Task", "ParentTaskId", "dbo.Task");
            DropForeignKey("dbo.TaskLink", "SourceId", "dbo.Task");
            DropPrimaryKey("dbo.Task");
            AlterColumn("dbo.Task", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Task", "Id");
            AddForeignKey("dbo.EmployeeTask", "TaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "TargetId", "dbo.Task", "Id");
            AddForeignKey("dbo.Task", "ParentTaskId", "dbo.Task", "Id");
            AddForeignKey("dbo.TaskLink", "SourceId", "dbo.Task", "Id");
        }
    }
}
