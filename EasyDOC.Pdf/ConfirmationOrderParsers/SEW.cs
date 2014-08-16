using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Tuple = System.Tuple;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class SEW : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties6;
        private List<List<Tuple<string, string>>> _properties7;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }
        public string PdfConverterRules6 { get; set; }
        public string PdfConverterRules7 { get; set; }

        public string Filter { get; set; }

        public SEW(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordrenr.", "OrderNumber"),
                    Tuple.Create("Dato", "OrderDate"),
                    Tuple.Create("Kundenr.", "CustomerNumber")
                }
            };

            _properties6 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Deres bestillingsnr.:", "Tag"),
                    Tuple.Create("Deres bestillingsdato", "")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Deres ref:", "CustomerReference")
                }
            };

            _properties7 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Betaling", "PaymentConditions"),
                    Tuple.Create("Leveringsbetingelser", "DeliveryConditions"),
                    Tuple.Create("Godsavsender", ""),
                    Tuple.Create("Forsendelsesinfo", "DeliveryMethod"),
                    Tuple.Create("Sendes til", "DeliveryAddress"),
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Deres ref:", "CustomerReference")
                }
            };

        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            DateFormat = "dd.MM.yyyy";

            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableColumnLeftToRight(_properties1, properties, 0, 0, order);

            order.OrderNumber = order.OrderNumber.Split(' ').First();

            properties = ParsePdfColumns(PdfConverterRules4);
            SetValueOfProperty(properties[1][0], o => o.VendorReference, order);

            properties = ParsePdfColumns(PdfConverterRules6);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 0, 0, order);
            ExtractFromTableColumnLeftToRight(_properties6, properties, 1, 2, order);

            properties = ParsePdfColumns(PdfConverterRules7);
            ExtractFromTableColumnLeftToRight(_properties7, properties, 0, 0, order);

            order.Vendor.Name = "SEW-EURODRIVE A/S";

            var itemTable1 = ParsePdfColumns(PdfConverterRules2);
            var itemTable2 = ParsePdfColumns(PdfConverterRules3);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                int test;

                if (int.TryParse(itemTable1[row][0], out test))
                {
                    var item = CreateOrderConfirmationItem(order);

                    DateTime date;
                    while (!DateTime.TryParse(itemTable1[row][2], out date) && row < itemTable1.Length)
                    {
                        ++row;
                    }

                    var qty = itemTable1[row][1].Split(' ');
                    SetValueOfProperty(qty.First(), i => i.Quantity, item);
                    SetValueOfProperty(qty.Last(), i => i.Unit, item);

                    SetValueOfProperty(itemTable1[row][2], i => i.DeliveryDate, item);
                    SetValueOfProperty(itemTable1[row + 1][1], c => c.Description, item.Component);
                    SetValueOfProperty(itemTable1[row + 2][1], c => c.Name, item.Component);

                    row += 2;
                    var startRow = row;
                    double test2;

                    while (!double.TryParse(itemTable1[row][3], out test2) && !double.TryParse(itemTable1[row][4], out test2) && row < itemTable1.Length)
                    {
                        ++row;
                    }

                    var endRow = row;

                    SetValueOfProperty(itemTable1[row + 2][3], i => i.AmountPerUnit, item);
                    SetValueOfProperty(itemTable1[row + 2][4], i => i.TotalAmount, item);

                    var orderDescription = "";
                    for (var r = startRow; r < endRow && r < itemTable2.Length; ++r)
                    {
                        var title = itemTable2[r][1].Trim();
                        var value = itemTable2[r][2];
                        var end = "";


                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            value = Regex.Match(value, @"\:?\s*(.*)").Groups[1].Value;
                            title += title.EndsWith(":") ? "" : ":";
                            end = "\n";
                        }

                        orderDescription += end + title + " " + value;
                    }

                    SetValueOfProperty(orderDescription.Trim(), i => i.OrderSpecificDescription, item);
                    order.Items.Add(item);
                }
                else if (itemTable1[row][0].StartsWith("Total") && itemTable1[row][1].StartsWith("beløp"))
                {
                    SetValueOfProperty(itemTable1[row][3], o => o.Currency, order);
                    SetValueOfProperty(itemTable1[row][4], o => o.TotalAmountWithoutTaxes, order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

    }
}