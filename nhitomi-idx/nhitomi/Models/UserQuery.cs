using System;
using nhitomi.Models.Queries;

namespace nhitomi.Models
{
    public class UserQuery : QueryBase<UserSort>
    {
        public RangeQuery<DateTime> CreatedTime { get; set; }
        public RangeQuery<DateTime> UpdatedTime { get; set; }
        public FilterQuery<UserPermissions> Permissions { get; set; }
    }
}