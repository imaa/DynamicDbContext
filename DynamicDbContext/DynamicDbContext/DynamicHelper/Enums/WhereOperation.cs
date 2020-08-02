using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.DynamicHelper.Enums
{
    public enum WhereOperation
    {
        DateEqual,
        NotNull,
        Null,
        NotContains,
        NotEndWith,
        EndWith,
        NotIn,
        In,
        NotBeginsWith,
        BeginsWith,
        GreaterThanOrEqual,
        GreaterThan,
        LessThanOrEqual,
        LessThan,
        Contains,
        NotEqual,
        Equal
    }
}
