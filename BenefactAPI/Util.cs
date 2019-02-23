using Replicate.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public static class Util
    {
        public static void UpdateMembersFrom<T>(T target, T newFields, string[] whiteList = null, string[] blackList = null)
        {
            var td = ReplicationModel.Default.GetTypeAccessor(typeof(T));
            IEnumerable<MemberAccessor> members = td.MemberAccessors;
            if (whiteList != null && whiteList.Any())
                members = members.Where(mem => whiteList.Contains(mem.Info.Name));
            if (blackList != null && blackList.Any())
                members = members.Where(mem => !blackList.Contains(mem.Info.Name));
            foreach (var member in members)
            {
                var newValue = member.GetValue(newFields);
                if (newValue == null) continue;
                member.SetValue(target, newValue);
            }
        }
        public static double Now() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }
}
