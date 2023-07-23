using System;

namespace Gallop.Live.Cutt
{
    public static class LiveTimelineEasing
    {
        public enum Type
        {
            Linear,
            ExpoEaseOut,
            ExpoEaseIn,
            ExpoEaseInOut,
            ExpoEaseOutIn,
            CircEaseOut,
            CircEaseIn,
            CircEaseInOut,
            CircEaseOutIn,
            QuadEaseOut,
            QuadEaseIn,
            QuadEaseInOut,
            QuadEaseOutIn,
            SineEaseOut,
            SineEaseIn,
            SineEaseInOut,
            SineEaseOutIn,
            CubicEaseOut,
            CubicEaseIn,
            CubicEaseInOut,
            CubicEaseOutIn,
            QuartEaseOut,
            QuartEaseIn,
            QuartEaseInOut,
            QuartEaseOutIn,
            QuintEaseOut,
            QuintEaseIn,
            QuintEaseInOut,
            QuintEaseOutIn,
            ElasticEaseOut,
            ElasticEaseIn,
            ElasticEaseInOut,
            ElasticEaseOutIn,
            BounceEaseOut,
            BounceEaseIn,
            BounceEaseInOut,
            BounceEaseOutIn,
            BackEaseOut,
            BackEaseIn,
            BackEaseInOut,
            BackEaseOutIn
        }

        private delegate double EasingFunctionDelegate(double t, double b, double c, double d);

        private static int _easeTypeMax = -1;

        private static EasingFunctionDelegate[] _functions = new EasingFunctionDelegate[41]
        {
            Linear,
            ExpoEaseOut,
            ExpoEaseIn,
            ExpoEaseInOut,
            ExpoEaseOutIn,
            CircEaseOut,
            CircEaseIn,
            CircEaseInOut,
            CircEaseOutIn,
            QuadEaseOut,
            QuadEaseIn,
            QuadEaseInOut,
            QuadEaseOutIn,
            SineEaseOut,
            SineEaseIn,
            SineEaseInOut,
            SineEaseOutIn,
            CubicEaseOut,
            CubicEaseIn,
            CubicEaseInOut,
            CubicEaseOutIn,
            QuartEaseOut,
            QuartEaseIn,
            QuartEaseInOut,
            QuartEaseOutIn,
            QuintEaseOut,
            QuintEaseIn,
            QuintEaseInOut,
            QuintEaseOutIn,
            ElasticEaseOut,
            ElasticEaseIn,
            ElasticEaseInOut,
            ElasticEaseOutIn,
            BounceEaseOut,
            BounceEaseIn,
            BounceEaseInOut,
            BounceEaseOutIn,
            BackEaseOut,
            BackEaseIn,
            BackEaseInOut,
            BackEaseOutIn
        };

        public static int easeTypeMax
        {
            get
            {
                if (_easeTypeMax < 0)
                {
                    foreach (object value in Enum.GetValues(typeof(Type)))
                    {
                        if ((int)value > _easeTypeMax)
                        {
                            _easeTypeMax = (int)value;
                        }
                    }
                    _easeTypeMax++;
                }
                return _easeTypeMax;
            }
        }

        public static float GetValue(Type type, float t, float b, float c, float d)
        {
            if ((int)type < easeTypeMax)
            {
                return (float)_functions[(int)type](t, b, c, d);
            }
            return 0f;
        }

        public static double Linear(double t, double b, double c, double d)
        {
            return c * t / d + b;
        }

        public static double ExpoEaseOut(double t, double b, double c, double d)
        {
            if (t != d)
            {
                return c * (0.0 - Math.Pow(2.0, -10.0 * t / d) + 1.0) + b;
            }
            return b + c;
        }

        public static double ExpoEaseIn(double t, double b, double c, double d)
        {
            if (t != 0.0)
            {
                return c * Math.Pow(2.0, 10.0 * (t / d - 1.0)) + b;
            }
            return b;
        }

        public static double ExpoEaseInOut(double t, double b, double c, double d)
        {
            if (t == 0.0)
            {
                return b;
            }
            if (t == d)
            {
                return b + c;
            }
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * Math.Pow(2.0, 10.0 * (t - 1.0)) + b;
            }
            return c / 2.0 * (0.0 - Math.Pow(2.0, -10.0 * (t -= 1.0)) + 2.0) + b;
        }

        public static double ExpoEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return ExpoEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return ExpoEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double CircEaseOut(double t, double b, double c, double d)
        {
            return c * Math.Sqrt(1.0 - (t = t / d - 1.0) * t) + b;
        }

        public static double CircEaseIn(double t, double b, double c, double d)
        {
            return (0.0 - c) * (Math.Sqrt(1.0 - (t /= d) * t) - 1.0) + b;
        }

