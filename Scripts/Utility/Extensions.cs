using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Random = NS.Core.Random;

namespace NS.Utility
{
    public static class Extensions
    {
        public static T RandomElement<T>(this T[] array)
        {
            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        public static T RandomElement<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        public static T RandomElement<T>(this IList<T> list, Random random)
        {
            return list[random.NextInt(0, list.Count)];
        }

        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector3(x, v.y);
        }

        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector3(v.x, y);
        }

        // Source - https://stackoverflow.com/a/1262619, modified
        // Posted by grenade, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-07-12, License - CC BY-SA 4.0
        public static void Shuffle<T>(this IList<T> list, [CanBeNull] Random rand)
        {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = Helpers.RandomInt(0, n + 1, rand);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            list.Shuffle(null);
        }

        private static class Helpers
        {
            public static int RandomInt(int minIncl, int maxExcl, Random rand = null)
            {
                return rand?.NextInt(minIncl, maxExcl) ?? UnityEngine.Random.Range(minIncl, maxExcl);
            }
        }

        public static void SetActive(this VisualElement visualElement, bool active)
        {
            visualElement.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
