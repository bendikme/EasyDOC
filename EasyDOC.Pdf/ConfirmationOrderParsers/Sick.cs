using System;
using System.Collections.Generic;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Sick : AbstractOrderConfirmationParser
    {
        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }
        public string PdfConverterRules6 { get; set; }
        public string PdfConverterRules7 { get; set; }

        private Dictionary<string, List<List<Tuple<string, string>>>> _properties;

        public string Filter { get; set; }

        public Sick(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties = new Dictionary<string, List<List<Tuple<string, string>>>>
            {
                { PdfConverterRules1, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Ordrebekreftelse", "OrderNumber"),
                        }
                    }
                },
                { PdfConverterRules2, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Dato:", "OrderDate"),
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Kundenr.:", "CustomerNumber"),
                        }
                    }
                },
                { PdfConverterRules3, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Deres ref.", "CustomerReference"),
                            Tuple.Create("Selger:", "VendorReference"),
                            Tuple.Create("Referanse.:", "Tag"),
                            Tuple.Create("Leveringsmåte:", "DeliveryMethod"),
                            Tuple.Create("Faktura valuta:", "Currency"),
                            Tuple.Create("Betalingsbet.:", "PaymentConditions"),
                        }
                    }
                },
                { PdfConverterRules4, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Lev. adr.", "DeliveryAddress")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Faktura til:", "InvoiceAddress")
                        }
                    }
                },
                { PdfConverterRules6, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Telefon:", "Vendor.PhoneNumber"),
                            Tuple.Create("Telefax:", "Vendor.FaxNumber"),
                            Tuple.Create("Org. nr.:", "Vendor.OrganizationNumber")
                        }
                    }
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            order.Vendor.Name = "Sick";

            var content = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableColumnLeftToRight(_properties[PdfConverterRules1], content, 0, 0, order);

            content = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties[PdfConverterRules2], content, order);

            content = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableColumnLeftToRight(_properties[PdfConverterRules3], content, 0, 0, order);

            content = ParsePdfColumns(PdfConverterRules4);
            ExtractFromTableRowDown(_properties[PdfConverterRules4], content, order);

            content = ParsePdfColumns(PdfConverterRules5);
            order.Vendor.PostalAddress = content[0][0].Trim() + "\n" + content[1][0].Trim();

            content = ParsePdfColumns(PdfConverterRules6);
            ExtractFromTableColumnLeftToRight(_properties[PdfConverterRules6], content, 0, 0, order);

            content = ParsePdfColumns(PdfConverterRules7);

            var skippingHeader = true;

            for (var row = 0; row < content.Length; ++row)
            {
                if (skippingHeader && content[row][0] == "Produktnr")
                {
                    skippingHeader = false;
                }

                int line;
                if (!skippingHeader && int.TryParse(content[row][0], out line))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(content[row][0], "Component.Name", item);
                    SetValueOfProperty(content[row][1], "Component.Description", item);

                    SetValueOfProperty(content[row + 1][1], "OrderSpecificDescription", item);

                    SetValueOfProperty(content[row][2], "Quantity", item);
                    SetValueOfProperty(content[row][3], "DeliveryDate", item);
                    SetValueOfProperty(content[row][4], "AmountPerUnit", item);
                    SetValueOfProperty(content[row][5], "Discount", item);
                    SetValueOfProperty(content[row][6], "TotalAmount", item);

                    order.Items.Add(item);
                }
                else if (content[row][1] == "Ordresum")
                {
                    SetValueOfProperty(content[row][6], "TotalAmountWithTaxes", order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}