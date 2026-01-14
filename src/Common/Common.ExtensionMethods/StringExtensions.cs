using System.Xml.Serialization;

namespace Common.ExtensionMethods;

public static class StringExtensions
{
    extension(string xml)
    {
        public T DeserializeXml<T>()
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StringReader(xml);
            return (T)serializer.Deserialize(reader)!;
        }
    }
}