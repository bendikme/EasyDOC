using System;
using System.Collections.Generic;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Kulelagerhuset : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties4;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }

        public Kulelagerhuset(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties2 = new List<List<Tuple<string, string>>>
            {
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Ordrenr", "OrderNumber"),
                        Tuple.Create("Ordredato", "OrderDate"),
                        Tuple.Create("Selger", "VendorReference"),
                        Tuple.Create("Deres ref", "CustomerReference"),
                        Tuple.Create("Merkes", "Tag"),
                        Tuple.Create("Transportør", "DeliveryMethod"),
                        Tuple.Create("Betalingsbetingelser", "PaymentConditions"),
                        Tuple.Create("Leveringsmåte", ""),
                        Tuple.Create("Kundenr", "CustomerNumber"),
                    }
            };

            _properties4 = new List<List<Tuple<string, string>>>
            {
                    new List<Tuple<string, string>>(),
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Telefonnr.:", "Vendor.PhoneNumber"),
                        Tuple.Create("Faksnr.:", "Vendor.FaxNumber"),
                        Tuple.Create("E-post:", "Vendor.Email"),
                        Tuple.Create("Hjemmeside:", "Vendor.Link"),
                    },
                    new List<Tuple<string, string>>
                    {
                        Tuple.Create("Org.nr.:", "Vendor.OrganizationNumber"),
                        Tuple.Create("Bank", ""),
                        Tuple.Create("Bankkontonr.:", "Vendor.AccountNumber"),
                    }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules1);
            order.InvoiceAddress = CombineRowsBetween("Leveringsadresse:", "-------", properties);
            order.DeliveryAddress = CombineRowsBetween("Leveringsadresse:", "-------", properties);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableColumnLeftToRight(_properties2, properties, 0, 0, order);

            properties = ParsePdfColumns(PdfConverterRules4);
            ExtractFromTableColumnLeftToRight(_properties4, properties, 1, 1, order);
            ExtractFromTableColumnLeftToRight(_properties4, properties, 2, 3, order);

            order.Currency = "NOK";
            order.Vendor.Name = "Kulelagerhuset AS";
            order.Vendor.PostalAddress = CombineRowsBetween("Kulelagerhuset AS", "-------", properties);

            var itemTable1 = ParsePdfColumns(PdfConverterRules3);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (itemTable1[row][0].Length > 0 || itemTable1[row][1].Length > 0)
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][0], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row][1], "Component.Description", item);
                    SetValueOfProperty(itemTable1[row][2], "DeliveryDate", item);
                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][4], "Unit", item);
                    SetValueOfProperty(itemTable1[row][5], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][6], "Discount", item);
                    SetValueOfProperty(itemTable1[row][7], "TotalAmount", item);

                    order.Items.Add(item);
                }
                else if (itemTable1[row][3].StartsWith(("To")))
                {
                    SetValueOfProperty(itemTable1[row][7], "TotalAmountWithoutTaxes", order);
                    SetValueOfProperty(itemTable1[row + 2][7], "TotalAmountWithTaxes", order);
                    break;
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}