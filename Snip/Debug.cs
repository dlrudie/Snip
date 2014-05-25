#region File Information
/*
 * Copyright (C) 2012-2014 David Rudie
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02111, USA.
 */
#endregion

namespace Winter
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    public static class Debug
    {
        private static readonly string debugPath = @Application.StartupPath + @"\Debug.txt";

        private static int methodMeasurementDepth = 0;

        public static void ToggleDebugging()
        {
            if (Globals.DebuggingIsEnabled)
            {
                Globals.DebuggingIsEnabled = false;
            }
            else
            {
                Globals.DebuggingIsEnabled = true;

                File.WriteAllText(debugPath, "Begin debugging" + Environment.NewLine);

                WriteDebugEmptyLine();

                if (Stopwatch.IsHighResolution)
                {
                    WriteDebugData("Operations timed using the system's high-resolution performance counter.");
                }
                else
                {
                    WriteDebugData("Operations timed using the DateTime class.");
                }

                long frequency = Stopwatch.Frequency;
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "  Timer frequency in ticks per second = {0}", frequency));

                long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "  Timer is accurate within {0} nanoseconds", nanosecPerTick));

                WriteDebugEmptyLine();
            }
        }

        public static void WriteDebugEmptyLine()
        {
            WriteDebugData(string.Empty);
        }

        public static void WriteDebugData(string data)
        {
            File.AppendAllText(@Application.StartupPath + @"\Debug.txt", data + Environment.NewLine);
        }

        public static void MeasureMethod(Action method)
        {
            if (method != null)
            {
                methodMeasurementDepth++;

                StackTrace stackTrace = new StackTrace();

                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Measuring {1}, called by {2}", "  ".AddIndentation(methodMeasurementDepth), method.Method.Name, stackTrace.GetFrame(1).GetMethod().Name));
                Stopwatch stopwatch = Stopwatch.StartNew();
                method();
                stopwatch.Stop();
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ticks: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedTicks));
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ms: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedMilliseconds));

                if (methodMeasurementDepth <= 1)
                {
                    WriteDebugEmptyLine();
                }

                methodMeasurementDepth--;
            }
        }

        public static void MeasureMethod(Action<string> method, string textToPass)
        {
            if (method != null)
            {
                methodMeasurementDepth++;

                StackTrace stackTrace = new StackTrace();

                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Measuring {1}, called by {2}", "  ".AddIndentation(methodMeasurementDepth), method.Method.Name, stackTrace.GetFrame(1).GetMethod().Name));
                Stopwatch stopwatch = Stopwatch.StartNew();
                method(textToPass);
                stopwatch.Stop();
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ticks: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedTicks));
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ms: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedMilliseconds));

                if (methodMeasurementDepth <= 1)
                {
                    WriteDebugEmptyLine();
                }

                methodMeasurementDepth--;
            }
        }

        public static void MeasureMethod(Action<object[]> method, object[] arrayToPass)
        {
            if (method != null)
            {
                methodMeasurementDepth++;

                StackTrace stackTrace = new StackTrace();

                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Measuring {1}, called by {2}", "  ".AddIndentation(methodMeasurementDepth), method.Method.Name, stackTrace.GetFrame(1).GetMethod().Name));
                Stopwatch stopwatch = Stopwatch.StartNew();
                method(arrayToPass);
                stopwatch.Stop();
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ticks: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedTicks));
                WriteDebugData(string.Format(CultureInfo.InvariantCulture, "{0}Elapsed ms: {1}", "  ".AddIndentation(methodMeasurementDepth), stopwatch.ElapsedMilliseconds));

                if (methodMeasurementDepth <= 1)
                {
                    WriteDebugEmptyLine();
                }

                methodMeasurementDepth--;
            }
        }

        private static string AddIndentation(this string source, int multiplier)
        {
            StringBuilder stringBuilder = new StringBuilder(multiplier + source.Length);

            for (int i = 0; i < multiplier; i++)
            {
                stringBuilder.Append(source);
            }

            return stringBuilder.ToString();
        }
    }
}
