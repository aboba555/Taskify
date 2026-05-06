using FluentMigrator;

namespace DataAccess.Migrations;
[Migration(7)]
public class AddingEmailVerificationAndIsActiveToUser : Migration {
    public override void Up()
    {
        Alter.Table("Users").AddColumn("IsActivated").AsBoolean().WithDefaultValue(false);
        
        Create.Table("EmailVerifications")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("UserId").AsInt32().NotNullable()
            .ForeignKey("FK_EmailVerifications_UserId_Users", "Users", "Id")
            .WithColumn("Code").AsString(6).NotNullable()
            .WithColumn("SentAt").AsDateTime().NotNullable()
            .WithColumn("ExpiresAt").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Column("IsActivated").FromTable("Users");
        Delete.Table("EmailVerifications");
    }
}