namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DatabaseLogFreeLengthContent : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.DatabaseLog", "Content", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.DatabaseLog", "Content", c => c.String(maxLength: 2000));
        }
    }
}
