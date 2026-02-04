using FluentMigrator;

namespace DataAccess.Migrations;
[Migration(4)]
public class AddingModelTelegramLinkCodes : Migration {
    public override void Up()
    {
        Create.Table("TelegramLinkCodes")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("UserId").AsInt32().NotNullable()
                .ForeignKey("FK_TelegramLinkCodes_Users", "Users", "Id")
            .WithColumn("Code").AsString(100).NotNullable()
            .WithColumn("ExpiresAt").AsDateTime().NotNullable()
            .WithColumn("IsUsed").AsBoolean().NotNullable();
        
        Create.Index("IX_TelegramLinkCodes_Code")
            .OnTable("TelegramLinkCodes")
            .OnColumn("Code")
            .Unique();
    }

    public override void Down()
    {
        Delete.Table("TelegramLinkCodes");
    }
}