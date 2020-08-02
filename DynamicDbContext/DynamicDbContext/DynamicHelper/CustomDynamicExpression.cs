using DynamicDbContext.DynamicHelper.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.DynamicHelper
{
     
    public class CustomDynamicExpression
    {
        //By Default DynamicExprression And DynamicExprression
        public CustomDynamicExpression()
        {
            DynamicCondition = new DynamicCondition();
            DynamicExpressions = new List<CustomDynamicExpression>();
            Operator = null;
        }
        public DynamicCondition DynamicCondition { get; set; }
        public Operator? Operator { get; set; }
        public List<CustomDynamicExpression> DynamicExpressions { get; set; }

    }
}
