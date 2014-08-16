using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;

namespace EasyDOC.Model
{
    public class ManualContext : DbContext
    {
        public ManualContext()
            : base("name=ManualContext")
        {
            LoadEnums();
        }

        public ManualContext(DbConnection connection)
            : base(connection, false)
        {
            LoadEnums();
        }

        private void LoadEnums()
        {
            FileRoles = (IQueryable<FileRole>)Enum.GetValues(typeof(FileRole)).AsQueryable();
            TaskStates = (IQueryable<TaskState>)Enum.GetValues(typeof(TaskState)).AsQueryable();
            ProjectStatus = (IQueryable<ProjectStatus>)Enum.GetValues(typeof(ProjectStatus)).AsQueryable();
            SafetyRoles = (IQueryable<SafetyRole>)Enum.GetValues(typeof(SafetyRole)).AsQueryable();
            TaskLinkTypes = (IQueryable<TaskLinkType>)Enum.GetValues(typeof(TaskLinkType)).AsQueryable();
            TaskConstraints = (IQueryable<TaskConstraint>)Enum.GetValues(typeof(TaskConstraint)).AsQueryable();
            RoleScopes = (IQueryable<RoleScope>)Enum.GetValues(typeof(RoleScope)).AsQueryable();
            Units = (IQueryable<Unit>)Enum.GetValues(typeof(Unit)).AsQueryable();
        }

