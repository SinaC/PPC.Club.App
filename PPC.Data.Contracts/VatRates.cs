using System.ComponentModel;
using System.Runtime.Serialization;

namespace PPC.Data.Contracts
{
    [DataContract]
    public enum VatRates
    {
        [EnumMember]
        [Description("6")]
        FoodDrink, // 6
        [EnumMember]
        [Description("21")]
        Other, // 21
    }
}
