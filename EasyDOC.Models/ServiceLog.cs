using System;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class ServiceLog : DatabaseObject, IEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }

        public int ServiceEngineerId { get; set; }
        public DateTime Date { get; set; }

        [MaxLength(8000)]
        public string Log { get; set; }
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public override void RemoveAllReferences()
        {
            
        }
    }
}