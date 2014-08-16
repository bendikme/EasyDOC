using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDOC.Model
{
    public class Vendor : DatabaseObject, IEntity, IHistoryEntity
    {
        public Vendor()
        {
            Components = new HashSet<AbstractComponent>();
            Maintenances = new HashSet<Maintenance>();
        }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(5)]
        public string ShortName { get; set; }

        [MaxLength(50)]
        public string Link { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string PostalAddress { get; set; }
        [MaxLength(200)]
        public string VisitingAddress { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        [MaxLength(20)]
        public string FaxNumber { get; set; }
        [MaxLength(20)]
        public string AccountNumber { get; set; }
        [MaxLength(20)]
        public string OrganizationNumber { get; set; }

        public virtual ICollection<AbstractComponent> Components { get; set; }
        public virtual ICollection<Maintenance> Maintenances { get; set; }

        public override void RemoveAllReferences()
        {
            Components.Clear();
            Maintenances.Clear();
        }

        public override bool CanDelete()
        {
            return !(Components.Any() || Maintenances.Any());
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
}