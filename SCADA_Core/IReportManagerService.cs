using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IReportManagerService" in both code and config file together.
    [ServiceContract]
    public interface IReportManagerService
    {
        [OperationContract]
        List<AlarmValue> GetAlarmsInTimeRange(DateTime from, DateTime to, string sortBy, string sortType);
        [OperationContract]
        List<AlarmValue> GetAlarmsByPriority(int prioritet, string sortType);
        [OperationContract]
        List<TagValue> GetTagsInTimeRange(DateTime from, DateTime to, string sortType);
        [OperationContract]
        List<TagValue> GetLastValuesAI(string sortType);
        [OperationContract]
        List<TagValue> GetLastValuesDI(string sortType);
        [OperationContract]
        List<TagValue> GetTagValuesByName(string tagName, string sortType);
    }
}
