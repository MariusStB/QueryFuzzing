using Microsoft.EntityFrameworkCore;
using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Database
{
    public class QueryFuzzContext:DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<FuzzingInstance> FuzzingInstance { get; set; }
        public DbSet<ProjectTarget> ProjectTargets { get; set; }
        public DbSet<InstanceTarget> InstanceTargets { get; set; }
        public DbSet<Executable> Executables { get; set; }
        public DbSet<Crash> Crash { get; set; }
        public DbSet<CrashedTarget> CrashedTargets { get; set; }

        public QueryFuzzContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=QFDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasKey(k => k.Id);
            modelBuilder.Entity<Project>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ProjectTarget>().HasKey(k => k.Id);
            modelBuilder.Entity<ProjectTarget>().Property(e=> e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<InstanceTarget>().HasKey(k => k.Id);
            modelBuilder.Entity<InstanceTarget>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<CrashedTarget>().HasKey(k => k.Id);
            modelBuilder.Entity<CrashedTarget>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<FuzzingInstance>().HasKey(k => k.Id);
            modelBuilder.Entity<FuzzingInstance>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Executable>().HasKey(k => k.Id);
            modelBuilder.Entity<Executable>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Crash>().HasKey(k => k.Id);
            modelBuilder.Entity<Crash>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<FuzzingStat>().HasKey(k => k.Id);
            modelBuilder.Entity<FuzzingStat>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Project>().HasMany(e => e.Targets).WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .HasPrincipalKey(e=> e.Id);

            modelBuilder.Entity<Project>().HasMany(e => e.FuzzingInstance).WithOne(e => e.Project)
                .HasForeignKey(e => e.ProjectId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<FuzzingInstance>().HasMany(e => e.Executables).WithOne(e => e.FuzzingInstance)
                .HasForeignKey(e => e.FuzzingInstanceId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<FuzzingInstance>().HasMany(e => e.InstanceTargets).WithOne(e => e.FuzzingInstance)
                .HasForeignKey(e => e.FuzzingInstanceId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<FuzzingInstance>().HasMany(e => e.Crashes).WithOne(e => e.FuzzingInstance)
                .HasForeignKey(e => e.FuzzingInstanceId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Crash>().HasMany(e => e.CrashedTargets).WithOne(e => e.Crash)
                .HasForeignKey(e => e.CrashId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<Crash>().HasMany(e => e.Stacktrace).WithOne(e => e.Crash)
                .HasForeignKey(e => e.CrashId)
                .HasPrincipalKey(e => e.Id);

            modelBuilder.Entity<FuzzingInstance>().HasOne(e => e.FinalStats).WithOne(e => e.FuzzingInstance)
                .HasForeignKey<FuzzingStat>(e => e.FuzzingInstanceId);

        }
    }
}
