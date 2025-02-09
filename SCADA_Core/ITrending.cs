﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITrending" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(ITrendingCallback))]
    public interface ITrending
    {
        [OperationContract]
        void initialize();
    }

    public interface ITrendingCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnInputTagChange(string input, double value);
    }
}
