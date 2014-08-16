namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderConfirmationFileReference : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderConfirmation", "FileId", c => c.Int());
            CreateIndex("dbo.OrderConfirmation", "FileId");
            AddForeignKey("dbo.OrderConfirmation", "FileId", "dbo.File", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderConfirmation", "FileId", "dbo.File");
            DropIndex("dbo.OrderConfirmation", new[] { "FileId" });
            DropColumn("dbo.OrderConfirmation", "FileId");
        }
    }
}
