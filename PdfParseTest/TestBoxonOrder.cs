using System;
using System.Linq;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.ConfirmationOrderParsers;
using EasyDOC.Pdf.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PdfParseTest
{
    [TestClass]
    public class TestBoxonOrder
    {
        private IUnitOfWork _uow;
        private OrderConfirmationService _orderService;
        private Project _project;
        private ProjectFile _relation;
        private IOrderConfirmationParser _parser;
        private OrderConfirmation _orderConfirmation;
        private const string Root = @"c:\users\bendike\documents\visual studio 2012\projects\easydoc\easydoc.api";

        [TestInitialize]
        public void Setup()
        {
            _uow = new UnitOfWork();
            _orderService = new OrderConfirmationService(null, _uow, Root);

            _project = _uow.ProjectRepository.Get(p => p.ProjectNumber == "P2013-087").FirstOrDefault();
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "OB_1_20022014_033418");
            _parser = OrderConfirmationFactory.GetParser(Root, _relation.File);
            _orderConfirmation = _orderService.CreateFromProjectFile(_relation);
        }

        [TestMethod]
        public void ProjectFound()
        {
            Assert.IsNotNull(_project);
        }

        [TestMethod]
        public void FileFound()
        {
            Assert.IsNotNull(_relation);
        }

        [TestMethod]
        public void FactoryReturnsGenericParser()
        {
            Assert.IsInstanceOfType(_parser, typeof(Boxon));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("46339", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("3035063", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Nina Herberg", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("DYNATEC AS\nNO-1814 ASKIM", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Tollpost", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("DYNATEC AS\nRAKKESTADVEIEN 1\nNO-1814 ASKIM", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("P2013-087/KLAUS", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Netto + 30 dager", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("20.02.2014"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Vintergata 19\nNO-3048 DRAMMEN", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("31301900", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("31301909", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("992 815 922 MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("8101 07 88293", _orderConfirmation.Vendor.AccountNumber); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(73862.95), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(92328.69), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("M-PA250ELR00", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("EASYLINER PRINT APPLY RH", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("30-04-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(1.00), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(new Decimal(23200.00), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(15.00), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(19720.00), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void NinthItem()
        {
            Assert.AreEqual("M-AG001L01", _orderConfirmation.Items.ElementAt(8).Component.Name);
            Assert.AreEqual("ADJUSTMENT GROUP FOR LABELLERS", _orderConfirmation.Items.ElementAt(8).Component.Description);
            Assert.AreEqual(DateTime.Parse("30-04-14"), _orderConfirmation.Items.ElementAt(8).DeliveryDate);
            Assert.AreEqual(new Decimal(1.00), _orderConfirmation.Items.ElementAt(8).Quantity);
            Assert.AreEqual(new Decimal(5499.85), _orderConfirmation.Items.ElementAt(8).AmountPerUnit);
            Assert.AreEqual(new Decimal(15.01), _orderConfirmation.Items.ElementAt(8).Discount);
            Assert.AreEqual(new Decimal(4674.87), _orderConfirmation.Items.ElementAt(8).TotalAmount);
        }

        [TestMethod]
        public void TenthItem()
        {
            Assert.AreEqual("M-F623B", _orderConfirmation.Items.ElementAt(9).Component.Name);
            Assert.AreEqual("MECHANICAL DEVICE FOR ADJUSTMENT", _orderConfirmation.Items.ElementAt(9).Component.Description);
            Assert.AreEqual(DateTime.Parse("30-04-14"), _orderConfirmation.Items.ElementAt(9).DeliveryDate);
            Assert.AreEqual(new Decimal(1.00), _orderConfirmation.Items.ElementAt(9).Quantity);
            Assert.AreEqual(new Decimal(2491.65), _orderConfirmation.Items.ElementAt(9).AmountPerUnit);
            Assert.AreEqual(new Decimal(15.01), _orderConfirmation.Items.ElementAt(9).Discount);
            Assert.AreEqual(new Decimal(2117.90), _orderConfirmation.Items.ElementAt(9).TotalAmount);
        }

        [TestMethod]
        public void EleventhItem()
        {
            Assert.AreEqual("110117", _orderConfirmation.Items.ElementAt(10).Component.Name);
            Assert.AreEqual("Arbeidstid Montering / Service", _orderConfirmation.Items.ElementAt(10).Component.Description);
            Assert.AreEqual(DateTime.Parse("30-04-14"), _orderConfirmation.Items.ElementAt(10).DeliveryDate);
            Assert.AreEqual(new Decimal(1.00), _orderConfirmation.Items.ElementAt(10).Quantity);
            Assert.AreEqual(new Decimal(6636.80), _orderConfirmation.Items.ElementAt(10).AmountPerUnit);
            Assert.AreEqual(new Decimal(6636.80), _orderConfirmation.Items.ElementAt(10).TotalAmount);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(10).Discount);
        }
    }
}
