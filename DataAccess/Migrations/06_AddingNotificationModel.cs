using FluentMigrator;

namespace DataAccess.Migrations;
[Migration(6)]
public class AddingNotificationModel : Migration {
    public override void Up()
    {
        Create.Table("Notifications")
            .WithColumn("Id").AsInt32().Identity().PrimaryKey()
            .WithColumn("Text").AsString().NotNullable()
            .WithColumn("FromUserId").AsInt32().Nullable()
            .ForeignKey("FK_Notifications_FromUser_Users", "Users", "Id")
            .WithColumn("ToUserId").AsInt32().NotNullable()
            .ForeignKey("FK_Notifications_ToUser_Users", "Users", "Id")
            .WithColumn("TaskId").AsInt32().Nullable()
            .ForeignKey("FK_Notifications_Task_Tasks", "TaskItems", "Id")
            .WithColumn("Type").AsInt32().NotNullable()
            .WithColumn("IsRead").AsBoolean().WithDefaultValue(false).NotNullable()
            .WithColumn("SentAt").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("Notifications");
    }
}