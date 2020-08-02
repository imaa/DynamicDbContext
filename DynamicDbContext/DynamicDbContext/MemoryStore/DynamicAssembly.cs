using DynamicDbContext.Builders;
using DynamicDbContext.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.MemoryStore
{
    public static class DynamicAssembly
    {
        public static bool AllowAccessContext = false; 
        public static DbContext _Context;

        private static ObservableCollection<DynamicEntity> _Entities { get; set; }
        public static ObservableCollection<DynamicEntity> Entities => _Entities; 
        private static bool _IsInitialize { get; set; }
        public static bool HasChanged { get; internal set; }
    }
}