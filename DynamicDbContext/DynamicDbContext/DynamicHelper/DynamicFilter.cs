

using DynamicDbContext.DynamicHelper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.DynamicHelper
{
    public class DynamicFilter
    {
        public List<CustomDynamicExpression> CustomDynamicExpression { get; set; }
        public string OrderBy { get; set; }
        public SortDirection? SortDirection { get; set; }
        public DynamicFilter()
        {
            CustomDynamicExpression = new List<CustomDynamicExpression>();
        }
    }
}
