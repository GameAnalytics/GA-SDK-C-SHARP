using System;
using System.Collections.Generic;
using System.Text;

namespace GameAnalyticsSDK.Net
{
    public interface IRemoteConfigsListener
    {
        void OnRemoteConfigsUpdated();
    }
}
