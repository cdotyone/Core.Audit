using System;

namespace Core.Audit
{
    internal static class Extensions
    {
        public static string InsureUID(this string uid)
        {
            if (!string.IsNullOrEmpty(uid)) return uid;
            return Guid.NewGuid().ToString().Replace("-", "").ToUpperInvariant();
        }
    }
}
