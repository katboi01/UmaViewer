using UnityEngine;

namespace Cutt
{
    public static class CuttRect_Helper
    {
        public static Rect Extend(this Rect rect, float extendVal)
        {
            float num = extendVal / 2f;
            rect.x -= num;
            rect.y -= num;
            rect.width += extendVal;
            rect.height += extendVal;
            return rect;
        }

        public static Rect GetTopEdge(this Rect rect, float width)
        {
            rect.height = width;
            return rect;
        }

        public static Rect GetLeftEdge(this Rect rect, float width)
        {
            rect.width = width;
            return rect;
        }

        public static Rect GetRightEdge(this Rect rect, float width)
        {
            rect.x = rect.x + rect.width - width;
            rect.width = width;
            return rect;
        }

        public static Rect GetBottomEdge(this Rect rect, float width)
        {
            rect.y = rect.y + rect.height - width;
            rect.height = width;
            return rect;
        }
    }
}
