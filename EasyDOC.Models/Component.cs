using System.Collections.Generic;

namespace EasyDOC.Model
{
    public class Component : AbstractComponent
    {
        public Component()
        {
            OrderConfirmationItems = new HashSet<OrderConfirmationItem>();
        }

        public virtual HashSet<OrderConfirmationItem> OrderConfirmationItems { get; set; }

        public int? SeriesId { get; set; }
        public virtual ComponentSeries Series { get; set; }
    }
}