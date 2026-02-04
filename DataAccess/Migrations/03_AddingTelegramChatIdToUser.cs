using FluentMigrator;

namespace DataAccess.Migrations;
[Migration(3)]
public class AddingTelegramChatIdToUser : Migration{
    public override void Up()
    {
        Alter.Table("Users").AddColumn("TelegramChatId").AsString(255).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TelegramChatId").FromTable("Users");
    }
}