using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket.BusAction.File
{
    public class FileSend
    {
        private byte[] DataHeader = new byte[] { 0x55, 0xA5, 0x9E, 0x2B };
        public void DealAccept(byte[] data)
        {
            //请求对方接收-发送文件头（对象）-发送文件体
            //using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            //{
            //    //一次性发送
            //    byte[] buffer = new byte[fs.Length];
            //    int r = fs.Read(buffer, 0, buffer.Length);
            //    SendData(buffer);
            //}
        }

    }
}
