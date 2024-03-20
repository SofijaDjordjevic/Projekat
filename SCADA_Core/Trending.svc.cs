using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SCADA_Core
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Trending" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Trending.svc or Trending.svc.cs at the Solution Explorer and start debugging.
    public class Trending : ITrending
    {
        public void initialize()
        {
            TagProcessing.inputChanged += OperationContext.Current.GetCallbackChannel<ITrendingCallback>().OnInputTagChange;
        }
    }
}
