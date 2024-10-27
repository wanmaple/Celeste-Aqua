using System;
using System.Text;

namespace Celeste.Mod.Aqua
{
    public class AquaException : Exception
    {
        public string From { get; private set; }

        public AquaException(string message, string from, Exception innerException = null)
        : base(message, innerException)
        {
            From = from;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("From: {0}", From));
            sb.Append(base.ToString());
            return sb.ToString();
        }
    }
}
