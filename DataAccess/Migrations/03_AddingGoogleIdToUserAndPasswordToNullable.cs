using FluentMigrator;

namespace DataAccess.Migrations;

[Migration(3)]
public class AddingGoogleIdToUserAndPasswordToNullable : Migration {
    public override void Up()
    {
        Alter.Table("Users").AddColumn("GoogleId").AsString(255).Nullable();
        Alter.Table("Users").AlterColumn("Password").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Column("GoogleId").FromTable("Users");
        Alter.Table("Users").AlterColumn("Password").AsString(255).NotNullable();
    }
}