using System;
using System.Data.Common;
using EasyDOC.DAL.Repositories;
using EasyDOC.Model;

namespace EasyDOC.DAL.DataAccess
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        //private readonly ManualContext _context;

        private IRepository<Category> _categoryRepository;
        private IRepository<Chapter> _chapterRepository;
        private IRepository<AbstractChapter> _allChaptersRepository;
        private IRepository<GeneratedChapter> _generatedChapterRepository;
        private IRepository<AbstractComponent> _componentRepository;
        private IRepository<Component> _singleComponentRepository;
        private IRepository<ComponentSeries> _componentSeriesRepository;
        private IRepository<Customer> _customerRepository;
        private IRepository<DatabaseLog> _databaseLogRepository;
        private IRepository<Employee> _employeeRepository;
        private IRepository<Folder> _catalogRepository;
        private IRepository<Documentation> _documentationRepository;
        private IRepository<Maintenance> _maintenanceRepository;
        private IRepository<File> _manualRepository;
        private IRepository<OrderConfirmation> _orderConfirmationRepository;
        private IRepository<Project> _projectRepository;
        private IRepository<Safety> _safetyRepository;
        private IRepository<ServiceLog> _serviceLogRepository;
        private IRepository<Type> _typeRepository;
        private IRepository<Vendor> _vendorRepository;
        private ISoftDeletableRepository<User> _userRepository;

        private bool _disposed;

        private readonly ManualContext _context;
        private static IUnitOfWork _instance;


        public UnitOfWork()
        {
            _context = new ManualContext();
            _instance = this;

        }

        public UnitOfWork(DbConnection connection)
        {
            _context = new ManualContext(connection);
            _instance = this;
        }

        public static IUnitOfWork Instance
        {
            get
            {
                return _instance;
            }
        }

        public IRepository<Category> CategoryRepository
        {
            get { return _categoryRepository ?? (_categoryRepository = new GenericRepository<Category>(_context)); }
        }

        public IRepository<Folder> CatalogRepository
        {
            get { return _catalogRepository ?? (_catalogRepository = new GenericRepository<Folder>(_context)); }
        }

        public IRepository<AbstractChapter> AllChaptersRepository
        {
            get { return _allChaptersRepository ?? (_allChaptersRepository = new GenericRepository<AbstractChapter>(_context)); }
        }

        public IRepository<AbstractComponent> ComponentRepository
        {
            get { return _componentRepository ?? (_componentRepository = new GenericRepository<AbstractComponent>(_context)); }
        }

        public IRepository<Component> SingleComponentRepository
        {
            get { return _singleComponentRepository ?? (_singleComponentRepository = new GenericRepository<Component>(_context)); }
        }

        public IRepository<ComponentSeries> ComponentSeriesRepository
        {
            get { return _componentSeriesRepository ?? (_componentSeriesRepository = new GenericRepository<ComponentSeries>(_context)); }
        }

        public IRepository<Customer> CustomerRepository
        {
            get { return _customerRepository ?? (_customerRepository = new GenericRepository<Customer>(_context)); }
        }

        public IRepository<DatabaseLog> DatabaseLogRepository
        {
            get { return _databaseLogRepository ?? (_databaseLogRepository = new GenericRepository<DatabaseLog>(_context)); }
        }

        public IRepository<Employee> EmployeeRepository
        {
            get { return _employeeRepository ?? (_employeeRepository = new GenericRepository<Employee>(_context)); }
        }

        public IRepository<File> FileRepository
        {
            get { return _manualRepository ?? (_manualRepository = new GenericRepository<File>(_context)); }
        }

        public IRepository<OrderConfirmation> OrderConfirmationRepository
        {
            get { return _orderConfirmationRepository ?? (_orderConfirmationRepository = new GenericRepository<OrderConfirmation>(_context)); }
        }

        public IRepository<Project> ProjectRepository
        {
            get { return _projectRepository ?? (_projectRepository = new GenericRepository<Project>(_context)); }
        }

        public IRepository<Safety> SafetyRepository
        {
            get { return _safetyRepository ?? (_safetyRepository = new GenericRepository<Safety>(_context)); }
        }

        public IRepository<ServiceLog> ServiceLogRepository
        {
            get { return _serviceLogRepository ?? (_serviceLogRepository = new GenericRepository<ServiceLog>(_context)); }
        }

        public ISoftDeletableRepository<User> UserRepository
        {
            get { return _userRepository ?? (_userRepository = new SoftDeletableRepository<User>(_context)); }
        }

        public IRepository<Vendor> VendorRepository
        {
            get { return _vendorRepository ?? (_vendorRepository = new GenericRepository<Vendor>(_context)); }
        }

        public IRepository<Maintenance> MaintenanceRepository
        {
            get { return _maintenanceRepository ?? (_maintenanceRepository = new GenericRepository<Maintenance>(_context)); }
        }

        public IRepository<Chapter> ChapterRepository
        {
            get { return _chapterRepository ?? (_chapterRepository = new GenericRepository<Chapter>(_context)); }
        }

        public IRepository<GeneratedChapter> GeneratedChapterRepository
        {
            get { return _generatedChapterRepository ?? (_generatedChapterRepository = new GenericRepository<GeneratedChapter>(_context)); }
        }

        public IRepository<Documentation> DocumentationRepository
        {
            get { return _documentationRepository ?? (_documentationRepository = new GenericRepository<Documentation>(_context)); }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }


        public void Commit()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void DetachAll()
        {
            _context.DetachAll();
        }
    }
}