using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Festo : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties4;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }

        public Festo(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kundenummer", "CustomerNumber"),
                    Tuple.Create("Leveringsmåte", "DeliveryMethod"),
                    Tuple.Create("Betalingsbetingelser", "PaymentConditions")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordrenummer", "OrderNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Salgsansvarlig", "VendorReference")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Leverandørnummer", ""),
                    Tuple.Create("Valuta", "Currency")
                }
            };

            _properties4 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Foretaksregisteret", "Vendor.OrganizationNumber"),
                    Tuple.Create("Festo AS", "")
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules5);
            SetValueOfProperty(CombineRowsBetween(0, 100, 0, properties), "InvoiceAddress", order);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties2, properties, order);

            properties = ParsePdfColumns(PdfConverterRules4);
            ExtractFromTableRowDown(_properties4, properties, order);

            var phone = FindCellStartingWith("Tel.:", properties);
            var fax = FindCellStartingWith("Fax:", properties);

            SetValueOfProperty(phone.Split(':')[1].Trim(), "Vendor.PhoneNumber", order);
            SetValueOfProperty(fax.Split(':')[1].Trim(), "Vendor.FaxNumber", order);

            properties = ParsePdfColumns(PdfConverterRules3);
            var result = Regex.Match(properties[0][0], @"Deres ordre (.*?) av (\d{4}-\d{2}-\d{2})");
            if (result.Success)
            {
                SetValueOfProperty(result.Groups[1].Value, "Tag", order);
                SetValueOfProperty(result.Groups[2].Value, "OrderDate", order);
            }

            if (order.Tag != null)
            {
                var initials = order.Tag.Split('/').Last();
                SetValueOfProperty(initials, "CustomerReference", order);
            }

            order.Vendor.Name = "Festo AS";

            var itemTable1 = ParsePdfColumns(PdfConverterRules1);

            var position = new Regex(@"Pos \d\d\d\d");
            var delivery = new[] { "Leverings dato", "24h Service" };

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (position.IsMatch(itemTable1[row][0]))
                {
                    var item = CreateOrderConfirmationItem(order);
                    ++row;

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row][2], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][3], "Quantity", item);
                    SetValueOfProperty(itemTable1[row][4], "TotalAmount", item);

                    var description = itemTable1[row][0];
                    var orderSpecificDescription = "";

                    var count = 1;
                    ++row;

                    while (!position.IsMatch(itemTable1[row][0]) && row < itemTable1.Length)
                    {
                        var line = itemTable1[row][0];

                        if (delivery.Any(line.Contains))
                        {
                            if (line.StartsWith("Leverings dato"))
                            {
                                SetValueOfProperty(line.Split(':')[1], "DeliveryDate", item);
                            }
                            else
                            {
                                item.DeliveryDate = order.OrderDate.AddHours(24);
                                orderSpecificDescription = (orderSpecificDescription + "\n" + line).Trim();
                            }

                            break;
                        }

                        if (count > 1)
                        {
                            orderSpecificDescription = (orderSpecificDescription + "\n" + line).Trim();
                        }
                        else
                        {
                            description = (description + "\n" + line).Trim();
                        }

                        ++count;
                        ++row;
                    }

                    item.OrderSpecificDescription = orderSpecificDescription;
                    item.Component.Description = description;

                    --row;

                    item.Unit = Unit.Units;
                    order.Items.Add(item);
                }
                else if (itemTable1[row][0].StartsWith("Totalt exkl. MVA"))
                {
                    SetValueOfProperty(itemTable1[row][4], "TotalAmountWithoutTaxes", order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}