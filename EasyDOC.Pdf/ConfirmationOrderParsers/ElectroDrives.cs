using System;
using System.Collections.Generic;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class ElectroDrives : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties3;
        private List<List<Tuple<string, string>>> _properties6;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }
        public string PdfConverterRules6 { get; set; }
        public string PdfConverterRules7 { get; set; }

        public ElectroDrives(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vår ref.", "VendorReference"),
                    Tuple.Create("Deres ref.", "CustomerReference"),
                }
            };

            _properties3 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordrenr.", "OrderNumber"),
                    Tuple.Create("Kundenr.", "CustomerNumber"),
                    Tuple.Create("Leveringsform", "DeliveryMethod"),
                    Tuple.Create("Lev.betingelser", "DeliveryConditions"),
                    Tuple.Create("Valuta", "Currency"),
                    Tuple.Create("Ordredato", "OrderDate"),
                    Tuple.Create("Bet. betingelser", "PaymentConditions")
                }
            };

            _properties6 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Telefon", "Vendor.PhoneNumber"),
                    Tuple.Create("Telefaks", "Vendor.FaxNumber"),
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Bank", "Vendor.AccountNumber"),
                    Tuple.Create("Org.nr", "Vendor.OrganizationNumber"),
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableColumnLeftToRight(_properties1, properties, 0, 0, order);

            properties = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableColumnLeftToRight(_properties3, properties, 0, 0, order);

            properties = ParsePdfColumns(PdfConverterRules6);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 0, 0, order);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 1, 2, order);

            properties = ParsePdfColumns(PdfConverterRules2);
            order.DeliveryAddress = CombineRowsBetween("Leveringsadresse", "-------", properties);
            order.InvoiceAddress = properties[0][0] + "\n" + properties[1][0] + "\n" + properties[2][0];

            properties = ParsePdfColumns(PdfConverterRules5);
            order.Vendor.PostalAddress = properties[0][0] + "\n" + properties[1][0];

            order.Vendor.Name = "ElectroDrives AS";

            var itemTable1 = ParsePdfColumns(PdfConverterRules4);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (itemTable1[row][0].Length > 0 && itemTable1[row][7].Length > 0)
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][0], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row][1], "Component.Description", item);
                    SetValueOfProperty(itemTable1[row][2], "DeliveryDate", item);
                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][4], "AmountPerUnit", item);

                    var discount = itemTable1[row][5];
                    if (discount.Length > 1)
                    {
                        discount = discount.Substring(0, discount.Length - 1);
                        SetValueOfProperty(discount, "Discount", item);
                    }

                    SetValueOfProperty(itemTable1[row][7], "TotalAmount", item);

                    if (row < itemTable1.Length - 1 && itemTable1[row + 1][0].Length == 0 && itemTable1[row + 1][1].Length > 1)
                    {
                        SetValueOfProperty(itemTable1[row + 1][1], "OrderSpecificDescription", item);
                    }

                    order.Items.Add(item);
                }
                else if (itemTable1[row][0].StartsWith("Mva-grunnlag"))
                {
                    SetValueOfProperty(itemTable1[row + 1][1], "TotalAmountWithoutTaxes", order);
                }
                else if (itemTable1[row][7].StartsWith(("Sum")))
                {
                    SetValueOfProperty(itemTable1[row + 1][itemTable1[row + 1].Length - 1], "TotalAmountWithTaxes", order);
                    break;
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}