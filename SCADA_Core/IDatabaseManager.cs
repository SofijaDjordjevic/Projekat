using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDatabaseManagerService" in both code and config file together.
    [ServiceContract]
    public interface IDatabaseManager
    {
        [OperationContract]
        bool isDatabaseEmpty();

        [OperationContract]
        bool Registration(string username, string password, string role);
        
        [OperationContract]
        string SignIn(string username, string password);

        [OperationContract]
        string AddOutputTag(string tagName, string desc, string io, double init, double lowLimit, double highLimit, string type, string unit);

        [OperationContract]
        string RemoveInputTag(string tagName);

        [OperationContract]
        string AddInputTag(string tagName, string desc, string io, string driver, int scanTime, bool onOffScan, double lowLimit, double highLimit, string type, string unit);

        [OperationContract]
        string RemoveOutputTag(string tagName);

        [OperationContract]
        string ChangeOutputValue(string tagName, double value);

        [OperationContract]
        string GetOutputValues();

        [OperationContract]
        string TurnScanOnOff(string tagName);
        
        [OperationContract]
        string AddAlarm(string tagName, string type, double limit, int priority);
        
        [OperationContract]
        string RemoveAlarm(string tagName);

    }
}
