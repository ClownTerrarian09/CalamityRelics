using Microsoft.Xna.Framework;
using System;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace CalamityRelics.Content.Common {
    public class CommonMath {
        public static float GetAngleFromTriangle(float adjacent1, float adjacent2, float opposite) {
            return MathF.Acos(
                ((adjacent1 * adjacent1) + (adjacent2 * adjacent2) - (opposite * opposite))
                / (2 * adjacent1 * adjacent2)
            );
        }

        /// <summary>
        /// Caps the vector2 while perserving direction.
        /// </summary>
        public static Vector2 Limit(Vector2 v, float max) {
            if (v.Length() > max)
                return Vector2.Normalize(v) * max;
            return v;
        }
    }

    public static class CommonMathExtensions {

        /// <summary>
        /// Caps the vector2 while perserving direction.
        /// </summary>
        public static void Limit(this ref Vector2 vec, float max) {
            vec = CommonMath.Limit(vec, max);
        }

        public static void Limit<T>(this ref T value, T min, T max) where T : struct, IComparable<T> {
            if (value.CompareTo(min) < 0)
                value = min;
            if (value.CompareTo(max) > 0)
                value = max;
        }

        public static void Min<T>(this ref T value, T min) where T : struct, IComparable<T> {
            if (value.CompareTo(min) < 0)
                value = min;
        }

        public static void Max<T>(this ref T value, T max) where T : struct, IComparable<T> {
            if (value.CompareTo(max) > 0)
                value = max;
        }


        public static T AsLimit<T>(this T value, T min, T max) where T : IComparable<T> {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }

        public static T AsMin<T>(this T value, T min) where T : struct, IComparable<T> {
            if (value.CompareTo(min) < 0)
                return min;
            return value;
        }

        public static T AsMax<T>(this T value, T max) where T : struct, IComparable<T> {
            if (value.CompareTo(max) > 0)
                return max;
            return value;
        }
    }
}