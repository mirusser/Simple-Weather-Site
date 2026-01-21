using System.Text;

namespace Common.ExtensionMethods;

public static class TypeExtensions
{
        public static string ToFriendlyFormat(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var tick = name.IndexOf('`');
            if (tick > 0) name = name[..tick];

            var args = type.GetGenericArguments();
            var sb = new StringBuilder();
            sb.Append(name);
            sb.Append('<');

            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(args[i].ToFriendlyFormat());
            }

            sb.Append('>');
            return sb.ToString();
        }
}