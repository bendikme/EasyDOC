using System;
using System.Collections.Generic;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Pdf.ConfirmationOrderParsers
{
    public class Volta : AbstractOrderConfirmationParser
    {
        private List<List<Tuple<string, string>>> _properties1;
        private List<List<Tuple<string, string>>> _properties2;
        private List<List<Tuple<string, string>>> _properties5;

        public string PdfConverterRules1 { get; set; }
        public string PdfConverterRules2 { get; set; }
        public string PdfConverterRules3 { get; set; }
        public string PdfConverterRules4 { get; set; }
        public string PdfConverterRules5 { get; set; }

        public Volta(string rootDirectory, string filename) : base(rootDirectory, filename) { }

        private void InitData()
        {
            _properties1 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>(),
                new List<Tuple<string, string>>
                {
                    Tuple.Create("ORDER DATE", "OrderDate")
                }
            };

            _properties2 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("WE REFER TO YOUR ORDER", "Tag")
                }
            };

            _properties5 = new List<List<Tuple<string, string>>>
            {
                new List<Tuple<string, string>>
                {
                    Tuple.Create("Phone", "Vendor.PhoneNumber"),
                    Tuple.Create("Telefax", "Vendor.FaxNumber"),
                    Tuple.Create("E-mail", "Vendor.Email"),
                }
            };
        }

        public override OrderConfirmation ExtractOrderConfirmationData(IUnitOfWork uow)
        {
            InitData();
            var order = CreateOrderConfirmation(uow);

            var properties = ParsePdfColumns(PdfConverterRules1);
            ExtractFromTableColumnLeftToRight(_properties1, properties, 1, 1, order);

            var orderNumber = properties[0][0].Substring(0, properties[0][0].IndexOf("ORDER CONFIRMATION", StringComparison.Ordinal));
            SetValueOfProperty(orderNumber.Trim(), "OrderNumber", order);
            SetValueOfProperty(CombineRowsBetween(1, 100, 0, properties), "InvoiceAddress", order);

            properties = ParsePdfColumns(PdfConverterRules2);
            ExtractFromTableColumnLeftToRight(_properties2, properties, 0, 0, order);

            var customerRef = properties[1][0].Substring("For the attention of : ".Length).Trim();
            SetValueOfProperty(customerRef, "CustomerReference", order);

            properties = ParsePdfColumns(PdfConverterRules5);
            ExtractFromTableColumnLeftToRight(_properties5, properties, 0, 1, order);
            SetValueOfProperty(CombineRowsBetween(2, 100, 0, properties), "Vendor.PostalAddress", order);

            var cell = FindCellStartingWith("VAT no.", properties).Split(' ');
            if (cell.Length > 0)
            {
                SetValueOfProperty(cell[cell.Length - 1], "Vendor.OrganizationNumber", order);
            }

            cell = FindCellStartingWith("IBAN", properties).Split(' ');
            if (cell.Length > 0)
            {
                SetValueOfProperty(cell[cell.Length - 1], "Vendor.AccountNumber", order);
            }


            order.Vendor.Name = "Volta Belting Europe BV";

            var itemTable1 = ParsePdfColumns(PdfConverterRules3);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                int test;

                if (int.TryParse(itemTable1[row][0], out test))
                {
                    var item = CreateOrderConfirmationItem(order);

                    SetValueOfProperty(itemTable1[row][1], "Component.Name", item);
                    SetValueOfProperty(itemTable1[row + 1][1], "Component.Description", item);
                    SetValueOfProperty(itemTable1[row][3], "AmountPerUnit", item);
                    SetValueOfProperty(itemTable1[row][4], "TotalAmount", item);
                    SetValueOfProperty(itemTable1[row][5], "DeliveryDate", item);

                    var qty = itemTable1[row][2].Trim().Split(' ');
                    if (qty.Length > 1)
                    {
                        SetValueOfProperty(qty[0], "Quantity", item);
                        SetValueOfProperty(qty[qty.Length - 1], "Unit", item);
                    }


                    order.Items.Add(item);
                }
            }

            itemTable1 = ParsePdfColumns(PdfConverterRules4);

            for (var row = 0; row < itemTable1.Length; ++row)
            {
                if (itemTable1[row][0].Contains("DESPATCHED BY"))
                {
                    var deliveryMethod = itemTable1[row][0].Substring(0, itemTable1[row][0].IndexOf("DESPATCHED BY", StringComparison.Ordinal));
                    SetValueOfProperty(deliveryMethod.Trim(), "DeliveryMethod", order);
                }
                else if (itemTable1[row][0].Contains("TERMS  AND MODE OF PAYMENT"))
                {
                    var paymentConditions = itemTable1[row][0].Substring(0, itemTable1[row][0].IndexOf("TERMS  AND MODE OF PAYMENT", StringComparison.Ordinal));
                    SetValueOfProperty(paymentConditions.Trim(), "PaymentConditions", order);
                }
                else if (itemTable1[row][0].Contains("NAME :"))
                {
                    var vendorReference = itemTable1[row][0].Substring(0, itemTable1[row][0].IndexOf("NAME", StringComparison.Ordinal));
                    SetValueOfProperty(vendorReference.Trim(), "VendorReference", order);
                }
                else if (itemTable1[row][1].StartsWith("Total in"))
                {
                    SetValueOfProperty(itemTable1[row][2], "Currency", order);
                    SetValueOfProperty(itemTable1[row][3], "TotalAmountWithTaxes", order);
                    SetValueOfProperty(itemTable1[row - 3][3], "TotalAmountWithoutTaxes", order);
                }
            }

            uow.OrderConfirmationRepository.Create(ReplaceExistingComponents(order, uow));
            return order;
        }
    }
}