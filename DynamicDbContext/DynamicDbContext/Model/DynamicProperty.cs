using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Model
{
    public class DynamicProperty
    {
        public string PrropertyName { get; set; }
        public Type PropertyType { get; set; }
        public bool IsForignKey { get; set; }
        public string ForignKey { get; set; }
        public int MaxStringSize { get; set; }
        public bool IsVirtual { get; set; }
    }
}