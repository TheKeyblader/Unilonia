using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using UnityEngine;
using UnityRect = UnityEngine.Rect;
using AvaloniaRect = Avalonia.Rect;
using UnityColor = UnityEngine.Color;
using AvaloniaColor = Avalonia.Media.Color;

namespace Unilonia
{
    public static class UnityHelpers
    {
        #region Vectors
        public static Vector2 ToUnity(this Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
        public static Vector2 ToUnity(this Size point)
        {
            return new Vector2((float)point.Width, (float)point.Height);
        }
        public static Vector2 ToUnity(this Vector vector)
        {
            return new Vector2((float)vector.X, (float)vector.Y);
        }
        public static Point ToAvalonia(this Vector2 vector)
        {
            return new Point(vector.x, vector.y);
        }
        #endregion

        #region Rects
        public static UnityRect ToUnity(this AvaloniaRect rect)
        {
            return new UnityRect(rect.Position.ToUnity(), rect.Size.ToUnity());
        }
        public static AvaloniaRect ToAvalonia(this UnityRect rect)
        {
            return new AvaloniaRect(rect.position.ToAvalonia(), new Size(rect.size.x, rect.size.y));
        }
        public static AvaloniaRect ToAvalonia(this Bounds bounds)
        {
            return new AvaloniaRect(((Vector2)bounds.min).ToAvalonia(), ((Vector2)bounds.max).ToAvalonia());
        }
        #endregion

        public static UnityColor ToUnity(this AvaloniaColor color)
        {
            return new UnityColor(color.R, color.G, color.B, color.A);
        }
    }
}
