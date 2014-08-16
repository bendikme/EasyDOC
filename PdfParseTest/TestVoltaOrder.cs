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
    public class TestVoltaOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "dynatec 2013-078");
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
            Assert.IsInstanceOfType(_parser, typeof(Volta));
        }

        [TestMethod] public void CustomerNumber() { Assert.IsNull(_orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("221401233", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Robin Koning", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("DYNATEC\nRakkestadveien 1\nN-1814 Askim\n1814 NORWAY", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Land", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("EURO", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.IsNull(_orderConfirmation.DeliveryAddress); }
        [TestMethod] public void DeliveryConditions() { Assert.IsNull(_orderConfirmation.DeliveryConditions); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-078", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("30 days from invoice", _orderConfirmation.PaymentConditions); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("IJzersteden 18\nEnschede 7547 TB\nThe Netherlands", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("(+31) 546 580 166", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("(+31) 546 579 508", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NL819559428B01", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("NL74INGB0660243350", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorLink() { Assert.IsNull(_orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("europe@voltabelting.com", _orderConfirmation.Vendor.Email); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(708.13), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(708.13), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("ZZFMBSD30", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("FMB-3 SD / 300mm(w) x 8.518mm(l) ENDLESS", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("25-02-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(1.0), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(0).Unit);
            Assert.AreEqual(new Decimal(529.07), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(529.07), _orderConfirmation.Items.ElementAt(0).TotalAmount);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("8162514HM", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual(")SD DRIVE  H/M 150mm 12T (40mm", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(1).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("25-02-14"), _orderConfirmation.Items.ElementAt(1).DeliveryDate);
            Assert.AreEqual(new Decimal(1.0), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(1).Unit);
            Assert.AreEqual(new Decimal(96.66), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(96.66), _orderConfirmation.Items.ElementAt(1).TotalAmount);
        }

        [TestMethod]
        public void ThirdItem()
        {
            Assert.AreEqual("8162542", _orderConfirmation.Items.ElementAt(2).Component.Name);
            Assert.AreEqual(")SD TAIL 150mm (40mm", _orderConfirmation.Items.ElementAt(2).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(2).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("25-02-14"), _orderConfirmation.Items.ElementAt(2).DeliveryDate);
            Assert.AreEqual(new Decimal(1.0), _orderConfirmation.Items.ElementAt(2).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(2).Unit);
            Assert.AreEqual(new Decimal(82.40), _orderConfirmation.Items.ElementAt(2).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(2).Discount);
            Assert.AreEqual(new Decimal(82.40), _orderConfirmation.Items.ElementAt(2).TotalAmount);
        }

        [TestMethod]
        public void FourthItem()
        {
            Assert.AreEqual("0060", _orderConfirmation.Items.ElementAt(3).Component.Name);
            Assert.AreEqual("Transport costs to be advised", _orderConfirmation.Items.ElementAt(3).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(3).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("25-02-14"), _orderConfirmation.Items.ElementAt(3).DeliveryDate);
            Assert.AreEqual(new Decimal(1.0), _orderConfirmation.Items.ElementAt(3).Quantity);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(3).Unit);
            Assert.AreEqual(new Decimal(0.00), _orderConfirmation.Items.ElementAt(3).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(3).Discount);
            Assert.AreEqual(new Decimal(0.00), _orderConfirmation.Items.ElementAt(3).TotalAmount);
        }
    }
}
