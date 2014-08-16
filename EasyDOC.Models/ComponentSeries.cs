using System.Collections.Generic;
using System.Linq;

namespace EasyDOC.Model
{
    public class ComponentSeries : AbstractComponent
    {
        public ComponentSeries()
        {
            Components = new HashSet<Component>();
        }

        public virtual ICollection<Component> Components { get; set; }

        public override void RemoveAllReferences()
        {
            Components.Clear();
        }

        public override bool CanDelete()
        {
            return !Components.Any();
        }
    }
}