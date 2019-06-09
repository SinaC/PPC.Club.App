using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PPC.Helpers
{
    public static class DataContractHelpers
    {
        public static T Read<T>(string filename)
        {
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(reader);
            }
        }

        public static async Task<T> ReadAsync<T>(string filename)
        {
            using (XmlTextReader reader = new XmlTextReader(filename))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                return (T)await serializer.ReadObjectAsync(reader);
            }
        }

        public static void Write<T>(string filename, T obj)
        {
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(writer, obj);
            }
        }

        public static async Task WriteAsync<T>(string filename, T obj)
        {
            using (XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                DataContractSerializer serializer = new DataContractSerializer(typeof(T));
                await serializer.WriteObjectAsync(writer, obj);
            }
        }
    }
}
