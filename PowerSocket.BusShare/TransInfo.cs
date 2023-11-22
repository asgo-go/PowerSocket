using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket
{
    [Serializable]
    public class TransInfo<T>
    {
        // 改进 前5个字节表示传输对象长度2^40 最大1T足够 表示类型的放入传输对象中

        //大对象 如文件 传输 会经过多次？？ 是否超过256M的先存到硬盘上？
        public string 消息ID { get; set; }
        /// <summary>
        /// 目标接收到 标识是哪个人通过服务端转发的
        /// </summary>
        public string 发送人 { get; set; }
        public string 接收人 { get; set; }


        public T Data { get; set; }

        /// <summary>
        /// 备注消息
        /// </summary>
        public string Msg { get; set; }
        //public AcceptStatus 状态 { get; set; }
    }
}
