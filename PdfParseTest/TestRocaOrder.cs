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
    public class TestRocaOrder
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

            _project = _uow.ProjectRepository.Get(p => p.ProjectNumber == "P2013-079").FirstOrDefault();
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "544888-1");
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
            Assert.IsInstanceOfType(_parser, typeof(Roca));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("5109", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("544888-1", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Peter Cederström", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("DYNATEC AS\nRAKKESTADVEIEN 1\n1814 ASKIM\nNORGE", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("DYNATEC AS\nRAKKESTADVEIEN 1\n1814 ASKIM", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("SCHENKER BTL", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("001 FRITT LAGER TYRESÖ" ,_orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("P2013-079/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentMethod() { Assert.IsNull(_orderConfirmation.PaymentMethod); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("30 DAGAR NETTO", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("03.03.14"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Roca Industry AB\nBox 693, Radiovägen 19\n135 26 Tyresö\nSverige", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+46 8 448 7320", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+46 8 742 3225", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("SE556103390201", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("904 409 012 03", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorLink() { Assert.AreEqual("www.rocaindustry.com", _orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("info@rocaindustry.com", _orderConfirmation.Vendor.Email); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(760.00), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(760.00), _orderConfirmation.TotalAmountWithTaxes); }
        [TestMethod] public void HasCorrectNumberOfLines() { Assert.AreEqual(2, _orderConfirmation.Items.Count); }

        [TestMethod]
        public void FirstItem()
        {
            var item = _orderConfirmation.Items.ElementAt(0);

            Assert.AreEqual("423224", item.Component.Name);
            Assert.AreEqual("HÅNDTAK Ø25\nC-C=450MM 316 EP", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("10.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(2), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(265.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(20.00), item.Discount);
            Assert.AreEqual(new Decimal(424.00), item.TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            var item = _orderConfirmation.Items.ElementAt(1);

            Assert.AreEqual("423223", item.Component.Name);
            Assert.AreEqual("HÅNDTAK Ø25\nC-C=300MM 316 EP", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("10.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(2), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(210.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(20.00), item.Discount);
            Assert.AreEqual(new Decimal(336.00), item.TotalAmount);
        }
    }
}
