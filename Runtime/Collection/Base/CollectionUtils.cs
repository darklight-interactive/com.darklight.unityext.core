using System;
using System.Collections.Generic;
using System.Linq;

namespace Darklight.UnityExt.Collection
{
    public static class CollectionUtils
    {
        public static List<TEnum> GetAllPossibleKeys<TEnum>()
            where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
        }

        public static int GetNextId(IEnumerable<int> ids)
        {
            return ids.Count() > 0 ? ids.Max() + 1 : 0;
        }
    }
}
