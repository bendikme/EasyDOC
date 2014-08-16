using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class CatalogService : GenericService<Folder>
    {
        public CatalogService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.CatalogRepository)
        {
        }

        #region Related entities

        public void AddCatalog(Folder parent, Folder folder)
        {
            parent.Catalogs.Add(folder);
        }

        public void AddCatalog(Folder parent, int catalogId)
        {
            var catalog = UnitOfWork.CatalogRepository.GetByKey(catalogId);
            parent.Catalogs.Add(catalog);
        }

        public void RemoveCatalog(Folder parent, int catalogId)
        {
            var catalog = UnitOfWork.CatalogRepository.GetByKey(catalogId);
            parent.Catalogs.Remove(catalog);
        }

        public void AddFile(Folder parent, int fileId)
        {
            var file = UnitOfWork.FileRepository.GetByKey(fileId);
            parent.Files.Add(file);
        }

        public void RemoveFile(Folder parent, int fileId)
        {
            var file = UnitOfWork.FileRepository.GetByKey(fileId);
            parent.Files.Remove(file);
        }

        #endregion

    }
}
