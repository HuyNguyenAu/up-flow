using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace UpFlow.API.Data
{
    /// <summary>
    /// The EF Core Database Context.
    /// </summary>
    public class TransactionDbContext : DbContext
    {
        /// <summary>
        /// The transactions table.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// The <see cref="TransactionDbContext"/> constructor used by DI.
        /// </summary>
        /// <param name="options">The DB context configuration.</param>
        public TransactionDbContext([NotNullAttribute] DbContextOptions<TransactionDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Override this method to configure the database (and other options) to be used
        /// for this context. This method is called for each instance of the context that
        /// is created. The base implementation does nothing.
        /// In situations where an instance of Microsoft.EntityFrameworkCore.DbContextOptions
        /// may or may not have been passed to the constructor, you can use Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured
        /// to determine if the options have already been set, and skip some or all of the
        /// logic in Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder).
        /// </summary>
        /// <param name="optionsBuilder">A builder used to create or modify options for this context.
        /// Databases (and other extensions) typically define extension methods on this object that
        /// allow you to configure the context.</param>

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention
        /// from the entity types exposed in Microsoft.EntityFrameworkCore.DbSet`1 properties
        /// on your derived context. The resulting model may be cached and re-used for subsequent
        /// instances of your derived context.
        /// 
        /// Remarks:
        /// If a model is explicitly set on the options for this context
        /// (via Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel))
        /// then this method will not be run.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and
        /// other extensions) typically define extension methods on this object that allow
        /// you to configure aspects of the model that are specific to a given database.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Transaction
            // Transactions.Id => Primary key
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Id);
            // Transaction.Id => Generate ID server side
            modelBuilder.Entity<Transaction>()
                .Property(d => d.Id)
                .UseIdentityColumn();
            // Device.UpId => Required
            modelBuilder.Entity<Transaction>()
                .Property(d => d.UpId)
                .IsRequired();
            // Device.UpId => Unique
            modelBuilder.Entity<Transaction>()
                .HasIndex(d => d.UpId)
                .IsUnique();
            // Device.Value => Required
            modelBuilder.Entity<Transaction>()
                .Property(d => d.Value)
                .IsRequired();
            // Transaction.SettledAt => Required
            modelBuilder.Entity<Transaction>()
                .Property(e => e.SettledAt)
                .IsRequired();
            // Transaction.CreatedAt
            modelBuilder.Entity<Transaction>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("getdate()");
            // Transaction.ModifiedAt
            modelBuilder.Entity<Transaction>()
                .Property(e => e.ModifiedAt)
                .HasComputedColumnSql("getdate()");
            #endregion
        }
    }
}
