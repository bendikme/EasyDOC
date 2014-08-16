using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class OrderConfirmation : DatabaseObject, IEntity, IHistoryEntity
    {
        public OrderConfirmation()
        {
            Items = new HashSet<OrderConfirmationItem>();
        }

        [MaxLength(50)]
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }

        [MaxLength(25)]
        public string Currency { get; set; }

        public int VendorId { get; set; }
        public virtual Vendor Vendor { get; set; }

        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public int? EmployeeId { get; set; }
        public Employee CustomerReference { get; set; }

        public int? FileId { get; set; }
        public File File { get; set; }

        [MaxLength(50)]
        public string VendorReference { get; set; }

        [MaxLength(50)]
        public string CustomerNumber { get; set; }

        [MaxLength(50)]
        public string CustomerOrderNumber { get; set; }

        [MaxLength(100)]
        public string Tag { get; set; }

        [MaxLength(200)]
        public string InvoiceAddress { get; set; }
        [MaxLength(200)]
        public string DeliveryAddress { get; set; }
        [MaxLength(200)]
        public string DeliveryMethod { get; set; }
        [MaxLength(200)]
        public string DeliveryConditions { get; set; }
        [MaxLength(200)]
        public string PaymentMethod { get; set; }
        [MaxLength(200)]
        public string PaymentConditions { get; set; }

        public Decimal TotalAmountWithoutTaxes { get; set; }
        public Decimal TotalAmountWithTaxes { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public virtual ICollection<OrderConfirmationItem> Items { get; set; }

        public override void RemoveAllReferences()
        {
        }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
    }

    public class OrderConfirmationItem : DatabaseObject, IEntity, IHistoryEntity
    {
        public int OrderConfirmationId { get; set; }
        public virtual OrderConfirmation OrderConfirmation { get; set; }

        public int ComponentId { get; set; }
        public virtual Component Component { get; set; } 

        [MaxLength(1000)]
        public string OrderSpecificDescription { get; set; }
        public DateTime DeliveryDate { get; set; }

        public Decimal Quantity { get; set; }
        public Unit Unit { get; set; }

        public Decimal Discount { get; set; }
        public Decimal AmountPerUnit { get; set; }
        public Decimal TotalAmount { get; set; }

        public override void RemoveAllReferences()
        {
            throw new NotImplementedException();
        }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }

        [MaxLength(1000)]
        public string Name { get; set; }
    }
}