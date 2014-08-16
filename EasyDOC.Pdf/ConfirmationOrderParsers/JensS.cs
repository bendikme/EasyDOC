using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class JensS : AbstractOrderConfirmationParser
    {
        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }

        private Dictionary<string, List<List<Tuple<string, string>>>> _properties;

        public JensS(string rootDirectory, string filename) : base(rootDirectory, filename) {}
    
        private void InitData() 
        {
            _properties = new Dictionary<string, List<List<Tuple<string, string>>>>
            {
                { PdfConverterRules1, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Dato", ""),
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Kundenr.", "CustomerNumber"),
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Ordrenr.", "OrderNumber"),
                        }
                    }
                },
                { PdfConverterRules2, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Faktura adresse", "InvoiceAddress"),
                            Tuple.Create("Vår ref.", "VendorReference"),
                            Tuple.Create("Levering", "DeliveryMethod"),
                            Tuple.Create("Leveringbetingelser", "DeliveryConditions"),
                            Tuple.Create("Godsmærke", "")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Leveringsadresse", "DeliveryAddress"),
                            Tuple.Create("Deres ref.", "CustomerReference"),
                            Tuple.Create("Deres ordrenr.", "Tag"),
                            Tuple.Create("Deres MVA nr.", ""),
                            Tuple.Create("Betalingsbetingelser", "PaymentConditions")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Ordredato", "OrderDate")
                        }
                    }
                },
                { PdfConverterRules3, new List<List<Tuple<string, string>>>
                    {
                        new List<Tuple<string, string>> {
                            Tuple.Create("Postadresse", "Vendor.PostalAddress")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Leveringsadresse", "Vendor.VisitingAddress")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Telefon", "Vendor.PhoneNumber"),
                            Tuple.Create("Faks", "Vendor.FaxNumber")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Kontonummer", "Vendor.AccountNumber"),
                            Tuple.Create("Foretaksnummer", "Vendor.OrganizationNumber")
                        },
                        new List<Tuple<string, string>> {
                            Tuple.Create("Internett", "Vendor.Link"),
                            Tuple.Create("e-post.", "Vendor.Email")
                        }
                    }
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            order.Vendor.Name = "Jens S. Transmisjoner AS";

            foreach (var table in _properties)
            {
                var properties = ParsePdfColumns(table.Key);
                ExtractFromTableRowDown(table.Value, properties, order);
            }

            var regex = new Regex(@"(\d){6}");
            var filter = new Regex(@"^(\*)+$");

            var itemTable1 = ParsePdfColumns(PdfConverterRules4);
            var itemTable2 = ParsePdfColumns(PdfConverterRules5);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (regex.IsMatch(itemTable1[row][0]))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row + 1][1], "Component.Description", item);

                    SetValueOfProperty(itemTable1[row][2], "DeliveryDate", item);
                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][4], "Unit", item);
                    SetValueOfProperty(itemTable1[row][5], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][6], "Discount", item);
                    SetValueOfProperty(itemTable1[row][7], "TotalAmount", item);

                    var orderSpecifics = "";
                    var firstLine = true;
                    row += 2;

                    while (itemTable1[row][1].Length > 0 && !regex.IsMatch(itemTable1[row][0]))
                    {
                        var line = itemTable2[row][1];
                        if (!filter.IsMatch(line) && !string.IsNullOrWhiteSpace(line))
                        {
                            if (!firstLine) orderSpecifics += "\n";
                            orderSpecifics += line;
                            firstLine = false;
                        }

                        ++row;
                    }

                    SetValueOfProperty(orderSpecifics, "OrderSpecificDescription", item);
                    order.Items.Add(item);
                    --row;
                }
                else if (itemTable1[row][2] == "SUM")
                {
                    SetValueOfProperty(itemTable1[row][7], "TotalAmountWithoutTaxes", order);
                    SetValueOfProperty(itemTable1[row + 3][7], "TotalAmountWithTaxes", order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}