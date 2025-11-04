using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace listly.Migrations
{
    /// <inheritdoc />
    public partial class MigracionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payment_mappings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preference_id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_reference = table.Column<string>(type: "varchar(500)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_mappings", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(50)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    photo_url = table.Column<string>(type: "varchar(500)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fcm_token = table.Column<string>(type: "varchar(500)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.uid);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lists",
                columns: table => new
                {
                    list_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "varchar(10)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lists", x => x.list_id);
                    table.ForeignKey(
                        name: "FK_lists_users_owner_uid",
                        column: x => x.owner_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserUid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceiveInvitationNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceiveItemAddedNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceiveItemStatusChangedNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReceiveItemDeletedNotifications = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_Settings_users_UserUid",
                        column: x => x.UserUid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                columns: table => new
                {
                    uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subscription_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subscriptions", x => new { x.uid, x.subscription_id });
                    table.ForeignKey(
                        name: "FK_UserSubscription_Subscription",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSubscription_User",
                        column: x => x.uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    invitation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    list_id = table.Column<int>(type: "int", nullable: false),
                    from_uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    to_uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.invitation_id);
                    table.ForeignKey(
                        name: "FK_invitations_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "lists",
                        principalColumn: "list_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invitations_users_from_uid",
                        column: x => x.from_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_users_to_uid",
                        column: x => x.to_uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    list_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    completed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    checked_by = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    unit = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_items_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "lists",
                        principalColumn: "list_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_items_users_checked_by",
                        column: x => x.checked_by,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "list_users",
                columns: table => new
                {
                    uid = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    list_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_list_users", x => new { x.uid, x.list_id });
                    table.ForeignKey(
                        name: "FK_list_users_lists_list_id",
                        column: x => x.list_id,
                        principalTable: "lists",
                        principalColumn: "list_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_list_users_users_uid",
                        column: x => x.uid,
                        principalTable: "users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "subscriptions",
                columns: new[] { "id", "description", "name", "price" },
                values: new object[,]
                {
                    { 1, "Plan gratuito con funciones básicas", "Free", 0.0m },
                    { 2, "Plan premium con todas las funciones", "Premium", 2.00m }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "uid", "display_name", "email", "fcm_token", "photo_url" },
                values: new object[,]
                {
                    { "57cV0RNcC9bnCKzbcNiBp1Tzr822", "Federico González", "alambratore@gmail.com", "", "https://lh3.googleusercontent.com/a/ACg8ocJPzlf1C9crnRGlPqM8B-86Xm68mJmqQ-2oGr7kW11g4apmgPg=s96-c" },
                    { "hQZnBgjuOwTTGIOVWgGHsE95OOX2", "Steam Secundario", "steamsecundario@gmail.com", "", "https://lh3.googleusercontent.com/a/ACg8ocK35d11bNqi6k1mqlcxakZ7QFmnPHbkbYs4KZ5k47XwxC8YuA=s96-c" },
                    { "IMImLkBokLWuwjRGy7Jm7wnBWAB3", "Fogón de Hugo", "fogondehugo@gmail.com", "", "https://lh3.googleusercontent.com/a/ACg8ocKdkpsOMhRxy-XTt2CyCGKNLs9sQLThXYsv4ZJpQYCWx-qMwg=s96-c" }
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "SettingId", "ReceiveInvitationNotifications", "ReceiveItemAddedNotifications", "ReceiveItemDeletedNotifications", "ReceiveItemStatusChangedNotifications", "UserUid" },
                values: new object[,]
                {
                    { 1, true, true, true, true, "57cV0RNcC9bnCKzbcNiBp1Tzr822" },
                    { 2, true, true, true, true, "hQZnBgjuOwTTGIOVWgGHsE95OOX2" },
                    { 3, true, true, true, true, "IMImLkBokLWuwjRGy7Jm7wnBWAB3" }
                });

            migrationBuilder.InsertData(
                table: "lists",
                columns: new[] { "list_id", "description", "icon", "owner_uid", "title" },
                values: new object[] { 1, "Lista para la compra semanal", "🛒", "57cV0RNcC9bnCKzbcNiBp1Tzr822", "Lista de Compras" });

            migrationBuilder.InsertData(
                table: "user_subscriptions",
                columns: new[] { "subscription_id", "uid", "end_date", "start_date" },
                values: new object[,]
                {
                    { 1, "57cV0RNcC9bnCKzbcNiBp1Tzr822", new DateTime(2025, 12, 4, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 1, "hQZnBgjuOwTTGIOVWgGHsE95OOX2", new DateTime(2025, 12, 4, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Local) },
                    { 1, "IMImLkBokLWuwjRGy7Jm7wnBWAB3", new DateTime(2025, 12, 4, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(2025, 11, 4, 0, 0, 0, 0, DateTimeKind.Local) }
                });

            migrationBuilder.InsertData(
                table: "list_users",
                columns: new[] { "list_id", "uid" },
                values: new object[] { 1, "57cV0RNcC9bnCKzbcNiBp1Tzr822" });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_from_uid",
                table: "invitations",
                column: "from_uid");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_list_id",
                table: "invitations",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_to_uid",
                table: "invitations",
                column: "to_uid");

            migrationBuilder.CreateIndex(
                name: "IX_items_checked_by",
                table: "items",
                column: "checked_by");

            migrationBuilder.CreateIndex(
                name: "IX_items_list_id",
                table: "items",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "IX_list_users_list_id",
                table: "list_users",
                column: "list_id");

            migrationBuilder.CreateIndex(
                name: "IX_lists_owner_uid",
                table: "lists",
                column: "owner_uid");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_UserUid",
                table: "Settings",
                column: "UserUid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_subscription_id",
                table: "user_subscriptions",
                column: "subscription_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "list_users");

            migrationBuilder.DropTable(
                name: "payment_mappings");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "user_subscriptions");

            migrationBuilder.DropTable(
                name: "lists");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