        public DbSet<AbstractChapter> Chapters { get; set; }
        public DbSet<AbstractComponent> Components { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ChapterFile> ChapterFiles { get; set; }
        public DbSet<Component> SingleComponents { get; set; }
        public DbSet<ComponentFile> ComponentFiles { get; set; }
        public DbSet<ComponentMaintenance> ComponentMaintenances { get; set; }
        public DbSet<ComponentSafety> ComponentSafeties { get; set; }
        public DbSet<ComponentSeries> ComponentSeries { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<DatabaseLog> DatabaseLogs { get; set; }
        public DbSet<Documentation> Documentations { get; set; }
        public DbSet<DocumentationChapter> DocumentationChapters { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeTask> EmployeeTasks { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<OrderConfirmation> OrderConfirmations { get; set; }
        public DbSet<OrderConfirmationItem> OrderConfirmationItems { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectComponent> ProjectComponents { get; set; }
        public DbSet<ProjectFile> ProjectFiles { get; set; }
        public DbSet<ProjectMaintenance> ProjectMaintenances { get; set; }
        public DbSet<ProjectSafety> ProjectSafeties { get; set; }
        public DbSet<Safety> Safeties { get; set; }
        public DbSet<ServiceLog> ServiceLogs { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskCheckPoint> TaskCheckPoints { get; set; }
        public DbSet<TaskLink> TaskLinks { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public IQueryable<FileRole> FileRoles { get; set; }
        public IQueryable<TaskState> TaskStates { get; set; }
        public IQueryable<TaskLinkType> TaskLinkTypes { get; set; }
        public IQueryable<TaskConstraint> TaskConstraints { get; set; }
        public IQueryable<ProjectStatus> ProjectStatus { get; set; }
        public IQueryable<SafetyRole> SafetyRoles { get; set; }
        public IQueryable<RoleScope> RoleScopes { get; set; }
        public IQueryable<Unit> Units { get; set; }

        public void DetachAll()
        {
            foreach (var item in ChangeTracker.Entries<IEntity>())
            {
                item.State = EntityState.Detached;
            }
        }

        public override int SaveChanges()
        {
            foreach (var item in ChangeTracker.Entries<ISoftDeletable>().Where(item => item.State == EntityState.Deleted))
            {
                item.State = EntityState.Modified;
                item.Entity.Deleted = DateTime.Now;
            }

            foreach (var item in ChangeTracker.Entries<IHistoryEntity>())
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        item.Entity.Created = DateTime.UtcNow;
                        item.Entity.Edited = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        item.Entity.Edited = DateTime.UtcNow;
                        break;
                }
            }

            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                Debug.WriteLine(e);
            }

            return 0;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            modelBuilder.Entity<AbstractComponent>().HasMany(p => p.Experiences).WithOptional(e => e.Component).HasForeignKey(e => e.ComponentId);
            modelBuilder.Entity<AbstractComponent>().HasMany(c => c.Maintenances).WithRequired(pc => pc.Component).HasForeignKey(pc => pc.ComponentId);
            modelBuilder.Entity<AbstractComponent>().HasMany(c => c.Projects).WithRequired(pc => pc.Component).HasForeignKey(pc => pc.ComponentId);

            modelBuilder.Entity<AbstractChapter>().HasOptional(p => p.Project).WithMany(c => c.Chapters).HasForeignKey(p => p.ProjectId);

            modelBuilder.Entity<Category>().HasMany(cs => cs.SubCategories).WithOptional(c => c.ParentCategory).HasForeignKey(c => c.ParentCategoryId);

            modelBuilder.Entity<Component>().HasMany(c => c.OrderConfirmationItems).WithRequired(i => i.Component).HasForeignKey(i => i.ComponentId);

            modelBuilder.Entity<ComponentFile>().HasRequired(f => f.Component).WithMany(c => c.Files).HasForeignKey(f => f.ComponentId);
            modelBuilder.Entity<ComponentFile>().HasRequired(f => f.File).WithMany(c => c.Components).HasForeignKey(f => f.FileId);

            modelBuilder.Entity<ComponentSeries>().HasMany(cs => cs.Components).WithOptional(c => c.Series).HasForeignKey(c => c.SeriesId);

            modelBuilder.Entity<Customer>().HasMany(c => c.Projects).WithOptional(p => p.Customer).HasForeignKey(p => p.CustomerId);

            modelBuilder.Entity<Documentation>().HasMany(cs => cs.DocumentationChapters).WithRequired(c => c.Documentation).HasForeignKey(c => c.DocumentationId);

            modelBuilder.Entity<DocumentationChapter>().HasRequired(f => f.Chapter).WithMany(c => c.DocumentationChapters).HasForeignKey(f => f.ChapterId);
            modelBuilder.Entity<DocumentationChapter>().HasRequired(f => f.Documentation).WithMany(c => c.DocumentationChapters).HasForeignKey(f => f.DocumentationId);

            modelBuilder.Entity<Employee>().HasMany(e => e.CheckPoints).WithOptional(tcp => tcp.Employee).HasForeignKey(tcp => tcp.EmployeeId);
            modelBuilder.Entity<Employee>().HasMany(e => e.ManagedProjects).WithOptional(p => p.ProjectManager).HasForeignKey(p => p.ProjectManagerId);

            modelBuilder.Entity<EmployeeTask>().HasRequired(t => t.Employee).WithMany(e => e.Tasks).HasForeignKey(rel => rel.EmployeeId);
            modelBuilder.Entity<EmployeeTask>().HasRequired(t => t.Task).WithMany(t => t.Resources).HasForeignKey(rel => rel.TaskId);

            modelBuilder.Entity<Folder>().HasMany(cs => cs.Catalogs).WithOptional(c => c.Parent).HasForeignKey(c => c.ParentId);
            modelBuilder.Entity<Folder>().HasMany(cs => cs.Files).WithRequired(c => c.Folder).HasForeignKey(c => c.CatalogId);

            modelBuilder.Entity<Project>().HasMany(p => p.Components).WithRequired(pc => pc.Project).HasForeignKey(pc => pc.ProjectId);
            modelBuilder.Entity<Project>().HasMany(p => p.Experiences).WithOptional(e => e.Project).HasForeignKey(e => e.ProjectId);
            modelBuilder.Entity<Project>().HasMany(p => p.OrderConfirmations).WithRequired(oc => oc.Project).HasForeignKey(oc => oc.ProjectId);
            modelBuilder.Entity<Project>().HasMany(p => p.SubProjects).WithOptional(oc => oc.MasterProject).HasForeignKey(oc => oc.MasterProjectId);

            modelBuilder.Entity<ProjectFile>().HasRequired(f => f.File).WithMany(c => c.Projects).HasForeignKey(f => f.FileId);
            modelBuilder.Entity<ProjectFile>().HasRequired(f => f.Project).WithMany(c => c.Files).HasForeignKey(f => f.ProjectId);

            modelBuilder.Entity<Safety>().HasMany(c => c.Components).WithRequired(rel => rel.Safety).HasForeignKey(rel => rel.SafetyId);
            modelBuilder.Entity<Safety>().HasMany(c => c.Projects).WithRequired(rel => rel.Safety).HasForeignKey(rel => rel.SafetyId);

            modelBuilder.Entity<Task>().HasMany(t => t.CheckPoints).WithRequired(tcp => tcp.Task).HasForeignKey(tcp => tcp.TaskId);
            modelBuilder.Entity<Task>().HasMany(t => t.ChildTasks).WithOptional(t => t.ParentTask).HasForeignKey(t => t.ParentTaskId);
            modelBuilder.Entity<Task>().HasMany(t => t.Predecessors).WithRequired(tl => tl.To).HasForeignKey(tl => tl.ToId);
            modelBuilder.Entity<Task>().HasMany(t => t.Successors).WithRequired(tl => tl.From).HasForeignKey(tl => tl.FromId);

            modelBuilder.Entity<UserRole>().HasRequired(ur => ur.Role).WithMany(r => r.Users).HasForeignKey(ur => ur.RoleId);
            modelBuilder.Entity<UserRole>().HasRequired(ur => ur.User).WithMany(r => r.Roles).HasForeignKey(ur => ur.UserId);
        }
    }
}