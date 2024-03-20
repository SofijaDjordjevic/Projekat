using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAlarmDisplay" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IAlarmDisplayCallback))]
    public interface IAlarmDisplay
    {
        [OperationContract]
        void initialize();
    }

    public interface IAlarmDisplayCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnAlarmInvoked(string alarm, int prioritet);
    }
}
