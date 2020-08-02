using DynamicDbContext.DynamicHelper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.DynamicHelper
{
    public class DynamicCondition
    {
        public WhereOperation WhereOperation { get; set; }
        public string Column { get; set; }
        public object Value { get; set; }
    }
}
