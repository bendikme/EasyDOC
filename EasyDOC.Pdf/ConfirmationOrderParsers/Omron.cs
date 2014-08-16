using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Tuple = System.Tuple;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Omron : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties3;
        private List<List<Tuple<string, string>>> _properties6;
        private List<List<Tuple<string, string>>> _properties5;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }
        public string PdfConverterRules6 { get; set; }
        public string PdfConverterRules7 { get; set; }

        public string Filter { get; set; }

        public Omron(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kundenr", "CustomerNumber"),
               }
            };

            _properties3 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Org. nr:", "Vendor.OrganizationNumber"),
                    Tuple.Create("Tel:", "Vendor.PhoneNumber"),
                    Tuple.Create("Email:", "Vendor.Email")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Internet:", "Vendor.Link")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("konto nr", "Vendor.AccountNumber")
                }
            };

            _properties5 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Transport måte", "DeliveryMethod"),
                    Tuple.Create("Leveringsbet", "DeliveryConditions")
                }
            };

            _properties6 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Your order n", "Tag")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Order date", "OrderDate")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Acknowledgement n", "OrderNumber")
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties2, properties, order);

            properties = ParsePdfColumns(PdfConverterRules5);
            ExtractFromTableColumnLeftToRight(_properties5, properties, 0, 0, order);

            properties = ParsePdfColumns(PdfConverterRules6);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 0, 0, order);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 1, 2, order);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 2, 4, order);

            properties = ParsePdfColumns(PdfConverterRules3);
            SetValueOfProperty(CombineRowsBetween(0, 4, 0, properties), "Vendor.PostalAddress", order);

            ExtractFromTableColumnLeftToRight(_properties3, properties, 0, order);
            ExtractFromTableColumnLeftToRight(_properties3, properties, 1, order);
            ExtractFromTableColumnLeftToRight(_properties3, properties, 2, order);

            properties = ParsePdfColumns(PdfConverterRules4);
            order.InvoiceAddress = CombineRowsBetween("Faktura adresse", "-----", properties);
            order.DeliveryAddress = CombineRowsBetween("Vareadresse", "-----", properties);

            if (order.Tag != null)
            {
                var name = order.Tag.Split('/').Last();
                SetValueOfProperty(name, "CustomerReference", order);
            }

            order.Vendor.Name = "Omron";

            var itemTable1 = ParsePdfColumns(PdfConverterRules1);

            var position = new Regex(@"\d+\.000");

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (position.IsMatch(itemTable1[row][0]))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row + 1][1], "Component.Description", item);
                    SetValueOfProperty(itemTable1[row + 1][2], "DeliveryDate", item);
                    SetValueOfProperty(itemTable1[row][4], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][5], "Discount", item);
                    SetValueOfProperty(itemTable1[row][7], "TotalAmount", item);

                    var qty = itemTable1[row][3].Split(' ');
                    SetValueOfProperty(qty[0], "Quantity", item);
                    SetValueOfProperty(qty[1], "Unit", item);

                    order.Items.Add(item);
                }
            }

            properties = ParsePdfColumns(PdfConverterRules7);

            for (var row = 0; row < properties.Length; ++row)
            {
                if (properties[row][0].StartsWith("VAT number"))
                {
                    order.PaymentConditions = properties[row + 3][2];
                    order.Currency = properties[row][4];

                    SetValueOfProperty(properties[row + 2][2].Split(' ').First(), "TotalAmountWithoutTaxes", order);
                    SetValueOfProperty(properties[row + 2][6].Split(' ').First(), "TotalAmountWithTaxes", order);

                    break;
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }


    }
}