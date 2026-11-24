using Microsoft.EntityFrameworkCore;
using Validation_Management_API.Models;

namespace Validation_Management_API.Data
{
    public class ApplicationDbContext (DbContextOptions options) : DbContext(options)
    {
        public DbSet<ComputerSystem> ComputerSystems => Set<ComputerSystem>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();
        public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
        public DbSet<Contributor> Contributors => Set<Contributor>();
        public DbSet<DocumentList> DocumentLists => Set<DocumentList>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<ValidationHistory> ValidationHistories => Set<ValidationHistory>();
        public DbSet<User> Users => Set<User>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DocumentList>()
                .HasMany(d => d.DependsOn).WithMany()
                .UsingEntity(j => j.ToTable("DocumentListDependencies"));

            modelBuilder.Entity<ProjectDocument>()
                .HasMany(d => d.DependsOn).WithMany()
                .UsingEntity(j => j.ToTable("ProjectDocumentDependencies"));

            modelBuilder.Entity<Project>()
                .HasMany(d => d.Locations).WithMany()
                .UsingEntity(j => j.ToTable("ProjectLocations"));

            modelBuilder.Entity<ComputerSystem>()
                .HasOne(s => s.Department).WithMany()
                .HasForeignKey(s => s.DepartmentId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComputerSystem>()
                .HasOne(s => s.Location).WithMany()
                .HasForeignKey(s => s.LocationId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComputerSystem>()
                .HasOne(s => s.Owner).WithMany()
                .HasForeignKey(s => s.OwnerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Manager).WithMany()
                .HasForeignKey(p => p.ManagerId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contributor>()
                .HasOne(c => c.Employee).WithMany()
                .HasForeignKey(c => c.EmployeeId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectDocument>()
                .HasOne(d => d.DocumentList).WithMany()
                .HasForeignKey(d => d.DocumentListId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Employee).WithMany()
                .HasForeignKey(u => u.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
