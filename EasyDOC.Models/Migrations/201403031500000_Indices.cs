namespace EasyDOC.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indices : DbMigration
    {
        public override void Up()
        {
            CreateIndex("Project", "ProjectNumber", true, "IX_Project_ProjectNumber");
            CreateIndex("Customer", "Name", true, "IX_Customer_Name");
            CreateIndex("Maintenance", "Name", true, "IX_Maintenance_Name");
            CreateIndex("Safety", "Name", true, "IX_Safety_Name");
            CreateIndex("Vendor", "Name", true, "IX_Vendor_Name");
            CreateIndex("OrderConfirmation", new[] { "OrderNumber", "VendorId" }, true, "IX_OrderConfirmation_OrderNumber_VendorId");
            CreateIndex("AbstractComponent", new[] { "Name", "VendorId" }, true, "IX_AbstractComponent_Name_VendorId");
            CreateIndex("File", "Name", true, "IX_File_Name");
            CreateIndex("User", "Username", true, "IX_User_Username");
            CreateIndex("User", "Email", true, "IX_User_Email");
        }
        
        public override void Down()
        {
            DropIndex("Project", "IX_Project_ProjectNumber");
            DropIndex("Customer", "IX_Customer_Name");
            DropIndex("Maintenance", "IX_Maintenance_Name");
            DropIndex("Safety", "IX_Safety_Name");
            DropIndex("Vendor", "IX_Vendor_Name");
            DropIndex("OrderConfirmation", "IX_OrderConfirmation_OrderNumber_VendorId");
            DropIndex("AbstractComponent", "IX_AbstractComponent_Name_VendorId");
            DropIndex("File", "IX_File_Name");
            DropIndex("User", "IX_User_Username");
            DropIndex("User", "IX_User_Email");
        }
    }
}
