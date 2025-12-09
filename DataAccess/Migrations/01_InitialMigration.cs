using System.Data;
using FluentMigrator;

namespace DataAccess.Migrations;

[Migration(1)]
public class InitialMigration : Migration {
    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("FirstName").AsString(50).NotNullable()
            .WithColumn("LastName").AsString(50).NotNullable()
            .WithColumn("Email").AsString(100).NotNullable().Unique()
            .WithColumn("Password").AsString(255).NotNullable();
        
        Create.Table("Teams")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(100).NotNullable()
            .WithColumn("Description").AsString(500).Nullable();

        Create.Table("TeamUsers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Role").AsInt32().NotNullable()
            .WithColumn("TeamId").AsInt32().NotNullable()
                .ForeignKey("FK_TeamUsers_Teams", "Teams", "Id")
                .OnDelete(Rule.Cascade)
            .WithColumn("UserId").AsInt32().NotNullable()
                .ForeignKey("FK_TeamUsers_Users", "Users", "Id")
                .OnDelete(Rule.Cascade);
        
        Create.Table("Invitations")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Email").AsString(100).NotNullable()
            .WithColumn("Created").AsDateTime().NotNullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("TeamId").AsInt32().NotNullable()
                .ForeignKey("FK_Invitations_Teams", "Teams", "Id")
                .OnDelete(Rule.Cascade)
            .WithColumn("SenderId").AsInt32().NotNullable()
                .ForeignKey("FK_Invitations_Sender", "Users", "Id")
                .OnDelete(Rule.None)
            .WithColumn("InvitedUserId").AsInt32().Nullable()
                .ForeignKey("FK_Invitations_InvitedUser", "Users", "Id")
                .OnDelete(Rule.SetNull);
        
        Create.Table("TaskItems")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Title").AsString(200).NotNullable()
            .WithColumn("Description").AsString(2000).Nullable()
            .WithColumn("Status").AsInt32().NotNullable()
            .WithColumn("Priority").AsInt32().NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            .WithColumn("UpdatedAt").AsDateTime().Nullable()
            .WithColumn("DueDate").AsDateTime().Nullable()
            
            .WithColumn("TeamId").AsInt32().NotNullable()
                .ForeignKey("FK_TaskItems_Teams", "Teams", "Id")
                .OnDelete(Rule.Cascade)
            
            .WithColumn("CreatedByUserId").AsInt32().NotNullable()
                .ForeignKey("FK_TaskItems_Creator", "Users", "Id")
                .OnDelete(Rule.None)
            
            .WithColumn("AssignedToUserId").AsInt32().Nullable()
                .ForeignKey("FK_TaskItems_Assignee", "Users", "Id")
                .OnDelete(Rule.SetNull);
        
        Create.Table("Comments")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Text").AsString(1000).NotNullable()
            .WithColumn("CreatedAt").AsDateTime().NotNullable()
            
            .WithColumn("TaskId").AsInt32().NotNullable()
                .ForeignKey("FK_Comments_Tasks", "TaskItems", "Id")
                .OnDelete(Rule.Cascade)
            
            .WithColumn("CreatedByUserId").AsInt32().NotNullable()
                .ForeignKey("FK_Comments_Users", "Users", "Id")
                .OnDelete(Rule.None);
    }

    public override void Down()
    {
        Delete.Table("Comments");
        Delete.Table("TaskItems");
        Delete.Table("Invitations");
        Delete.Table("TeamUsers");
        Delete.Table("Teams");
        Delete.Table("Users");
    }
}