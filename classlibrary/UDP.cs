/****
UDP通信クラス Hisashi Ishihara 2018/5/21更新
送信と受信のクライアントの生成，削除，送受信機能を提供するC#用クラスです．
送信は随時要求に応じて実施されます．
受信は別スレッドの無限ループ内で実施されます．

---使い方---
0. クラスの導入
    Visual Studioのソリューションエクスプローラでこのクラスファイルを追加し，namespaceを修正します

1. 最低限の準備
    ・メインのプログラムでインスタンス生成．
          public UDP commUDP = new UDP();     //UDPインスタンスの生成

    ・その後，良いところで初期設定．
          commUDP.init(int型の送信用ポート番号, int型の送信先ポート番号, int型の受信用ポート番号);

２．すると下記の機能が使えるようになります．
    ・UDP送信　commUDP.send(string型のメッセージ)
    ・UDP受信開始　commUDP.startReceive()
    ・UDP受信停止  commUDP.stopReceive()
    ・UDP受信した文字列の取得　commmUDP.rcvMsg
    ・終了処理　commUDP.end()
****/


using System.Text;
using System.Net;//for UDP
using System.Net.Sockets; //for UDP
using System.Threading;//for Interlocked

namespace ProptotypeControler //namespaceは本体に合わせて要修正
{
    public class UDP
    {
        private UdpClient udpForSend; //送信クライアント
        private string remoteHost = "localhost";//送信先のIP
        private int remotePort;//送信先のポート

        private UdpClient udpForReceive; //受信クライアント
        public string rcvMsg = "ini";//受信メッセージ格納用
        private System.Threading.Thread rcvThread; //受信用スレッド

        public UDP()
        {
            
        }

        public bool init(int port_snd, int port_to, int port_rcv) //UDP設定（送受信用ポートを開いて受信用スレッドを生成）
        {
            try
            {
                udpForSend = new UdpClient(port_snd); //送信用ポート
                remotePort = port_to; //送信先ポート
                udpForReceive = new UdpClient(port_rcv); //受信用ポート
                rcvThread = new System.Threading.Thread(new System.Threading.ThreadStart(receive)); //受信スレッド生成
                return true;
            }
            catch
            {
                return false;
            }


        }

        public void send(string sendMsg) //文字列を送信用ポートから送信先ポートに送信
        {
            try
            {
                byte[] sendBytes = Encoding.ASCII.GetBytes(sendMsg);
                udpForSend.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);
            }
            catch{ }
        }

        public void receive() //受信スレッドで実行される関数
        {
            IPEndPoint remoteEP = null;//任意の送信元からのデータを受信
            while (true)
            {
                try
                {
                    byte[] rcvBytes = udpForReceive.Receive(ref remoteEP);
                    Interlocked.Exchange(ref rcvMsg, Encoding.ASCII.GetString(rcvBytes));
                }
                catch { }
            }
        }

        public void start_receive() //受信スレッド起動
        {
            try
            {
                rcvThread.Start();
            }
            catch { }
            
        }

        public void stop_receive() //受信スレッドを停止
        {
            try
            {
                rcvThread.Interrupt();
            }
            catch { }
        }

        public void end() //送受信用ポートを閉じて受信用スレッドも廃止
        {
            try
            {
                udpForReceive.Close();
                udpForSend.Close();
                rcvThread.Abort();
            }
            catch { }
        }
    }
}
