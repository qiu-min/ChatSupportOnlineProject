using ChatSupportOnline.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatSupportOnline.Api.Data;

/// <summary>
/// 应用的数据库上下文。
/// 它负责把内存中的实体对象映射到数据库表，并定义实体之间的关系。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 用户表，对应系统中的客服和访客账号。
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// 会话表，对应一个客服聊天会话。
    /// </summary>
    public DbSet<Conversation> Conversations => Set<Conversation>();

    /// <summary>
    /// 消息表，对应会话中的每一条聊天记录。
    /// </summary>
    public DbSet<Message> Messages => Set<Message>();

    /// <summary>
    /// 使用 Fluent API 明确配置实体关系和字段约束。
    /// 这样做比完全依赖默认约定更清晰，也更适合学习数据库建模。
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.UserName).HasMaxLength(50).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.HasIndex(user => user.UserName).IsUnique();
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(conversation => conversation.Id);
            entity.Property(conversation => conversation.Subject).HasMaxLength(200).IsRequired();

            entity.HasOne(conversation => conversation.Customer)
                .WithMany(user => user.CustomerConversations)
                .HasForeignKey(conversation => conversation.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(conversation => conversation.Agent)
                .WithMany(user => user.AgentConversations)
                .HasForeignKey(conversation => conversation.AgentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Content).HasMaxLength(2000).IsRequired();

            entity.HasOne(message => message.Conversation)
                .WithMany(conversation => conversation.Messages)
                .HasForeignKey(message => message.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(message => message.Sender)
                .WithMany(user => user.Messages)
                .HasForeignKey(message => message.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
