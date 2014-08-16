namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AbstractComponent", "TypeId", "dbo.Type");
            DropForeignKey("dbo.Type", "CreatorId", "dbo.User");
            DropForeignKey("dbo.Type", "EditorId", "dbo.User");
            DropIndex("dbo.AbstractComponent", new[] { "TypeId" });
            DropIndex("dbo.Type", new[] { "CreatorId" });
            DropIndex("dbo.Type", new[] { "EditorId" });
            DropColumn("dbo.AbstractComponent", "TypeId");
            DropTable("dbo.Type");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Type",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        NameSe = c.String(),
                        Timestamp = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Created = c.DateTime(nullable: false),
                        Edited = c.DateTime(nullable: false),
                        CreatorId = c.Int(),
                        EditorId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AbstractComponent", "TypeId", c => c.Int());
            CreateIndex("dbo.Type", "EditorId");
            CreateIndex("dbo.Type", "CreatorId");
            CreateIndex("dbo.AbstractComponent", "TypeId");
            AddForeignKey("dbo.Type", "EditorId", "dbo.User", "Id");
            AddForeignKey("dbo.Type", "CreatorId", "dbo.User", "Id");
            AddForeignKey("dbo.AbstractComponent", "TypeId", "dbo.Type", "Id");
        }
    }
}
