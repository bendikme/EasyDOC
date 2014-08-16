using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public interface IMaintenanceRelation : IIdentifyable
    {
        int MaintenanceId { get; set; }
        string Remarks { get; set; }

        [Display(Name = "Include in manual")]
        bool IncludeInManual { get; set; }

        Maintenance Maintenance { get; set; }
    }
}