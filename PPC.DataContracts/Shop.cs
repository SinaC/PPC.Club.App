﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PPC.DataContracts
{
    [DataContract]
    public class Shop
    {
        [DataMember]
        public List<Item> Articles { get; set; }
        [DataMember]
        public double Cash { get; set; }
        [DataMember]
        public double BankCard { get; set; }
    }
}
