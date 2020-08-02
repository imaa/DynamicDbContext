using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Model
{
    public class DynamicEntity
    {
        public DynamicEntity()
        {
            Properties = new ObservableCollection<DynamicProperty>();

        }
        public long Id { get; set; }
        public Guid UniqueId { get; set; }
        public string Name { get; set; }
        public Type EntityType { get; set; }
        public ObservableCollection<DynamicProperty> Properties { get; set; }
        public bool IsDetailedEntity { get; internal set; }
        public string TableName { get; internal set; }
        public Type BaseType { get; internal set; }
        public long MasterId { get; internal set; }
    }
}
