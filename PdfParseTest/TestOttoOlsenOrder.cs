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
    public class TestOttoOlsenOrder
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

            _project = _uow.ProjectRepository.Get(p => p.ProjectNumber == "P2013-078").FirstOrDefault();
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "order");
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
            Assert.IsInstanceOfType(_parser, typeof(OttoOlsen));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("7061270", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("542105", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Øyvin Hagen", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("DYNATEC AS\nRAKKESTADVEIEN 1\n1814  ASKIM", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("POST 2 (BLÅ)", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("DYNATEC AS\nRAKKESTADVEIEN 1\n1814\nASKIM\nNO", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-078/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("PR. 30 dager", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("21-02-14"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorVisitingAddress() { Assert.AreEqual("Nesgaten 19.\nN-2004 Lillestrøm", _orderConfirmation.Vendor.VisitingAddress); }
        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Postboks 44\nN-2001 Lillestrøm", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 63 89 08 00", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+47 63 89 08 99", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NO 980 350 665 MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("6201.05.07001", _orderConfirmation.Vendor.AccountNumber); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(1277.50), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(1596.88), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("38100234", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("BA             35,0  50,0  7,0", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.AreEqual("FKM", _orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("21-02-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(5.00), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(0).Unit);
            Assert.AreEqual(new Decimal(135.50), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(677.50), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("8009737", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual("O-ring 98,00 x 2,00 Viton 85sh", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.AreEqual("Produseres", _orderConfirmation.Items.ElementAt(1).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("21-02-14"), _orderConfirmation.Items.ElementAt(1).DeliveryDate);
            Assert.AreEqual(new Decimal(4.00), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(1).Unit);
            Assert.AreEqual(new Decimal(150.00), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(600.00), _orderConfirmation.Items.ElementAt(1).TotalAmount);
        }
    }
}
