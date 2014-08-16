using System;
using System.Collections.Generic;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Tuple = System.Tuple;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Lesjofors : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties3;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }

        public string Filter { get; set; }

        public Lesjofors(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vårt momsreg nr", "Vendor.OrganizationNumber"),
                    Tuple.Create("Leveringsadresse", "DeliveryAddress"),
                    Tuple.Create("Betaler", "CustomerNumber"),
                    Tuple.Create("Deres referanse", "CustomerReference"),
                    Tuple.Create("Godsmerke", "")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vår referanse", "VendorReference"),
                    Tuple.Create("Selger", "")
                }
            };

            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kunde", "InvoiceAddress"),
                    Tuple.Create("Betalingsvilkår", "PaymentConditions"),
                    Tuple.Create("Leveringsbetingelse", "DeliveryConditions"),
                    Tuple.Create("Leveringsmåte", "DeliveryMethod")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Valuta", "Currency"),
                    Tuple.Create("Kontantrabatt", ""),
                }
            };

            _properties3 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Dato", ""),
                    Tuple.Create("Kundenr", "")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordrenr", "OrderNumber"),
                    Tuple.Create("Deres ordrenr", "Tag")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordre dato", "OrderDate"),
                    Tuple.Create("Deres d", "")
                }
            };

        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            DateFormat = "dd-MM-yy";

            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableRowDown(_properties1, properties, order);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties2, properties, order);

            properties = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableRowDown(_properties3, properties, order);

            order.Vendor.Name = "Lesjöfors A/S";

            var itemTable = ParsePdfColumns(PdfConverterRules4);

            for (var row = 0; row < itemTable.Length; ++row)
            {
                int test;

                if (int.TryParse(itemTable[row][0], out test))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable[row][1], c => c.Name, item.Component);
                    SetValueOfProperty(itemTable[row + 1][1], c => c.Description, item.Component);

                    SetValueOfProperty(itemTable[row][2], i => i.Quantity, item);
                    SetValueOfProperty(itemTable[row][3], i => i.Unit, item);
                    SetValueOfProperty(itemTable[row + 1][4], i => i.DeliveryDate, item);
                    SetValueOfProperty(itemTable[row][5], i => i.AmountPerUnit, item);
                    SetValueOfProperty(itemTable[row + 1][6], i => i.TotalAmount, item);

                    if (!int.TryParse(itemTable[row + 2][0], out test) && itemTable[row + 2][1].Length > 0)
                    {
                        item.Component.Description += "\n" + itemTable[row + 2][1];
                    }

                    order.Items.Add(item);
                }
                else if (itemTable[row][4].StartsWith("MVA-grunn"))
                {
                    SetValueOfProperty(itemTable[row][6], o => o.TotalAmountWithoutTaxes, order);
                    SetValueOfProperty(itemTable[row + 4][6], o => o.TotalAmountWithTaxes, order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

    }
}