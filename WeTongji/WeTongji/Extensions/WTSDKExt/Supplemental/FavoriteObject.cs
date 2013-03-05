using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace WeTongji.Api.Domain
{
    public enum FavoriteIndex : uint
    {
        kActivity,
        kPeopleOfWeek,
        kTongjiNews,
        kAroundNews,
        kOfficialNotes,
        kClubNews,

        FavoriteTypeCount
    }

    [Table()]
    public class FavoriteObject
    {
        /// <summary>
        /// Refers to FavoriteIndex
        /// </summary>
        [Column(IsPrimaryKey = true)]
        public uint Id { get; set; }

        /// <summary>
        /// {Id0}_{Id1}_.....{Idn}
        /// </summary>
        [Column()]
        public String Value { get; set; }
    }
}
