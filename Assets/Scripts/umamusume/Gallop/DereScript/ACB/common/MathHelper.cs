using System;

namespace DereTore.Common {
    public static class MathHelper {

        public static double ClampUpper(double value, double minimum) {
            return value < minimum ? minimum : value;
        }

        public static double ClampLower(double value, double maximum) {
            return value > maximum ? maximum : value;
        }

        public static double Clamp(double value, double minimum, double maximum) {
            return value < minimum ? minimum : (value > maximum ? maximum : value);
        }

        public static float Clamp(float value, float minimum, float maximum) {
            return value < minimum ? minimum : (value > maximum ? maximum : value);
        }

        public static int NextRandomPositiveInt32() {
            return Random.Next(1, int.MaxValue);
        }

        public static int NextRandomInt32() {
            return Random.Next();
        }

        public static int NextRandomInt32(int maxValue) {
            return Random.Next(maxValue);
        }

        public static int NextRandomInt32(int minValue, int maxValue) {
            return Random.Next(minValue, maxValue);
        }

        public static long NextRandomInt64() {
            var v1 = (long)NextRandomInt32();
            var v2 = NextRandomInt32();
            return (v1 << 32) + v2;
        }

        public static float NextRandomSingle() {
            return (float)Random.NextDouble();
        }

        public static double NextRandomDouble() {
            return Random.NextDouble();
        }

        public static uint GreatestCommonFactor(uint a, uint b) {
            while (true) {
                if (a < b) {
                    var t = a;
                    a = b;
                    b = t;
                }
                var m = a % b;
                if (m == 0 || m == 1) {
                    return b;
                } else {
                    a = b;
                    b = m;
                }
            }
        }

        public static uint GreatestCommonFactor(params uint[] numbers) {
            if (numbers.Length == 0) {
                throw new ArgumentException();
            }
            if (numbers.Length == 1) {
                return numbers[0];
            }
            var gcd = GreatestCommonFactor(numbers[0], numbers[1]);
            if (numbers.Length == 2) {
                return gcd;
            }
            var currentIndex = 2;
            while (currentIndex < numbers.Length) {
                if (gcd == 1) {
                    break;
                }
                // Please note the param order.
                gcd = GreatestCommonFactor(numbers[currentIndex], gcd);
                ++currentIndex;
            }
            return gcd;
        }

        public static bool IsMultipleOf(this int a, int b) {
            return a / b * b == a;
        }

        public static int RoundUpTo(int value, int align) {
            if (value % align == 0) {
                return value;
            }
            return value + (align - (value % align));
        }

        public static int RoundUpToNext(int value, int align) {
            if (value % align == 0) {
                return value + align;
            }
            return value + (align - (value % align));
        }

        public static readonly Random Random = new Random();

    }
}
