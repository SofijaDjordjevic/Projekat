using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AlarmDisplay" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AlarmDisplay.svc or AlarmDisplay.svc.cs at the Solution Explorer and start debugging.
    public class AlarmDisplay : IAlarmDisplay
    {
        public void initialize()
        {
            TagProcessing.alarmCalled += OperationContext.Current.GetCallbackChannel<IAlarmDisplayCallback>().OnAlarmInvoked;
        }
    }
}
