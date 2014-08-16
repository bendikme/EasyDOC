using EasyDOC.DAL.Repositories;
using EasyDOC.Model;

namespace EasyDOC.DAL.DataAccess
{
    public interface IUnitOfWork
    {
        IRepository<Category> CategoryRepository { get; }
        IRepository<Folder> CatalogRepository { get; }
        IRepository<AbstractChapter> AllChaptersRepository { get; }
        IRepository<Chapter> ChapterRepository { get; }
        IRepository<GeneratedChapter> GeneratedChapterRepository { get; }
        IRepository<AbstractComponent> ComponentRepository { get; }
        IRepository<Component> SingleComponentRepository { get; }
        IRepository<ComponentSeries> ComponentSeriesRepository { get; }
        IRepository<Customer> CustomerRepository { get; }
        IRepository<DatabaseLog> DatabaseLogRepository { get; }
        IRepository<Documentation> DocumentationRepository { get; }
        IRepository<Employee> EmployeeRepository { get; }
        IRepository<Maintenance> MaintenanceRepository { get; }
        IRepository<File> FileRepository { get; }
        IRepository<OrderConfirmation> OrderConfirmationRepository { get; }
        IRepository<Project> ProjectRepository { get; }
        IRepository<Safety> SafetyRepository { get; }
        IRepository<ServiceLog> ServiceLogRepository { get; }
        ISoftDeletableRepository<User> UserRepository { get; }
        IRepository<Vendor> VendorRepository { get; }

        void Commit();
        void Dispose();
        void DetachAll();
    }
}