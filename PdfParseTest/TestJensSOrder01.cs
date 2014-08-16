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
    public class TestPdfParserOnJensSOrder01
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "0000166317-1");
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
            Assert.IsInstanceOfType(_parser, typeof(JensS));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("52063", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("0000166317-1", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Christian Holmsen", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 Askim", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Tollpost DPD", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void DeliveryConditions() { Assert.AreEqual("FCA Oslo.", _orderConfirmation.DeliveryConditions); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Dynatec A/S\nRakkestadveien 1\n1814 Askim", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-083", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Fri mnd + 30 dager", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("13-02-14"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Postboks 9, Manglerud\n0612 Oslo", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorVisitingAddress() { Assert.AreEqual("Enebakkveien 117\n0680 Oslo", _orderConfirmation.Vendor.VisitingAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("+47 23 06 04 00", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("+47 23 06 04 01", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("8397.10.10447", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NO 982 147 344 MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorLink() { Assert.AreEqual("www.jens-s.no", _orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("post@jens-s.no", _orderConfirmation.Vendor.Email); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("12B1", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual(@"WITRA KJEDE 3/4"" S (12B1)", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.AreEqual(@"2 stk av 79 ledd + 1 halvledd = 1524mm", _orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("20-02-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(3.010), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(Unit.Meters, _orderConfirmation.Items.ElementAt(0).Unit);
            Assert.AreEqual(new Decimal(257.00), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(65), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(270.75), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("12B1-12", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual(@"Halvledd 3/4"" S (12B1)", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.AreEqual("", _orderConfirmation.Items.ElementAt(1).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("20-02-14"), _orderConfirmation.Items.ElementAt(1).DeliveryDate);
            Assert.AreEqual(new Decimal(2), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(1).Unit);
            Assert.AreEqual(new Decimal(81.00), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(65), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(56.70), _orderConfirmation.Items.ElementAt(1).TotalAmount);
        }

        [TestMethod]
        public void EightItem()
        {
            Assert.AreEqual("12B1-12", _orderConfirmation.Items.ElementAt(8).Component.Name);
            Assert.AreEqual(@"Halvledd 3/4"" S (12B1)", _orderConfirmation.Items.ElementAt(8).Component.Description);
            Assert.AreEqual("", _orderConfirmation.Items.ElementAt(8).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("20-02-14"), _orderConfirmation.Items.ElementAt(8).DeliveryDate);
            Assert.AreEqual(new Decimal(4), _orderConfirmation.Items.ElementAt(8).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(8).Unit);
            Assert.AreEqual(new Decimal(81.00), _orderConfirmation.Items.ElementAt(8).AmountPerUnit);
            Assert.AreEqual(new Decimal(65), _orderConfirmation.Items.ElementAt(8).Discount);
            Assert.AreEqual(new Decimal(113.40), _orderConfirmation.Items.ElementAt(8).TotalAmount);
        }

        [TestMethod]
        public void NineItem()
        {
            Assert.AreEqual("12B1-11", _orderConfirmation.Items.ElementAt(9).Component.Name);
            Assert.AreEqual(@"Ledd 3/4"" S (12B1)", _orderConfirmation.Items.ElementAt(9).Component.Description);
            Assert.AreEqual("", _orderConfirmation.Items.ElementAt(9).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("20-02-14"), _orderConfirmation.Items.ElementAt(9).DeliveryDate);
            Assert.AreEqual(new Decimal(18), _orderConfirmation.Items.ElementAt(9).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(9).Unit);
            Assert.AreEqual(new Decimal(35.00), _orderConfirmation.Items.ElementAt(9).AmountPerUnit);
            Assert.AreEqual(new Decimal(65), _orderConfirmation.Items.ElementAt(9).Discount);
            Assert.AreEqual(new Decimal(220.50), _orderConfirmation.Items.ElementAt(9).TotalAmount);
        }

    }
}
