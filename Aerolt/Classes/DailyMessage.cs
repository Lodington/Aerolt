using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aerolt.Enums;
using Aerolt.Helpers;
using Newtonsoft.Json;

namespace Aerolt.Classes
{
    public class DailyMessage
    {
        public static string GetMessage()
        {
            try
            {
                var json = new WebClient().DownloadString("http://170.187.136.238:3000");
                var message = JsonConvert.DeserializeObject<Message>(json);

                return message.message;
                Tools.Log(LogLevel.Debug, message.message);
            }
            catch (Exception e)
            {
                //Load.CallPopup("Daily Message Error", "Can't get daily message, This could be because you are offline or bubbet doesnt know how to run a server.", Load.settingsRoot.transform);
                Tools.Log(LogLevel.Error, "Server is down");
                return "Can't get daily message, This could be because you are offline or bubbet doesnt know how to run a server.";
            }
        }
    }
}
