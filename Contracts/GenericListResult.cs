﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Arena.Custom.NPM.WebServiceMatching.Contracts
{
    [DataContract(Namespace = "", Name = "GenericListResult")]
    public class GenericListResult<T>
    {
		[DataMember]
		public List<T> Items { get; set; }
		[DataMember]
		public int Total { get; set; }
		[DataMember]
		public int Max { get; set; }
		[DataMember]
		public int Start { get; set; }
    }
}
