namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Logging : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DatabaseLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Content = c.String(maxLength: 2000),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.CreatorId)
                .ForeignKey("dbo.User", t => t.EditorId)
                .Index(t => t.CreatorId)
                .Index(t => t.EditorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DatabaseLog", "EditorId", "dbo.User");
            DropForeignKey("dbo.DatabaseLog", "CreatorId", "dbo.User");
            DropIndex("dbo.DatabaseLog", new[] { "EditorId" });
            DropIndex("dbo.DatabaseLog", new[] { "CreatorId" });
            DropTable("dbo.DatabaseLog");
        }
    }
}
