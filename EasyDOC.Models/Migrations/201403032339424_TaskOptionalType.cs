namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskOptionalType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Task", "TypeId", "dbo.TaskType");
            DropIndex("dbo.Task", new[] { "TypeId" });
            AlterColumn("dbo.Task", "TypeId", c => c.Int());
            CreateIndex("dbo.Task", "TypeId");
            AddForeignKey("dbo.Task", "TypeId", "dbo.TaskType", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Task", "TypeId", "dbo.TaskType");
            DropIndex("dbo.Task", new[] { "TypeId" });
            AlterColumn("dbo.Task", "TypeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Task", "TypeId");
            AddForeignKey("dbo.Task", "TypeId", "dbo.TaskType", "Id");
        }
    }
}
