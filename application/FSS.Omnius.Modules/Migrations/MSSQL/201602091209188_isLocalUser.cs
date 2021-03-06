namespace FSS.Omnius.Modules.Migrations.MSSQL
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isLocalUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Persona_Users", "isLocalUser", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Persona_Users", "isLocalUser");
        }
    }
}
