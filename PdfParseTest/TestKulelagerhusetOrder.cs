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
    public class TestKulelagerhusetOrder
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
            _relation = _project.Files.FirstOrDefault(f => f.File.Name == "Ordrebekreftelse58529");
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
            Assert.IsInstanceOfType(_parser, typeof(Kulelagerhuset));
        }

        [TestMethod] public void CustomerNumber() { Assert.AreEqual("2925", _orderConfirmation.CustomerNumber); }
        [TestMethod] public void OrderNumber() { Assert.AreEqual("58529", _orderConfirmation.OrderNumber); }
        [TestMethod] public void VendorReference() { Assert.AreEqual("Thomas Fredbo", _orderConfirmation.VendorReference); }
        [TestMethod] public void InvoiceAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 Askim", _orderConfirmation.InvoiceAddress); }
        [TestMethod] public void DeliveryMethod() { Assert.AreEqual("Bedriftpakke Dør-Dør", _orderConfirmation.DeliveryMethod); }
        [TestMethod] public void Currency() { Assert.AreEqual("NOK", _orderConfirmation.Currency); }
        [TestMethod] public void DeliveryAddress() { Assert.AreEqual("Dynatec AS\nRakkestadveien 1\n1814 Askim", _orderConfirmation.DeliveryAddress); }
        [TestMethod] public void CustomerReference() { Assert.AreEqual(_orderConfirmation.CustomerReference.Name, "Klaus Ole Herland"); }
        [TestMethod] public void Tag() { Assert.AreEqual("2013-078/KOH", _orderConfirmation.Tag); }
        [TestMethod] public void PaymentConditions() { Assert.AreEqual("Netto pr. 30 dager", _orderConfirmation.PaymentConditions); }
        [TestMethod] public void OrderDate() { Assert.AreEqual(DateTime.Parse("21.02.2014"), _orderConfirmation.OrderDate); }

        [TestMethod] public void VendorPostalAddress() { Assert.AreEqual("Teglverksveien 9 B\nLIER 3413", _orderConfirmation.Vendor.PostalAddress); }
        [TestMethod] public void VendorPhoneNumber() { Assert.AreEqual("32244180", _orderConfirmation.Vendor.PhoneNumber); }
        [TestMethod] public void VendorFaxNumber() { Assert.AreEqual("32244195", _orderConfirmation.Vendor.FaxNumber); }
        [TestMethod] public void VendorOrganizationNumber() { Assert.AreEqual("NO980059944MVA", _orderConfirmation.Vendor.OrganizationNumber); }
        [TestMethod] public void VendorAccountNumber() { Assert.AreEqual("5140 05 56931", _orderConfirmation.Vendor.AccountNumber); }
        [TestMethod] public void VendorLink() { Assert.AreEqual("www.kule-as.no", _orderConfirmation.Vendor.Link); }
        [TestMethod] public void VendorEmail() { Assert.AreEqual("postmaster@kule-as.no", _orderConfirmation.Vendor.Email); }

        [TestMethod] public void TotalWithoutTaxes() { Assert.AreEqual(new Decimal(528.00), _orderConfirmation.TotalAmountWithoutTaxes); }
        [TestMethod] public void TotalWithTaxes() { Assert.AreEqual(new Decimal(660.00), _orderConfirmation.TotalAmountWithTaxes); }

        [TestMethod]
        public void FirstItem()
        {
            Assert.AreEqual("KH 22207 W33", _orderConfirmation.Items.ElementAt(0).Component.Name);
            Assert.AreEqual("Rullelager", _orderConfirmation.Items.ElementAt(0).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(0).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("27-02-14"), _orderConfirmation.Items.ElementAt(0).DeliveryDate);
            Assert.AreEqual(new Decimal(4), _orderConfirmation.Items.ElementAt(0).Quantity);
            Assert.AreEqual(new Decimal(132.00), _orderConfirmation.Items.ElementAt(0).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(0).Discount);
            Assert.AreEqual(new Decimal(528.00), _orderConfirmation.Items.ElementAt(0).TotalAmount);
            Assert.AreEqual(Unit.Units, _orderConfirmation.Items.ElementAt(0).Unit);
        }

        [TestMethod]
        public void SecondItem()
        {
            Assert.AreEqual("", _orderConfirmation.Items.ElementAt(1).Component.Name);
            Assert.AreEqual("Frakt", _orderConfirmation.Items.ElementAt(1).Component.Description);
            Assert.IsNull(_orderConfirmation.Items.ElementAt(1).OrderSpecificDescription);
            Assert.AreEqual(DateTime.Parse("27-02-14"), _orderConfirmation.Items.ElementAt(1).DeliveryDate);
            Assert.AreEqual(new Decimal(1), _orderConfirmation.Items.ElementAt(1).Quantity);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).AmountPerUnit);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).Discount);
            Assert.AreEqual(new Decimal(0), _orderConfirmation.Items.ElementAt(1).TotalAmount);
            Assert.AreEqual(Unit.Other, _orderConfirmation.Items.ElementAt(1).Unit);
        }
    }
}
