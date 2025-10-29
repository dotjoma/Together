using Microsoft.EntityFrameworkCore.Migrations;

namespace Together.Infrastructure.Migrations
{
    /// <summary>
    /// Migration to add performance indexes on foreign keys and commonly queried fields
    /// </summary>
    public partial class AddPerformanceIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // User indexes
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PartnerId",
                table: "Users",
                column: "PartnerId");

            // Post indexes
            migrationBuilder.CreateIndex(
                name: "IX_Posts_AuthorId_CreatedAt",
                table: "Posts",
                columns: new[] { "AuthorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CreatedAt",
                table: "Posts",
                column: "CreatedAt");

            // FollowRelationship indexes
            migrationBuilder.CreateIndex(
                name: "IX_FollowRelationships_FollowerId_Status",
                table: "FollowRelationships",
                columns: new[] { "FollowerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FollowRelationships_FollowingId_Status",
                table: "FollowRelationships",
                columns: new[] { "FollowingId", "Status" });

            // MoodEntry indexes
            migrationBuilder.CreateIndex(
                name: "IX_MoodEntries_UserId_Timestamp",
                table: "MoodEntries",
                columns: new[] { "UserId", "Timestamp" });

            // JournalEntry indexes
            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ConnectionId_CreatedAt",
                table: "JournalEntries",
                columns: new[] { "ConnectionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_AuthorId",
                table: "JournalEntries",
                column: "AuthorId");

            // TodoItem indexes
            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ConnectionId_Completed",
                table: "TodoItems",
                columns: new[] { "ConnectionId", "Completed" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_AssignedTo",
                table: "TodoItems",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_DueDate",
                table: "TodoItems",
                column: "DueDate");

            // SharedEvent indexes
            migrationBuilder.CreateIndex(
                name: "IX_SharedEvents_ConnectionId_EventDate",
                table: "SharedEvents",
                columns: new[] { "ConnectionId", "EventDate" });

            // Challenge indexes
            migrationBuilder.CreateIndex(
                name: "IX_Challenges_ConnectionId_ExpiresAt",
                table: "Challenges",
                columns: new[] { "ConnectionId", "ExpiresAt" });

            // Like indexes
            migrationBuilder.CreateIndex(
                name: "IX_Likes_PostId_UserId",
                table: "Likes",
                columns: new[] { "PostId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId",
                table: "Likes",
                column: "UserId");

            // Comment indexes
            migrationBuilder.CreateIndex(
                name: "IX_Comments_PostId_CreatedAt",
                table: "Comments",
                columns: new[] { "PostId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            // Notification indexes
            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            // CoupleConnection indexes
            migrationBuilder.CreateIndex(
                name: "IX_CoupleConnections_User1Id",
                table: "CoupleConnections",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_CoupleConnections_User2Id",
                table: "CoupleConnections",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_CoupleConnections_Status",
                table: "CoupleConnections",
                column: "Status");

            // ConnectionRequest indexes
            migrationBuilder.CreateIndex(
                name: "IX_ConnectionRequests_FromUserId",
                table: "ConnectionRequests",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionRequests_ToUserId_Status",
                table: "ConnectionRequests",
                columns: new[] { "ToUserId", "Status" });

            // VirtualPet indexes
            migrationBuilder.CreateIndex(
                name: "IX_VirtualPets_ConnectionId",
                table: "VirtualPets",
                column: "ConnectionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all indexes in reverse order
            migrationBuilder.DropIndex(name: "IX_VirtualPets_ConnectionId", table: "VirtualPets");
            migrationBuilder.DropIndex(name: "IX_ConnectionRequests_ToUserId_Status", table: "ConnectionRequests");
            migrationBuilder.DropIndex(name: "IX_ConnectionRequests_FromUserId", table: "ConnectionRequests");
            migrationBuilder.DropIndex(name: "IX_CoupleConnections_Status", table: "CoupleConnections");
            migrationBuilder.DropIndex(name: "IX_CoupleConnections_User2Id", table: "CoupleConnections");
            migrationBuilder.DropIndex(name: "IX_CoupleConnections_User1Id", table: "CoupleConnections");
            migrationBuilder.DropIndex(name: "IX_Notifications_UserId_IsRead", table: "Notifications");
            migrationBuilder.DropIndex(name: "IX_Notifications_UserId_CreatedAt", table: "Notifications");
            migrationBuilder.DropIndex(name: "IX_Comments_AuthorId", table: "Comments");
            migrationBuilder.DropIndex(name: "IX_Comments_PostId_CreatedAt", table: "Comments");
            migrationBuilder.DropIndex(name: "IX_Likes_UserId", table: "Likes");
            migrationBuilder.DropIndex(name: "IX_Likes_PostId_UserId", table: "Likes");
            migrationBuilder.DropIndex(name: "IX_Challenges_ConnectionId_ExpiresAt", table: "Challenges");
            migrationBuilder.DropIndex(name: "IX_SharedEvents_ConnectionId_EventDate", table: "SharedEvents");
            migrationBuilder.DropIndex(name: "IX_TodoItems_DueDate", table: "TodoItems");
            migrationBuilder.DropIndex(name: "IX_TodoItems_AssignedTo", table: "TodoItems");
            migrationBuilder.DropIndex(name: "IX_TodoItems_ConnectionId_Completed", table: "TodoItems");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_AuthorId", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_JournalEntries_ConnectionId_CreatedAt", table: "JournalEntries");
            migrationBuilder.DropIndex(name: "IX_MoodEntries_UserId_Timestamp", table: "MoodEntries");
            migrationBuilder.DropIndex(name: "IX_FollowRelationships_FollowingId_Status", table: "FollowRelationships");
            migrationBuilder.DropIndex(name: "IX_FollowRelationships_FollowerId_Status", table: "FollowRelationships");
            migrationBuilder.DropIndex(name: "IX_Posts_CreatedAt", table: "Posts");
            migrationBuilder.DropIndex(name: "IX_Posts_AuthorId_CreatedAt", table: "Posts");
            migrationBuilder.DropIndex(name: "IX_Users_PartnerId", table: "Users");
            migrationBuilder.DropIndex(name: "IX_Users_Username", table: "Users");
            migrationBuilder.DropIndex(name: "IX_Users_Email", table: "Users");
        }
    }
}
