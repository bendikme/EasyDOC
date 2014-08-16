using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.Interface
{
    public interface IOrderConfirmationParser
    {
        OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow);
        string VendorName { get; set; }
    }
}