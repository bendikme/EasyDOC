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
    public class TestLesjoforsOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "Confirm_65225_0001622957");
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
            Assert.IsInstanceOfType(_parser, typeof(Lesjofors));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("65225", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("0001622957", _orderConfirmation.OrderNumber); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("24-02-14"), _orderConfirmation.OrderDate); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Post", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("FCA Incoterms® 2010", _orderConfirmation.DeliveryConditions); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 ASKIM", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 ASKIM", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-078/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Netto pr. 30 dager", _orderConfirmation.PaymentConditions); }

        [TestMethod] public void VendorReference() { Assert.AreEqual("Rita Mørk", _orderConfirmation.VendorReference); }
        [TestMethod] public void VendorPostalAddress() { Assert.IsNull(_orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.IsNull(_orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.IsNull(_orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("968703439MVA", _orderConfirmation.Vendor.OrganizationNumber); }

        [TestMethod] public void TotalAmountWithoutTaxes() { Assert.AreEqual(new Decimal(427.00), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalAmountWithTaxes() { Assert.AreEqual(new Decimal(533.75), _orderConfirmation.TotalAmountWithTaxes); }
        [TestMethod] public void HasCorrectNumberOfItems() { Assert.AreEqual(2, _orderConfirmation.Items.Count); }

        [TestMethod]
        public void FirstItem()
        {
            var item = _orderConfirmation.Items.ElementAt(0);

            Assert.AreEqual("4224", item.Component.Name);
            Assert.AreEqual("LSA 4 RF\nLåsesplint SS2331", item.Component.Description);
            //Assert.AreEqual("", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("28.02.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(2), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(111.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(222.00), item.TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            var item = _orderConfirmation.Items.ElementAt(1);

            Assert.AreEqual("SK1", item.Component.Name);
            Assert.AreEqual("Frakt og emballasje", item.Component.Description);
            //Assert.AreEqual("", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("28.02.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(155.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(155.00), item.TotalAmount);
        }
    }
}
