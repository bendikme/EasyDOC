namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OverwritableFile : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.File", "IsOverwritable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.File", "IsOverwritable");
        }
    }
}
