using System;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;

namespace EasyDOC.BLL.Services
{
    public class OrderConfirmationService : GenericService<OrderConfirmation>
    {
        private readonly string _rootDirectory;

        public OrderConfirmationService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork, string rootDirectory)
            : base(validationDictionary, unitOfWork, unitOfWork.OrderConfirmationRepository)
        {
            _rootDirectory = rootDirectory;
        }

        public OrderConfirmation CreateFromProjectFile(ProjectFile relation)
        {
            var file = UnitOfWork.FileRepository.GetByKey(relation.FileId);

            if (file == null)
            {
                throw new Exception("File with id " + relation.FileId + " not found in database.");
            }

            var parser = OrderConfirmationFactory.GetParser(_rootDirectory, file);

            if (parser != null)
            {
                var confirmation = parser.ExtractOrderConfirmationData(UnitOfWork);
                confirmation.ProjectId = relation.ProjectId;
                confirmation.FileId = relation.FileId;
                confirmation.Name = "Ordrebekreftelse " + confirmation.OrderNumber; // + " fra " + confirmation.Vendor.Name;

                Create(confirmation);
                return confirmation;
            }

            return null;
        }
    }
}