using FluentMigrator;

namespace DataAccess.Migrations;
[Migration(2)]
public class AddingLabelToTaskItem : Migration {
    public override void Up()
    {
        Alter.Table("TaskItems")
            .AddColumn("Label")
            .AsInt32()
            .NotNullable()
            .WithDefaultValue(0);
    }

    public override void Down()
    {
        Delete.Column("Label").FromTable("TaskItems");
    }
}