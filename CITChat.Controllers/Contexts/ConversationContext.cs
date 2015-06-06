using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using CITChat.Models;

namespace CITChat.Controllers.Contexts
{
    // You can add custom code to this file. Changes will not be overwritten.
    // 
    // If you want Entity Framework to drop and regenerate your database
    // automatically whenever you change your model schema, add the following
    // code to the Application_Start method in your Global.asax file.
    // Note: this will destroy and re-create your database with every model change.
    // 
    // System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<CITChat.Models.CITChatContext>());
    public class ConversationContext : DbContext
    {
        public ConversationContext()
            : base("name=DefaultConnection")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationUser> ConversationUsers { get; set; }
        public DbSet<Message> Messages { get; set; }

        ///// <summary>
        ///// </summary>
        //static ConversationContext()
        //{
        //    IObjectContextAdapter lazyLoadingInstanceObjectContextAdapter = NonLazyLoadingInstance;
        //    lazyLoadingInstanceObjectContextAdapter.ObjectContext.ContextOptions.LazyLoadingEnabled = false;
        //}

        /// <summary>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Turn off cascading deletes
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            // Establish the relationships between these entities.
            // Define the composite key on ConversationUsers
            modelBuilder.Entity<ConversationUser>().
                         HasKey(tl => new {tl.ConversationId, tl.UserId});
            // Tell Entity Framework that a Conversation can have many ConversationUsers, that there can't be a ConversationUsers without a Conversation,
            // and that the relationship between Conversations and ConversationUsers is built on the ConversationId property in the ConversationUsers entity.
            modelBuilder.Entity<Conversation>().
                         HasMany(t => t.ConversationUsers).
                         WithRequired(tl => tl.Conversation).
                         HasForeignKey(tl => tl.ConversationId);
            // Tell Entity Framework that a User can have many ConversationUsers, that there can't be a ConversationUsers without a User,
            // and that the relationship between Users and ConversationUsers is built on the UserId property in the ConversationUsers entity.
            modelBuilder.Entity<User>().
                         HasMany(t => t.ConversationUsers).
                         WithRequired(tl => tl.User).
                         HasForeignKey(tl => tl.UserId);
        }
    }
}