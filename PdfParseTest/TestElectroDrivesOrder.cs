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
    public class TestElectroDrivesOrder
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

            _project = _uow.ProjectRepository.Get(p => p.ProjectNumber == "P2013-080").FirstOrDefault();
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "Ordrebekreftelse __115723");
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
            Assert.IsInstanceOfType(_parser, typeof(ElectroDrives));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("30 408", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("115 723", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Øivind Jensen", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\nNO-1814 ASKIM", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Bedriftspakke", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Rakkestadveien 1\nNO-1814 ASKIM", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("FCA Lager Askim", _orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.IsNull(_orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Netto 30 dager", _orderConfirmation.PaymentConditions); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Rakkestadveien 11\nNO-1814 ASKIM", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("(+47) 46 82 75 80", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("(+47) 69 79 35 10", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("976 754 212MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("6550 05 16482", _orderConfirmation.Vendor.AccountNumber); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(9995.23), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(12494.04), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("190558", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("GN352-50-20-M10-S-55", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(new Decimal(1.00), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(new Decimal(18.90), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(18.90), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("190547", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual("GN352-30-25-M8-S-55", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.AreEqual(new Decimal(4.00), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(new Decimal(10.90), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(43.60), _orderConfirmation.Items.ElementAt(1).TotalAmount);
        }

        [TestMethod]
        public void FifthItem()
        {
            Assert.AreEqual("105364", _orderConfirmation.Items.ElementAt(4).Component.Name);
            Assert.AreEqual("GN707 30", _orderConfirmation.Items.ElementAt(4).Component.Description);
            Assert.AreEqual("REF: 2013-080/KOH", _orderConfirmation.Items.ElementAt(4).OrderSpecificDescription);
            Assert.AreEqual(new Decimal(20.00), _orderConfirmation.Items.ElementAt(4).Quantity);
            Assert.AreEqual(new Decimal(89.20), _orderConfirmation.Items.ElementAt(4).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(4).Discount);
            Assert.AreEqual(new Decimal(1784.00), _orderConfirmation.Items.ElementAt(4).TotalAmount);
        }

        [TestMethod]
        public void EightItem()
        {
            Assert.AreEqual("20622356-SNR", _orderConfirmation.Items.ElementAt(7).Component.Name);
            Assert.AreEqual("EXT.208", _orderConfirmation.Items.ElementAt(7).Component.Description);
            Assert.AreEqual(new Decimal(12.00), _orderConfirmation.Items.ElementAt(7).Quantity);
            Assert.AreEqual(new Decimal(1121.99), _orderConfirmation.Items.ElementAt(7).AmountPerUnit);
            Assert.AreEqual(new Decimal(73.00), _orderConfirmation.Items.ElementAt(7).Discount);
            Assert.AreEqual(new Decimal(3635.25), _orderConfirmation.Items.ElementAt(7).TotalAmount);
        }
    }
}
