using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicDbContext.Test.Base
{
   public class EntityBase
    {
        [Key]
        public int Id { get; set; }
        public int CreatedBy { get; set; }
    }
}
