namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewTaskModel3 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.TaskLink", name: "FromId", newName: "__mig_tmp__0");
            RenameColumn(table: "dbo.TaskLink", name: "ToId", newName: "FromId");
            RenameColumn(table: "dbo.TaskLink", name: "__mig_tmp__0", newName: "ToId");
            RenameIndex(table: "dbo.TaskLink", name: "IX_ToId", newName: "__mig_tmp__0");
            RenameIndex(table: "dbo.TaskLink", name: "IX_FromId", newName: "IX_ToId");
            RenameIndex(table: "dbo.TaskLink", name: "__mig_tmp__0", newName: "IX_FromId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.TaskLink", name: "IX_FromId", newName: "__mig_tmp__0");
            RenameIndex(table: "dbo.TaskLink", name: "IX_ToId", newName: "IX_FromId");
            RenameIndex(table: "dbo.TaskLink", name: "__mig_tmp__0", newName: "IX_ToId");
            RenameColumn(table: "dbo.TaskLink", name: "ToId", newName: "__mig_tmp__0");
            RenameColumn(table: "dbo.TaskLink", name: "FromId", newName: "ToId");
            RenameColumn(table: "dbo.TaskLink", name: "__mig_tmp__0", newName: "FromId");
        }
    }
}
