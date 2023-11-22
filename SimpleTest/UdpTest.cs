

using System; 

using System.Collections.Generic;
 using System.ComponentModel;
 using System.Data; 
using System.Drawing; 
using System.Text; 

using System.Windows.Forms;
 using System.Text.RegularExpressions;
 using System.Net; 

using System.Net.Sockets; 

using System.Threading; 

namespace UDPListenPorts
{
    public partial class UDPSever : Form
    {
        /*声明了一个事件MessageArrived，声明事件之前我们先声明了一个名为MessageHandler的委托。
        * 可以看出，与委托有明显的区别，事件不是一个类型，而是类的一个成员，是属于类的，和字 
       * 段、属性、方法一样，都是类的一部分。声明MessageArrived事件需要使用关键字event，并 
       * 在前面加上委托类型的名称，如果不加关键字event就和上文所述的声明委托变量一样了。事 
       * 件前面的委托类型说明，处理事件的函数必须符合委托所指定的原型形式。*/

        public delegate void MessageHandler(string Message);//定义委托事件 
        public event MessageHandler MessageArrived;
        public delegate void DelegateChangeText(string Messages);//委托文本框 
        StringBuilder stringbuilder = new StringBuilder();//缓存 
        public delegate void DelegateChangeDataGridView(string time, string ip, string data);//委托
        DataGridView public delegate void DataGridViewHandler(string time, string ip, string data);//定义委托事件
        public event DataGridViewHandler DataGridViewed;
        ConnectSql connectsql = new ConnectSql();
        public UdpClient ReceiveUdpClient;

        /// 

        /// 侦听端口名称 /// 
        public int PortName;

        /// /// 本地地址 /// 
        public IPEndPoint LocalIPEndPoint;

        /// /// 日志记录 /// 
        public StringBuilder Note_StringBuilder;

