using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Boxon : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties4;
        private List<List<Tuple<string, string>>> _properties2;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }

        public Boxon(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties4 = new List<List<Tuple<string, string>>>
            {
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Ordrenummer:", "OrderNumber"),
                        Tuple.Create("Ordredato:", "OrderDate"),
                        Tuple.Create("Valuta:", "Currency"),
                        Tuple.Create("Vår ref:", "VendorReference"),
                        Tuple.Create("Betaling:", "PaymentConditions"),
                        Tuple.Create("Fraktmåte:", "DeliveryMethod"),
                        Tuple.Create("Best. ref:", "Tag"),
                    }
            };

            _properties2 = new List<List<Tuple<string, string>>>
            {
                    new List<Tuple<string, string>>(),
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Tlf.", "Vendor.PhoneNumber"),
                        Tuple.Create("Fax", "Vendor.FaxNumber"),
                        Tuple.Create("E-post", "Vendor.Email"),
                        Tuple.Create("Internett", "Vendor.Link"),
                    },
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Foretaksnr.", "Vendor.OrganizationNumber"),
                        Tuple.Create("Bankkonto", "Vendor.AccountNumber"),
                    }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules1);

            var customerNumberRow = FindCellStartingWith("Bestilt av", properties);
            order.CustomerNumber = Regex.Match(customerNumberRow, @"(\d+)").Groups[1].Value;

            order.InvoiceAddress = CombineRowsBetween("Bestilt av", "Bestiller", properties);
            order.DeliveryAddress = CombineRowsBetween("Leveres", "Att", properties);

            var customer = FindCellStartingWith("Bestiller:", properties);
            if (customer.StartsWith("Bestiller:"))
            {
                SetValueOfProperty(customer.Substring("Bestiller:".Length), "CustomerReference", order);
            }

            properties = ParsePdfColumns(PdfConverterRules4);
            ExtractFromTableColumnLeftToRight(_properties4, properties, 0, 0, order);

            properties = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableColumnLeftToRight(_properties2, properties, 1, 1, order);
            ExtractFromTableColumnLeftToRight(_properties2, properties, 2, 3, order);

            order.Vendor.Name = "Boxon AS";
            order.Vendor.PostalAddress = CombineRowsBetween("Boxon AS", "-------", properties);

            var orderLine = new Regex(@"(\d)+");
            var date = new Regex(@"((\d){2}\.(\d){2}\.(\d){4})");

            var itemTable1 = ParsePdfColumns(PdfConverterRules2);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (orderLine.IsMatch(itemTable1[row][0]))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);

                    var description2 = itemTable1[row + 1][2];
                    SetValueOfProperty((itemTable1[row][2] + " " + description2).Trim(), "Component.Description", item);

                    var rowOffset = 0;
                    var deliveryDate = date.Match(itemTable1[row + 2][2]).Groups[1].Value;
                    if (string.IsNullOrEmpty(deliveryDate))
                    {
                        deliveryDate = date.Match(itemTable1[row + 3][2]).Groups[1].Value;
                        rowOffset = 1;
                    }

                    SetValueOfProperty(deliveryDate, "DeliveryDate", item);

                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][5], "Unit", item);

                    var amountPerUnit = itemTable1[row + 1 + rowOffset][6];
                    if (amountPerUnit.Length == 0)
                    {
                        SetValueOfProperty(itemTable1[row][6], "AmountPerUnit", item);
                    }
                    else
                    {
                        SetValueOfProperty(itemTable1[row + 1 + rowOffset][6], "AmountPerUnit", item);
                        SetValueOfProperty(itemTable1[row + 2 + rowOffset][6], "Discount", item);
                    }

                    SetValueOfProperty(itemTable1[row][8], "TotalAmount", item);

                    row += 2;

                    order.Items.Add(item);

                }
                else if (itemTable1[row][4].StartsWith(("Sum")))
                {
                    SetValueOfProperty(itemTable1[row][8], "TotalAmountWithoutTaxes", order);
                    SetValueOfProperty(itemTable1[row + 2][8], "TotalAmountWithTaxes", order);
                    break;
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

    }
}