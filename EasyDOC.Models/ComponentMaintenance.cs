using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ComponentMaintenance : IMaintenanceRelation, IHistoryEntity
    {
        [Key]
        [Column(Order = 0)]
        public int ComponentId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int MaintenanceId { get; set; }

        public string Remarks { get; set; }
        public bool IncludeInManual { get; set; }

        public virtual AbstractComponent Component { get; set; }
        public virtual Maintenance Maintenance { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
        public string GetId()
        {
            return string.Format("ComponentId:{0}:::MaintenanceId:{1}", ComponentId, MaintenanceId);
        }
    }
}