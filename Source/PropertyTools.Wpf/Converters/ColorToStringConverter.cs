﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorToStringConverter.cs" company="PropertyTools">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Converts <see cref="Color" /> instances to <see cref="string" /> instances..
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyTools.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converts <see cref="Color" /> instances to <see cref="string" /> instances..
    /// </summary>
    [ValueConversion(typeof(Color), typeof(string))]
    public class ColorToStringConverter : IValueConverter
    {
        /// <summary>
        /// The string to color map.
        /// </summary>
        private static Dictionary<string, Color> colors;

        /// <summary>
        /// Gets the string to color map.
        /// </summary>
        /// <value> The color map. </value>
        public static Dictionary<string, Color> ColorMap
        {
            get
            {
                if (colors == null)
                {
                    colors = new Dictionary<string, Color>();
                    var t = typeof(Colors);
                    var fields = t.GetProperties(BindingFlags.Public | BindingFlags.Static);
                    foreach (var fi in fields)
                    {
                        var c = (Color)fi.GetValue(null, null);
                        colors.Add(fi.Name, c);
                    }

                    colors.Add("Undefined", ColorHelper.UndefinedColor);
                    colors.Add("Automatic", ColorHelper.Automatic);
                }

                return colors;
            }
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">
        /// The value produced by the binding source.
        /// </param>
        /// <param name="targetType">
        /// The type of the binding target property.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var color = (Color)value;

            // if (ShowAsHex)
            // return ColorHelper.ColorToHex(color);
            string nearestColor = null;
            double nearestDist = 30;

            // find the color that is closest
            foreach (var kvp in ColorMap)
            {
                if (color == kvp.Value)
                {
                    return kvp.Key;
                }

                double d = ColorHelper.ColorDifference(color, kvp.Value);
                if (d < nearestDist)
                {
                    nearestColor = "~ " + kvp.Key; // 'kind of'
                    nearestDist = d;
                }
            }

            if (nearestColor == null)
            {
                return color.ColorToHex();
            }

            if (color.A < 255)
            {
                return string.Format("{0}, {1:0} %", nearestColor, color.A / 2.55);
            }

            return nearestColor;
        }

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">
        /// The value that is produced by the binding target.
        /// </param>
        /// <param name="targetType">
        /// The type to convert to.
        /// </param>
        /// <param name="parameter">
        /// The converter parameter to use.
        /// </param>
        /// <param name="culture">
        /// The culture to use in the converter.
        /// </param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            if (s == null)
            {
                return DependencyProperty.UnsetValue;
            }

            Color color;

            if (ColorMap.TryGetValue(s, out color))
            {
                return color;
            }

            var c = ColorHelper.HexToColor(s);
            if (c != ColorHelper.UndefinedColor)
            {
                return c;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}