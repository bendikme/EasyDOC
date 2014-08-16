using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class EmployeeTask
    {
        [Key]
        [Column(Order = 1)]
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }

        [Key]
        [Column(Order = 2)]
        public int TaskId { get; set; }
        public virtual Task Task { get; set; }
    }
}