using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EasyDOC.Model
{
    public class Maintenance : DatabaseObject, IEntity, IHistoryEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string NameSe { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(500)]
        public string DescriptionSe { get; set; }

        public int? ManualId { get; set; }

        [Display(Name = "Page")]
        public int? ManualPage { get; set; }

        public int? VendorId { get; set; }

        public virtual File Image { get; set; }
        public int? ImageId { get; set; }

        [Display(Name = "Day")]
        public bool IntervalDaily { get; set; }

        [Display(Name = "Week")]
        public bool IntervalWeekly { get; set; }

        [Display(Name = "14 days")]
        public bool IntervalWeekly2 { get; set; }

        [Display(Name = "Month")]
        public bool IntervalMonthly { get; set; }

        [Display(Name = "3 months")]
        public bool IntervalMonthly3 { get; set; }

        [Display(Name = "6 months")]
        public bool IntervalHalfYearly { get; set; }

        [Display(Name = "Year")]
        public bool IntervalYearly { get; set; }

        [Display(Name = "> Year")]
        public bool IntervalRarely { get; set; }

        public virtual File Manual { get; set; }
        public virtual Vendor Vendor { get; set; }

        [NotMapped]
        public virtual IEnumerable<bool> Intervals
        {
            get
            {
                return new List<bool>
                {
                    IntervalDaily,
                    IntervalWeekly,
                    IntervalWeekly2,
                    IntervalMonthly,
                    IntervalMonthly3,
                    IntervalHalfYearly,
                    IntervalYearly,
                    IntervalRarely
                };
            }
        }

        public virtual ICollection<ComponentMaintenance> Components { get; set; }
        public virtual ICollection<ProjectMaintenance> Projects { get; set; }

        public override void RemoveAllReferences()
        {
            Components.Clear();
            Projects.Clear();
        }

        public override bool CanDelete()
        {
            return !Components.Any() && !Projects.Any();
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