        public static double CircEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return (0.0 - c) / 2.0 * (Math.Sqrt(1.0 - t * t) - 1.0) + b;
            }
            return c / 2.0 * (Math.Sqrt(1.0 - (t -= 2.0) * t) + 1.0) + b;
        }

        public static double CircEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return CircEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return CircEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double QuadEaseOut(double t, double b, double c, double d)
        {
            return (0.0 - c) * (t /= d) * (t - 2.0) + b;
        }

        public static double QuadEaseIn(double t, double b, double c, double d)
        {
            return c * (t /= d) * t + b;
        }

        public static double QuadEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * t * t + b;
            }
            return (0.0 - c) / 2.0 * ((t -= 1.0) * (t - 2.0) - 1.0) + b;
        }

        public static double QuadEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return QuadEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return QuadEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double SineEaseOut(double t, double b, double c, double d)
        {
            return c * Math.Sin(t / d * (Math.PI / 2.0)) + b;
        }

        public static double SineEaseIn(double t, double b, double c, double d)
        {
            return (0.0 - c) * Math.Cos(t / d * (Math.PI / 2.0)) + c + b;
        }

        public static double SineEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * Math.Sin(Math.PI * t / 2.0) + b;
            }
            return (0.0 - c) / 2.0 * (Math.Cos(Math.PI * (t -= 1.0) / 2.0) - 2.0) + b;
        }

        public static double SineEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return SineEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return SineEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double CubicEaseOut(double t, double b, double c, double d)
        {
            return c * ((t = t / d - 1.0) * t * t + 1.0) + b;
        }

        public static double CubicEaseIn(double t, double b, double c, double d)
        {
            return c * (t /= d) * t * t + b;
        }

        public static double CubicEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * t * t * t + b;
            }
            return c / 2.0 * ((t -= 2.0) * t * t + 2.0) + b;
        }

        public static double CubicEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return CubicEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return CubicEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double QuartEaseOut(double t, double b, double c, double d)
        {
            return (0.0 - c) * ((t = t / d - 1.0) * t * t * t - 1.0) + b;
        }

        public static double QuartEaseIn(double t, double b, double c, double d)
        {
            return c * (t /= d) * t * t * t + b;
        }

        public static double QuartEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * t * t * t * t + b;
            }
            return (0.0 - c) / 2.0 * ((t -= 2.0) * t * t * t - 2.0) + b;
        }

        public static double QuartEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return QuartEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return QuartEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double QuintEaseOut(double t, double b, double c, double d)
        {
            return c * ((t = t / d - 1.0) * t * t * t * t + 1.0) + b;
        }

        public static double QuintEaseIn(double t, double b, double c, double d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }

        public static double QuintEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * t * t * t * t * t + b;
            }
            return c / 2.0 * ((t -= 2.0) * t * t * t * t + 2.0) + b;
        }

        public static double QuintEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return QuintEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return QuintEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double ElasticEaseOut(double t, double b, double c, double d)
        {
            if ((t /= d) == 1.0)
            {
                return b + c;
            }
            double num = d * 0.3;
            double num2 = num / 4.0;
            return c * Math.Pow(2.0, -10.0 * t) * Math.Sin((t * d - num2) * (Math.PI * 2.0) / num) + c + b;
        }

        public static double ElasticEaseIn(double t, double b, double c, double d)
        {
            if ((t /= d) == 1.0)
            {
                return b + c;
            }
            double num = d * 0.3;
            double num2 = num / 4.0;
            return 0.0 - c * Math.Pow(2.0, 10.0 * (t -= 1.0)) * Math.Sin((t * d - num2) * (Math.PI * 2.0) / num) + b;
        }

        public static double ElasticEaseInOut(double t, double b, double c, double d)
        {
            if ((t /= d / 2.0) == 2.0)
            {
                return b + c;
            }
            double num = d * 0.44999999999999996;
            double num2 = num / 4.0;
            if (t < 1.0)
            {
                return -0.5 * (c * Math.Pow(2.0, 10.0 * (t -= 1.0)) * Math.Sin((t * d - num2) * (Math.PI * 2.0) / num)) + b;
            }
            return c * Math.Pow(2.0, -10.0 * (t -= 1.0)) * Math.Sin((t * d - num2) * (Math.PI * 2.0) / num) * 0.5 + c + b;
        }

        public static double ElasticEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return ElasticEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return ElasticEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double BounceEaseOut(double t, double b, double c, double d)
        {
            if ((t /= d) < 0.36363636363636365)
            {
                return c * (7.5625 * t * t) + b;
            }
            if (t < 0.72727272727272729)
            {
                return c * (7.5625 * (t -= 0.54545454545454541) * t + 0.75) + b;
            }
            if (t < 0.90909090909090906)
            {
                return c * (7.5625 * (t -= 0.81818181818181823) * t + 0.9375) + b;
            }
            return c * (7.5625 * (t -= 21.0 / 22.0) * t + 63.0 / 64.0) + b;
        }

        public static double BounceEaseIn(double t, double b, double c, double d)
        {
            return c - BounceEaseOut(d - t, 0.0, c, d) + b;
        }

        public static double BounceEaseInOut(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return BounceEaseIn(t * 2.0, 0.0, c, d) * 0.5 + b;
            }
            return BounceEaseOut(t * 2.0 - d, 0.0, c, d) * 0.5 + c * 0.5 + b;
        }

        public static double BounceEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return BounceEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return BounceEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }

        public static double BackEaseOut(double t, double b, double c, double d)
        {
            return c * ((t = t / d - 1.0) * t * (2.70158 * t + 1.70158) + 1.0) + b;
        }

        public static double BackEaseIn(double t, double b, double c, double d)
        {
            return c * (t /= d) * t * (2.70158 * t - 1.70158) + b;
        }

        public static double BackEaseInOut(double t, double b, double c, double d)
        {
            double num = 1.70158;
            if ((t /= d / 2.0) < 1.0)
            {
                return c / 2.0 * (t * t * (((num *= 1.525) + 1.0) * t - num)) + b;
            }
            return c / 2.0 * ((t -= 2.0) * t * (((num *= 1.525) + 1.0) * t + num) + 2.0) + b;
        }

        public static double BackEaseOutIn(double t, double b, double c, double d)
        {
            if (t < d / 2.0)
            {
                return BackEaseOut(t * 2.0, b, c / 2.0, d);
            }
            return BackEaseIn(t * 2.0 - d, b + c / 2.0, c / 2.0, d);
        }
    }
}
