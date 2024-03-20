using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IRealTimeUnit" in both code and config file together.
    [ServiceContract]
    public interface IRealTimeUnit
    {
        [OperationContract]
        bool initialize(string id, string io, double lowLimit, double highLimit, byte[] signature, string message);

        [OperationContract]
        void ValueToAddres(string io, int number);
    }
}
