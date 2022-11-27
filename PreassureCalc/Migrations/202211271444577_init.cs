namespace PreassureCalc.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Wells",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        numberWell = c.String(),
                        depth = c.Single(nullable: false),
                        preassure = c.Single(nullable: false),
                        calculatedDepth = c.Single(nullable: false),
                        density = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Wells");
        }
    }
}
