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
    public class TestFestoOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "FESTO_Order2160130484_20140324");
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
            Assert.IsInstanceOfType(_parser, typeof(Festo));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("0016139923", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("2160130484", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("00160020", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec A/S\nRakkestadveien 1\n1814 Askim\nNorge", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Post", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.IsNull(_orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryConditions() { Assert.IsNull(_orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("P2013-083/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("30 dager netto", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("24.03.2014"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Ole Deviks vei 2\n0666 Oslo", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 22 72 89 50", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+47 22 72 89 51", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("No 990 922 969 MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.IsNull(_orderConfirmation.Vendor.AccountNumber); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(66885.38), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(0), _orderConfirmation.TotalAmountWithTaxes); }
        [TestMethod] public void HasCorrectNumberOfLines() { Assert.AreEqual(4, _orderConfirmation.Items.Count); }

        [TestMethod]
        public void FirstItem()
        {
            var item = _orderConfirmation.Items.ElementAt(0);

            Assert.AreEqual("530411", item.Component.Name);
            Assert.AreEqual("MPA-FB-VI\nVALVE TERMINAL", item.Component.Description);
            Assert.AreEqual("50E-F38GCQSNMKBNMKB-D+I2G\nZBABHBE\n32P-SGL-R-M3B-JJKS3J", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("01.04.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(new Decimal(16625.29), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(16625.29), item.TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            var item = _orderConfirmation.Items.ElementAt(1);

            Assert.AreEqual("530411", item.Component.Name);
            Assert.AreEqual("MPA-FB-VI\nVALVE TERMINAL", item.Component.Description);
            Assert.AreEqual("50E-F38GCQSNMKBNMKB-D+I2G\nZBABHBE\n32P-SGL-R-M4B-8J", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("01.04.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(new Decimal(18625.87), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(18625.87), item.TotalAmount);
        }

        [TestMethod]
        public void ThirdItem()
        {
            var item = _orderConfirmation.Items.ElementAt(2);

            Assert.AreEqual("530411", item.Component.Name);
            Assert.AreEqual("MPA-FB-VI\nVALVE TERMINAL", item.Component.Description);
            Assert.AreEqual("50E-F38GCQSNMKBNMKB-D+IGZ\nBABHBE\n32P-SGL-R-M3BQZBU-4JLLQBJ\nL", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("01.04.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(new Decimal(20625.88), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(20625.88), item.TotalAmount);
        }

        [TestMethod]
        public void FourthItem()
        {
            var item = _orderConfirmation.Items.ElementAt(3);

            Assert.AreEqual("569926", item.Component.Name);
            Assert.AreEqual("MPAL-VI\nVALVE TERMINAL", item.Component.Description);
            Assert.AreEqual("34P-CX-U4AZ4AZ-5J3K\n50E-F36GCQS-L+IGZBE", item.OrderSpecificDescription);
	    Assert.AreEqual(DateTime.Parse("01.04.2014"), item.DeliveryDate);
            Assert.AreEqual(new Decimal(1), item.Quantity);
            Assert.AreEqual(new Decimal(11008.34), item.AmountPerUnit);
            Assert.AreEqual(new Decimal(0), item.Discount);
            Assert.AreEqual(new Decimal(11008.34), item.TotalAmount);
        }

    }
}
