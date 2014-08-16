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
    public class TestOmronOrder
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

            _project = _uow.ProjectRepository.Get(p => p.ProjectNumber == "P2013-083").FirstOrDefault();
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "R5942565_EU0028S_22104496 SO 10449601");
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
            Assert.IsInstanceOfType(_parser, typeof(Omron));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("42173189", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("22104496 SO", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.IsNull(_orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("DYNATEC A/S*\nRAKKESTADVEIEN 1\n1814 ASKIM\nNorway", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("DYNATEC AS*\nRAKKESTADVEIEN 1\nAtt. Bendik Eide\n1814 ASKIM\nNorway", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Road", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("Delivered Duty Paid" ,_orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Bendik Eide"); }
        [TestMethod] public void Tag() { Assert.AreEqual("P2013-083 /Bendik Eide", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentMethod() { Assert.IsNull(_orderConfirmation.PaymentMethod); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("45 DAYS NET", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("13.03.2014"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Omron Electronics Norway A/S\nPB 109 Bryn\nNO-0611 OSLO\nNorway", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 22 65 75 00", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.IsNull(_orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NO 852 345 632 MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("9760.01.03699", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorLink() { Assert.AreEqual("www.omron.no", _orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("omron.norway@eu.omron.com", _orderConfirmation.Vendor.Email); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(2035.00), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(2543.75), _orderConfirmation.TotalAmountWithTaxes); }
        [TestMethod] public void HasCorrectNumberOfLines() { Assert.AreEqual(2, _orderConfirmation.Items.Count); }

        [TestMethod]
        public void FirstItem()
        {
            var item = _orderConfirmation.Items.ElementAt(0);

            Assert.AreEqual("R88A-CSK003S-E", item.Component.Name);
            Assert.AreEqual("Accessory for Servo", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("13.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(13), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(145.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(1885.00), item.TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            var item = _orderConfirmation.Items.ElementAt(1);

            Assert.AreEqual("FRAKT", item.Component.Name);
            Assert.AreEqual("Other services", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("14.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(150.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(150.00), item.TotalAmount);
        }
    }
}
