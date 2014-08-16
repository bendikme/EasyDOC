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
    public class TestRollcoOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "Order-1300349");
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
            Assert.IsInstanceOfType(_parser, typeof(Rollco));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("NO0002", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("1300349", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Mads Dyhrfjeld", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 Askim\nNorge", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 Askim", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("TNT ECONOMY", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("EXW Faktureres,  ekskl. emb." ,_orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-079KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentMethod() { Assert.IsNull(_orderConfirmation.PaymentMethod); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("30 dager netto", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("23.02.14"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Rollco Norge AS\nBergliveien 2\n3400  Lier\nNorge", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 32840034", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+47 32840091", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NO984962983MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("9493.05.36355", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorLink() { Assert.AreEqual("www.rollco.no", _orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("info@rollco.no", _orderConfirmation.Vendor.Email); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(8213.76), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(0.00), _orderConfirmation.TotalAmountWithTaxes); }
        [TestMethod] public void HasCorrectNumberOfLines() { Assert.AreEqual(4, _orderConfirmation.Items.Count); }

        [TestMethod]
        public void FirstItem()
        {
            var item = _orderConfirmation.Items.ElementAt(0);

            Assert.AreEqual("TR050.0390BQ4000", item.Component.Name);
            Assert.AreEqual("Roller, Inox quadr. fixplate", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("11.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(2), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(1686.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(38.00), item.Discount);
            Assert.AreEqual(new Decimal(2090.64), item.TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            var item = _orderConfirmation.Items.ElementAt(1);

            Assert.AreEqual("UP050.0780, L=1400mm", item.Component.Name);
            Assert.AreEqual("U-profile Inox inwidth = 53", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("11.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1.4), item.Quantity);
            Assert.AreEqual(Unit.Meters, item.Unit);
            Assert.AreEqual(new Decimal(1350.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(38.00), item.Discount);
            Assert.AreEqual(new Decimal(1171.80), item.TotalAmount);
        }

        [TestMethod]
        public void ThirdItem()
        {
            var item = _orderConfirmation.Items.ElementAt(2);

            Assert.AreEqual("SXTE30-825, E=39mm", item.Component.Name);
            Assert.AreEqual("Rostfri Skena", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("11.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(2), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(675.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(38.00), item.Discount);
            Assert.AreEqual(new Decimal(837.00), item.TotalAmount);
        }

        [TestMethod]
        public void FourthItem()
        {
            var item = _orderConfirmation.Items.ElementAt(3);

            Assert.AreEqual("LXWL30-88-2RS", item.Component.Name);
            Assert.AreEqual("Rollco Slider Inox Light", item.Component.Description);
            Assert.IsNull(item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("11.03.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(6), item.Quantity);
            Assert.AreEqual(Unit.Units, item.Unit);
            Assert.AreEqual(new Decimal(1106.00), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(38.00), item.Discount);
            Assert.AreEqual(new Decimal(4114.32), item.TotalAmount);
        }
    }
}
