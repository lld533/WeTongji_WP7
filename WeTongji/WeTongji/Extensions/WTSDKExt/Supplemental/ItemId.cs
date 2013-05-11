using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace WeTongji.Api.Domain
{
    [Table()]
    public class ItemId
    {
        [Column(IsPrimaryKey=true)]
        public int Id { get; set; }
    }
}
