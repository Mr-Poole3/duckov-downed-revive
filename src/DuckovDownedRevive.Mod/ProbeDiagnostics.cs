using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DuckovDownedRevive
{
    internal static class ProbeDiagnostics
    {
        private const int MaxRecentLines = 8;
        private static readonly object Sync = new object();
        private static readonly Queue<string> Recent = new Queue<string>();

        public static int HealthPrefixCount { get; private set; }
        public static int HealthPostfixCount { get; private set; }
        public static int ReceiverPrefixCount { get; private set; }
        public static int ReceiverPostfixCount { get; private set; }

        public static void RecordHealthPrefix(string line)
        {
            HealthPrefixCount++;
            Record(line);
        }

        public static void RecordHealthPostfix(string line)
        {
            HealthPostfixCount++;
            Record(line);
        }

        public static void RecordReceiverPrefix(string line)
        {
            ReceiverPrefixCount++;
            Record(line);
        }

        public static void RecordReceiverPostfix(string line)
        {
            ReceiverPostfixCount++;
            Record(line);
        }

        public static void RecordSystem(string line)
        {
            Record(line);
        }

        public static string FormatRecent()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Probe: Health=");
            builder.Append(HealthPrefixCount);
            builder.Append("/");
            builder.Append(HealthPostfixCount);
            builder.Append(" Receiver=");
            builder.Append(ReceiverPrefixCount);
            builder.Append("/");
            builder.Append(ReceiverPostfixCount);

            lock (Sync)
            {
                foreach (string line in Recent)
                {
                    builder.Append("\n");
                    builder.Append(line);
                }
            }

            return builder.ToString();
        }

        private static void Record(string line)
        {
            string safeLine = line ?? string.Empty;
            lock (Sync)
            {
                Recent.Enqueue(safeLine);
                while (Recent.Count > MaxRecentLines)
                {
                    Recent.Dequeue();
                }
            }

            Debug.LogWarning(safeLine);
            try
            {
                string path = Path.Combine(
                    Application.persistentDataPath,
                    "DuckovDownedReviveProbe.log");
                File.AppendAllText(
                    path,
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                    "] " + safeLine + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}
