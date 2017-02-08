using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace PPC.Helpers
{
    public static class DataContractSerializerExtensions
    {
        public static async Task WriteObjectAsync(this DataContractSerializer serializer, XmlWriter writer, object graph)
        {
            await Task.Run(() => serializer.WriteObject(writer, graph));
        }

        public static async Task<object> ReadObjectAsync(this DataContractSerializer serializer, XmlReader reader)
        {
            return await Task.Run(() => serializer.ReadObject(reader));
        }
    }
}
