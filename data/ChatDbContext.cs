using Microsoft.EntityFrameworkCore;
using SignalRChatServer.Models;

namespace SignalRChatServer.Data
{
    public class ChatDbContext : DbContext
    {
        public ChatDbContext (DbContextOptions<ChatDbContext> options)
            : base(options) {}

        public DbSet<UserMessage> Message { get; set; } = default!;
        public DbSet<UserConnection> ChatServerConnection { get; set; } = default!;
        public DbSet<Product> Product { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<ChatList> ChatList { get; set; } = default!;
    }
}
