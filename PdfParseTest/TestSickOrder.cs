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
    public class TestSickOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "200230");
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
            Assert.IsInstanceOfType(_parser, typeof(Sick));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("10223", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("66474", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Rune Svarthe", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec A/S\nRakkestadveien 1\n1814 ASKIM", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Bedriftspakke Bring", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Dynatec A/S\nRakkestadveien 1\n1814 ASKIM", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-079/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Netto pr. 30 dager", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("25-02-14"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Bærumsveien 383\n1346 Gjettum", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 67 81 50 00", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+47 67 81 50 01", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("966879548MVA", _orderConfirmation.Vendor.OrganizationNumber); }

        [TestMethod] public void Total() { Assert.AreEqual(new Decimal(9483.75), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("6028433", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("WF80-40B410 Gaffelfotocelle", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.AreEqual("Country of origin: France", _orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("28-02-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(3), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(new Decimal(2015.00), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(7045.00), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("997", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual("Emballasje", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.AreEqual(DateTime.Parse("28-02-14"), _orderConfirmation.Items.ElementAt(1).DeliveryDate);
            Assert.AreEqual(new Decimal(1), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(new Decimal(42.00), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(42.00), _orderConfirmation.Items.ElementAt(1).TotalAmount);
        }
    }
}
