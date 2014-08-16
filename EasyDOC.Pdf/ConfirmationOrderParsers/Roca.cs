using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using Tuple = System.Tuple;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Roca : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties4;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }

        public string Filter { get; set; }

        public Roca(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Dato", "OrderDate")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Kundenr", "CustomerNumber")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("OWebbordrenr", "")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Ordrenr", "OrderNumber")
                }
            };

            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Vår ref.", "VendorReference"),
                    Tuple.Create("Leveringsadresse", "DeliveryAddress"),
                    Tuple.Create("Lev.betingelser", "DeliveryConditions"),
                    Tuple.Create("Bet.betingelser", "PaymentConditions")
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Deres ref. / Deres ordrenr.", "Tag"),
                    Tuple.Create("Køpere", "InvoiceAddress"),
                    Tuple.Create("Leveringsform", "DeliveryMethod"),
                    Tuple.Create("Anmerkning", "")
                }
            };

            _properties4 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>(),
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Telefon", "Vendor.PhoneNumber"),
                    Tuple.Create("Faks", "Vendor.FaxNumber"),
                },
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Org.nr", "Vendor.OrganizationNumber")
                }
            };

        }

        public DateTime GetLastOccurenceOfDay(DateTime value, DayOfWeek dayOfWeek)
        {
            var daysToAdd = dayOfWeek - value.DayOfWeek;
            if (daysToAdd < 1)
            {
                daysToAdd -= 7;
            }
            return value.AddDays(daysToAdd);
        }

        public DateTime GetFirstDayOfWeek(int year, int weekNumber, DayOfWeek dayOfWeek)
        {
            return GetLastOccurenceOfDay(new DateTime(year, 1, 1).AddDays(7 * weekNumber), dayOfWeek);
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            DateFormat = "yy-MM-dd";
            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableRowDown(_properties1, properties, order);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableRowDown(_properties2, properties, order);

            var tag = order.Tag.Split('\n');
            if (tag.Length > 1)
            {
                SetValueOfProperty(tag.First(), o => o.CustomerReference, order);
                SetValueOfProperty(tag.Last(), o => o.Tag, order);
            }

            order.Vendor.Name = "Roca Industry AB";

            properties = ParsePdfColumns(PdfConverterRules4);
            SetValueOfProperty(CombineRowsBetween(0, 100, 0, properties), o => o.PostalAddress, order.Vendor);

            ExtractFromTableColumnLeftToRight(_properties4, properties, 1, order);
            ExtractFromTableColumnLeftToRight(_properties4, properties, 2, order);

            order.Vendor.Email = properties[2][1];
            order.Vendor.Link = properties[3][1];
            order.Vendor.AccountNumber = properties[2][2];

            var itemTable = ParsePdfColumns(PdfConverterRules3);

            for (var row = 0; row < itemTable.Length; ++row)
            {
                int test;

                if (int.TryParse(itemTable[row][0], out test))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable[row][0], c => c.Name, item.Component);
                    SetValueOfProperty(itemTable[row][1] + "\n" + itemTable[row + 1][1], c => c.Description, item.Component);
                    SetValueOfProperty(itemTable[row][2], i => i.Quantity, item);

                    var date = itemTable[row][3];
                    var year = int.Parse(date.Substring(0, 2)) + 2000;
                    var week = int.Parse(date.Substring(2));
                    item.DeliveryDate = GetFirstDayOfWeek(year, week, DayOfWeek.Monday);

                    SetValueOfProperty(itemTable[row][4], i => i.AmountPerUnit, item);
                    SetValueOfProperty(itemTable[row][5], i => i.Discount, item);
                    SetValueOfProperty(itemTable[row][6], i => i.TotalAmount, item);

                    item.Unit = Unit.Units;
                    order.Items.Add(item);
                }
                else if (string.IsNullOrWhiteSpace(itemTable[row][0]) && itemTable[row][1].StartsWith("Sum exkl. mva."))
                {
                    SetValueOfProperty(itemTable[row][4], o => o.Currency, order);
                    SetValueOfProperty(itemTable[row][6], o => o.TotalAmountWithoutTaxes, order);

                    decimal mva;
                    decimal.TryParse(itemTable[row + 1][4].Split(' ').First(), out mva);

                    order.TotalAmountWithTaxes = order.TotalAmountWithoutTaxes * (1 + (mva / 100));
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }

    }
}