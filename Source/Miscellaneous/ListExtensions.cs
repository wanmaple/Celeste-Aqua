using System.Collections.Generic;

namespace Celeste.Mod.Aqua.Miscellaneous
{
    public static class ListExtensions
    {
        public static bool RemoveAtFast<T>(this List<T> self, int index)
        {
            if (index >= 0 && index < self.Count)
            {
                self[index] = self[self.Count - 1];
                self.RemoveAt(self.Count - 1);
                return true;
            }
            return false;
        }

        public static bool RemoveFast<T>(this List<T> self, T item)
        {
            int index = self.IndexOf(item);
            if (index >= 0)
            {
                return self.RemoveAtFast(index);
            }
            return false;
        }
    }
}
