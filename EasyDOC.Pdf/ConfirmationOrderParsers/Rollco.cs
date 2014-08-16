using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Tuple = System.Tuple;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Rollco : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties3;
        private List<List<Tuple<string, string>>> _properties5;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }

        public string Filter { get; set; }

        public Rollco(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vårt ordrenr.", "OrderNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kundenummer", "CustomerNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordredato", "OrderDate")
                }
            };

            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Leveringsadresse", "DeliveryAddress")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kundeadresse", "InvoiceAddress")
                }
            };

            _properties3 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Deres referanse", "CustomerReference"),
                    Tuple.Create("Deres ordrenummer", "Tag"),
                    Tuple.Create("Leveringsvillkår", "DeliveryConditions"),
                    Tuple.Create("Leveringsmåte", "DeliveryMethod")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vår referense", "VendorReference"),
                    Tuple.Create("Forsendelsesdato", ""),
                    Tuple.Create("Betalingsbetingelser", "PaymentConditions")
                }
            };

            _properties5 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Adresse", "Vendor.PostalAddress"),
                    Tuple.Create("Organisasjonsnummer", "")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Telefon", "Vendor.PhoneNumber"),
                    Tuple.Create("Fax", "Vendor.FaxNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Momsreg.nr.", "Vendor.OrganizationNumber"),
                    Tuple.Create("Bankkonto", "Vendor.AccountNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Internet", "Vendor.Link"),
                    Tuple.Create("e-post", "Vendor.Email")
                }
            };

        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            DateFormat = "yyyy-MM-dd";

            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableRowDown(_properties1, properties, order);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties2, properties, order);

            properties = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableColumnLeftToRight(_properties3, properties, 0, 0, order);
            ExtractFromTableColumnLeftToRight(_properties3, properties, 1, 2, order);

            var date = properties[1][3];

            properties = ParsePdfColumns(PdfConverterRules5);
            ExtractFromTableRowDown(_properties5, properties, order);

            order.Vendor.Name = "Rollco Norge AS";

            var itemTable = ParsePdfColumns(PdfConverterRules4);

            for (var row = 0; row < itemTable.Length; ++row)
            {
                if (!string.IsNullOrWhiteSpace(itemTable[row][0]) && !string.IsNullOrWhiteSpace(itemTable[row][1]))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable[row][0], c => c.Name, item.Component);
                    SetValueOfProperty(itemTable[row + 1][0], c => c.Description, item.Component);
                    SetValueOfProperty(itemTable[row][1], i => i.Quantity, item);
                    SetValueOfProperty(itemTable[row][2], i => i.Unit, item);
                    SetValueOfProperty(itemTable[row][3], i => i.AmountPerUnit, item);
                    SetValueOfProperty(itemTable[row][5], i => i.TotalAmount, item);
                    SetValueOfProperty(date, i => i.DeliveryDate, item);

                    var discount = itemTable[row][4].Split('%').First();
                    SetValueOfProperty(discount, i => i.Discount, item);

                    order.Items.Add(item);
                }
                else if (itemTable[row][0].StartsWith("Eksklusiv MVA"))
                {
                    SetValueOfProperty(itemTable[row][4], o => o.Currency, order);
                    SetValueOfProperty(itemTable[row][5], o => o.TotalAmountWithoutTaxes, order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

    }
}