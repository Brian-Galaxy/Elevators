using System;
using System.IO;
using Sandbox.ModAPI;
using VRage.Utils;

namespace Vicizlat.MultifloorElevator
{
    internal class Logging
    {
        private static Logging ThisInstance;
        private readonly TextWriter Writer;

        public Logging()
        {
            string[] name = GetType().FullName.Split('.');
            Writer = MyAPIGateway.Utilities.WriteFileInLocalStorage($"{name[name.Length - 2]}.log", typeof(Logging));
            ThisInstance = this;
        }

        public static Logging Instance => MyAPIGateway.Utilities == null ? null : (ThisInstance ?? new Logging());

        public void WriteLine(string text)
        {
            try
            {
                Writer.WriteLine(DateTime.Now.ToString("[HH:mm:ss:ffff] ") + text);
                Writer.Flush();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"[ Error: {e.Message}\n{e.StackTrace} ]");
            }
        }

        public void WriteException(string message, string stackTrace) => WriteLine($"[ Error: {message}\n{stackTrace} ]");

        internal void Close()
        {
            Writer.Flush();
            Writer.Close();
        }
    }
}