using System.Text;

namespace Norify
{
    internal static class InternalStatic
    {
        private static readonly StringBuilder _builder = new StringBuilder();

        public static StringBuilder Sb => _builder;
    }
}