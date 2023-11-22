using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket.BusAction
{
    public class ReplyTime
    {
        public void ReTime(byte[] data, Socket sc)
        {
            string msg = Encoding.UTF8.GetString(data);
            if (msg.Contains("time"))
            {
                var rbuffer = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                sc.Send(rbuffer);
            }
        }
    }
}
