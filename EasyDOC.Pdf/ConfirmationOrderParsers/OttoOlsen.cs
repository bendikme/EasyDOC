using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class OttoOlsen : AbstractOrderConfirmationParser
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

        public OttoOlsen(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            DateFormat = "ddMMyy";

            _properties = new Dictionary<string, List<List<Tuple<string, string>>>>
            {
                { PdfConverterRules1, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Ordrenr.", "OrderNumber"),
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Ordredato", "OrderDate"),
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Kundenr", "CustomerNumber"),
                        }
                    }
                },
                { PdfConverterRules2, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Kunde", "InvoiceAddress"),
                            Tuple.Create("Deres referanse", "CustomerReference"),
                            Tuple.Create("Deres best.nr.", "Tag"),
                            Tuple.Create("Vår referanse", "VendorReference")
                        }
                    }
                },
                { PdfConverterRules3, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Leveringadresse", "DeliveryAddress"),
                            Tuple.Create("Fakturamottaker", ""),
                            Tuple.Create("Bet.betingelser", "PaymentConditions"),
                            Tuple.Create("Lev.betingelser", "DeliveryConditions"),
                            Tuple.Create("Leveringsmåte", "DeliveryMethod")
                        }
                    }
                },
                { PdfConverterRules5, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Totalt eks moms", "TotalAmountWithoutTaxes")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Valuta", "Currency")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Totalt", "TotalAmountWithTaxes")
                        }
                    }
                },
                { PdfConverterRules6, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>>(),
                        new List<Tuple<string, string>>(),
                        new List<Tuple<string, string>> {
                            Tuple.Create("Bank account, Norway:", "Vendor.AccountNumber")
                        }
                    }
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            order.Vendor.Name = "Otto Olsen AS";

            var content = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableRowDown(_properties[PdfConverterRules1], content, order);

            content = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties[PdfConverterRules2], content, order);

            content = ParsePdfColumns(PdfConverterRules3);
            ExtractFromTableRowDown(_properties[PdfConverterRules3], content, order);

            content = ParsePdfColumns(PdfConverterRules5);
            ExtractFromTableRowDown(_properties[PdfConverterRules5], content, order);

            content = ParsePdfColumns(PdfConverterRules6);
            ExtractFromTableRowDown(_properties[PdfConverterRules6], content, order);

            order.Vendor.VisitingAddress = content[2][0] + "\n" + content[4][0];

            content = ParsePdfColumns(PdfConverterRules7);
            order.Vendor.PostalAddress = content[1][0] + "\n" + content[3][0];

            var orgNr = content[5][0];
            order.Vendor.OrganizationNumber = orgNr.Substring(orgNr.IndexOf("NO", StringComparison.Ordinal));

            order.Vendor.Link = content[0][1].Substring("Internet:".Length).Trim();
            order.Vendor.Email = content[2][1].Substring("Email:".Length).Trim();
            order.Vendor.PhoneNumber = content[4][1].Substring("Tlf".Length).Trim();
            order.Vendor.FaxNumber = content[5][1].Substring("Fax".Length).Trim();

            var itemTable1 = ParsePdfColumns(PdfConverterRules4);
            var orderLine = new Regex(@"(\d)+");

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (orderLine.IsMatch(itemTable1[row][0]))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row + 1][1], "Component.Description", item);
                    SetValueOfProperty(itemTable1[row + 2][1], "OrderSpecificDescription", item);
                    SetValueOfProperty(itemTable1[row][2], "DeliveryDate", item);
                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][4], "Unit", item);
                    SetValueOfProperty(itemTable1[row][5], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][6], "TotalAmount", item);

                    row += 2;

                    order.Items.Add(item);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}