        /// /// 本地IP地址 /// 
        public IPAddress MyIPAddress;
        public UDPSever()
        {
            InitializeComponent();

            //获取本机可用IP地址 
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipa in ips)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    MyIPAddress = ipa;//获取本地IP地址
                    break;
                }
            }
            Note_StringBuilder = new StringBuilder();
        }
        public void Thread_Listen()
        {

            //创建一个线程接收远程主机发来的信息 
            Thread myThread = new Thread(ReceiveData);
            myThread.IsBackground = true; myThread.Start();
        }

        /// /// 接收数据 /// 
        private void ReceiveData()
        {
            try
            {
                ReceiveUdpClient = new UdpClient(PortName);
            }
            catch
            {
                MessageBox.Show("正在监听中!");
            }
            IPEndPoint remote = new IPEndPoint(IPAddress.Any, PortName);
            while (true)
            {
                try
                {

                    //关闭udpClient 时此句会产生异常
                    byte[] receiveBytes = ReceiveUdpClient.Receive(ref remote);
                    string receiveMessage = Encoding.Default.GetString(receiveBytes, 0, receiveBytes.Length);
                    // receiveMessage = ASCIIEncoding.ASCII.GetString(receiveBytes, 0, receiveBytes.Length); 
                    string datetime = DateTime.Now.ToString();
                    MessageArrived(string.Format("{0}来自{1}:{2}",
                    datetime, remote, receiveMessage));
                    DataGridViewed(datetime, remote.ToString(), receiveMessage);

                    //try 

                    //{
                    // Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");
                    // ReceiveUdpClient.Send(sendBytes, sendBytes.Length, local);
                    //} 

                    //catch (Exception e) 

                    //{ 

                    //} 

                    //break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
        }

        //监听 
        private void btn_Listen_Click(object sender, EventArgs e)
        {
            PortName = Convert.ToInt32(cbx_Port.Text.ToString(), 10);
            Thread_Listen();
            MessageArrived += new MessageHandler(MessageArriveds);//文本事件 
            DataGridViewed += new DataGridViewHandler(DataGridViewShow);
        } //文本框Invoke方法需要创建一个委托。你可以事先写好函数和与之对应的委托 
        private void MessageArriveds(string Message)
        {
            tbx_Show.Invoke(new DelegateChangeText(ChangeTxt), Message);
        }

        //委托函数 
        void ChangeTxt(string Messages)
        {
            string SbuText = stringbuilder.ToString();
            stringbuilder.Remove(0, stringbuilder.Length);
            stringbuilder.Append(Messages + "\r\n" + SbuText);
            tbx_Show.Text = stringbuilder.ToString();
        }
        private void DataGridViewShow(string time, string ip, string data)
        {
            dgv_Content.Invoke(new DelegateChangeDataGridView(Changedgv), new object[] { time, ip, data });
        }

        //委托DataGridView控件显示列的文本
        void Changedgv(string time, string ip, string data)
        {
            int index = dgv_Content.Rows.Add();
            dgv_Content.Rows[index].Cells[0].Value = time;
            dgv_Content.Rows[index].Cells[1].Value = ip;
            dgv_Content.Rows[index].Cells[2].Value = data;
            Match ipRegex = Regex.Match(ip, @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
            if (ipRegex.Success)
            {
                if (!this.IPList.Items.Contains(ipRegex.Value))
                {

                    // 向listBox中插入数据 
                    this.IPList.Items.Add(ipRegex.Value);
                }
                connectsql.Insert(time, ipRegex.Value, data);//将数据存到数据库 
            }
        }

        //停止监听
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            ReceiveUdpClient.Close();
        }

        /// 

        /// 大小更改时，DataGridView列表的宽度改变 ///
        ///
        ///

        private void UDPSever_SizeChanged(object sender, EventArgs e)
        {
            dgv_Content.Columns[2].Width = dgv_Content.Columns[2].Width = dgv_Content.Width - dgv_Content.Columns[0].Width - dgv_Content.Columns[1].Width;
        }

        //筛选IP
        private void IPList_SelectedIndexChanged(object sender, EventArgs e)
        {

            //移除列表 
            dgv_Content.Columns.Remove("Time");
            dgv_Content.Columns.Remove("IP");
            dgv_Content.Columns.Remove("Content");
            try
            {
                DataTable dt = connectsql.Select(IPList.SelectedItem.ToString(), dgv_Content);
                dgv_Content.DataSource = dt;//绑定表 
                dgv_Content.Columns[0].Width = 150;
                dgv_Content.Columns[1].Width = 145;
                dgv_Content.Columns[2].Width = dgv_Content.Width - dgv_Content.Columns[0].Width - dgv_Content.Columns[1].Width;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //清空所有数据 
        private void btn_Clear_Click(object sender, EventArgs e)
        {
            dgv_Content.Rows.Clear();
            IPList.Items.Clear();
            tbx_Show.Text = "";
        }

        //关闭前删除数据表中的数据 
        private void UDPSever_FormClosing(object sender, FormClosingEventArgs e)
        {
            dgv_Content.DataSource = null;
            connectsql.Truncate();
        }
    }
}

connectsql.cs文件
 using System;
 using System.Collections.Generic; 
using System.Text; using System.Data; 
using System.Windows.Forms; 
using System.Data.SqlServerCe;
 namespace UDPListenPorts
{
    class ConnectSql
    {
        SqlCeConnection conn; //声明
        public SqlCeConnection Connect()
        {

            //string connectionStr = @"Data Source=GFOC4K9TL9GN0Z9;Initial Catalog=UDPListen;User ID=sa;Password=;Integrated Security=True;Connect Timeout=30";//连接SQLServer 
            string connectionStr = @"Data Source=E:\dk\程序\UDPListenPorts\UDPListenPorts\UDPListen.sdf";//Integrated Security=True;Connect Timeout=30//这里使用的是本地数据库 
            conn = new SqlCeConnection(connectionStr); return conn;
        }

        /// 

        /// 向表中插入所接收的数据 /// 

        ///时间 

        ///ip ///
        data public void Insert(string time, string ip, string data)
        {
            conn = Connect();
            string Insert_Str = "INSERT INTO [UDPlisten](Time,IP,Content) VALUES('" + time + "','" + ip + "','" + data + "')";
            SqlCeCommand cmd = new SqlCeCommand(Insert_Str, conn);//创建命令
            conn.Open();
            cmd.ExecuteNonQuery();//执行命令 
            conn.Close();
        }

        /// /// 从表中查询所需的数据 /// ///传入选中的IP ///传入要绑定的DataGridView控件名 ///
        public DataTable Select(string ip, DataGridView dgv)
        {
            conn = Connect();
            conn.Open();
            DataRow row; DataTable _table = CreateDataTable();
            if (ip == "IP列表")
            {
                string Select_Str = @"SELECT * FROM UDPlisten";
                SqlCeDataAdapter sda = new SqlCeDataAdapter();//用于填充 DataSet 和更新 SQL Server 数据库的一组数据命令和一个数据库连接 
                SqlCeCommand sqlComm = new SqlCeCommand(Select_Str, conn);
                //sqlComm.ExecuteNonQuery();
                SqlCeDataReader sdrr = sqlComm.ExecuteReader();
                while (sdrr.Read())
                {

                    //int count = sdrr.FieldCount; 
                    row = _table.NewRow();
                    row["Time"] = sdrr[0];
                    row["IP"] = sdrr[1];
                    row["Content"] = sdrr[2];
                    _table.Rows.Add(row);
                }
                int i = _table.Rows.Count;
            }
            else
            {
                try
                {
                    string Select_Str = @"SELECT Time,IP, Content FROM UDPlisten WHERE IP=" + "'" + ip.ToString() + "'";
                    SqlCeDataAdapter sda = new SqlCeDataAdapter();//用于填充 DataSet 和更新 SQL Server 数据库的一组数据命令和一个数据库连接 
                    SqlCeCommand sqlComm = new SqlCeCommand(Select_Str, conn);

                    //sqlComm.ExecuteNonQuery();
                    SqlCeDataReader sdrr = sqlComm.ExecuteReader();
                    while (sdrr.Read())
                    {

                        //int count = sdrr.FieldCount; 
                        row = _table.NewRow();
                        row["Time"] = sdrr[0];
                        row["IP"] = sdrr[1];
                        row["Content"] = sdrr[2];
                        _table.Rows.Add(row);
                    }
                    int i = _table.Rows.Count;

                    //dgv.DataSource = _table; 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return _table;
        }

        //Create DataTable Rows and Column 
        private static DataTable CreateDataTable()
        {

            //Create new DataTable DataTable _table = new DataTable();
            //Declare DataColumn and DataRow variables;
            DataColumn column;
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Time";
            _table.Columns.Add(column);
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "IP";
            _table.Columns.Add(column);
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String"); column.ColumnName = "Content";
            _table.Columns.Add(column);
            return _table;
        }
        public void Delete()
        { }
        //删除表的内容
        public void Truncate()
        {
            conn = Connect();
            string Insert_Str = "DELETE FROM UDPlisten";
            SqlCeCommand cmd = new SqlCeCommand(Insert_Str, conn);//创建命令
            conn.Open();
            try
            {
                cmd.ExecuteNonQuery();//执行命令
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            conn.Close();
        }
    }
}