using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFramework;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
using UnityEngine.XR.WSA;

namespace ShowNow
{
    public delegate void OnJoinRoomHandler(string userID, bool isSuccess);
    public delegate void OnUploadWorldAnchorHandler();
    public delegate void OnDownloadWorldAnchorHandler();

    public class NetHelper:ScriptSingleton<NetHelper>
    {
       
        #region 
        public event OnJoinRoomHandler OnJoinRoom;
        public event OnUploadWorldAnchorHandler OnUploadWorldAnchor;
        public event OnDownloadWorldAnchorHandler OnDownloadWorldAnchor;
        #endregion



        public bool IsConnect = false;
        public string UserID="";
        public string MessageTopic = "RoomMessage/";
        public string TopicJoinRoom = "RoomMessage/"+MessageType.JoinRoom;
        public string IsCreater="";
        public string Topic1 = "";

        public string ServerIP = "192.168.5.139";
        //public string ServerIP = "122.228.19.14";
        
        public int ServerMqttPort = 1883;
        public int HttpPort = 9090;

        LockedQueue<MqttMsgPublishEventArgs> receviceMessages=new LockedQueue<MqttMsgPublishEventArgs>();
        LockedQueue<MqttMsgPublishEventArgs> publishMessages=new LockedQueue<MqttMsgPublishEventArgs>();

        private MqttHelper Mqtt = null;
        #region 同步逻辑
       
        public void JoinRoom()
        {
            ResourceManager.Instance.log.text += " JoinRoom";
            UserID = (DateTime.Now.Hour * 10000 + DateTime.Now.Minute * 100 + DateTime.Now.Second).ToString();
            Debug.Log(UserID);
            Mqtt = new MqttHelper();
            Mqtt.OnReceiveMsg += receiveMsg;
            Mqtt.Connect(ServerIP, ServerMqttPort, UserID, "111", UserID, new MqttConnectDelegate((MqttHelper t, bool result) =>
            {
                if (result)
                {

                    ResourceManager.Instance.log.text += " Connect";
                    Mqtt.SubscribeMsg(MessageTopic+"#", null);
                    IsConnect = true;
                    OnJoinRoom?.Invoke(UserID, true);
                    //连接完成,延时三秒检查有无其他主机后决定是否需要创建房间
                   // Invoke("createRoom", 3);
                }
                else
                {
                    OnJoinRoom?.Invoke(UserID, false);
                }
                    

            }));
        }
        bool joinedRoom = false;
        void createRoom()
        {
            if (joinedRoom) return;

            InvokeRepeating("publishJoinRoomMessage", 0, 0.5f);  //不断发送加入房间消息
            //创建房间逻辑

            

        }
        void publishJoinRoomMessage()
        {
            Message m = new Message();
            m.MessageType = MessageType.LeaveRoom;
            m.Msg = UserID + "已经创建房间";
            string json = JsonConvert.SerializeObject(m);
            Mqtt.PublishMsg(TopicJoinRoom, Encoding.UTF8.GetBytes(json),null);
        }
        public void LeaveRoom()
        {
            if (Mqtt == null)
            {
                return;
            }
            Message m = new Message();
            m.MessageType = MessageType.LeaveRoom;
            m.Msg = UserID + "离开房间";
            string json= JsonConvert.SerializeObject(m);
            Mqtt.PublishMsg(MessageTopic+ MessageType.LeaveRoom, Encoding.UTF8.GetBytes(json), new MqttSendMsgDelegate((bool res) =>
            {
                Mqtt.Disconnect();
                Mqtt = null;
            }));


        }

        List<MonoBehaviour> cmdHandlerList = new List<MonoBehaviour>();
        public void SyncCMD(string id, string[] param)
        {
            if (Mqtt == null) return;
            Message m = new Message();
            m.MessageType = MessageType.SyncCmd;
            m.Msg = UserID + "同步操作";
            m.Arg1 = id;
            m.Arg2 = param;
            string json = JsonConvert.SerializeObject(m);
            //Debug.Log(MessageTopic + MessageType.SyncCmd);
            //Debug.Log(json);
            Mqtt.PublishMsg(MessageTopic + MessageType.SyncCmd, Encoding.UTF8.GetBytes(json), null);
        
        }
        public void OnSyncCMD(string id, string[] param)
        {
            ResourceManager.Instance.log.text += " OnSyncCMD";
            try
            {
                MethodInfo mi = null;
                MonoBehaviour hander = null;
                for (int i = 0; i < cmdHandlerList.Count; i++)
                {
                    mi = cmdHandlerList[i].GetType().GetMethod(id);
                    if (mi != null)
                    {
                        hander = cmdHandlerList[i];
                        break;
                    }
                }
                object[] parameters = null;

               // ParameterInfo[] parameterInfos = mi.GetParameters();
                if ( param != null && !(param.Length < 1))
                {
                    parameters = param;
                }

                if (mi != null)
                {
                    mi.Invoke(hander, parameters);
                }
                else
                {
                    Debug.Log("SyncInterface:" + id + "->找不到！");
                }
            }
            catch (Exception e)
            {
                Debug.Log("SyncInterface:" + id + "->方法处理出错！" + e.Message);
            }
        }
        public void RegistCmdHandler(MonoBehaviour handler)
        {
            cmdHandlerList.Add(handler);
        }
        public void UnRegistCmdHandler(MonoBehaviour handler)
        {
            cmdHandlerList.Remove(handler);
        }
        
        [HideInInspector]
        public List<byte> AnchorData = new List<byte>();   //世界坐标系数据
        public void UploadWorldAnchor()
        {
#if UNITY_WSA
            WorldAnchorTransferBatch watb = new WorldAnchorTransferBatch();
            WorldAnchor worldAnchor = ResourceManager.Instance.World.AddComponent<WorldAnchor>();

            watb.AddWorldAnchor(NetHelper.Instance.UserID, worldAnchor);
            WorldAnchorTransferBatch.ExportAsync(watb,
                new WorldAnchorTransferBatch.SerializationDataAvailableDelegate((byte[] data) => {
                    AnchorData.AddRange(data);
                }),
                new WorldAnchorTransferBatch.SerializationCompleteDelegate((SerializationCompletionReason status) => {
                    if (status == SerializationCompletionReason.Succeeded)
                    {
                        Message m = new Message();
                        m.MessageType = MessageType.UploadWorldAnchor;
                        m.Arg3 = AnchorData.ToArray();
                        string json = JsonConvert.SerializeObject(m);
                        Mqtt.PublishMsg(MessageTopic + MessageType.UploadWorldAnchor, Encoding.UTF8.GetBytes(json), null);

                        Debug.Log("锚点准备好了");
                    }
                    else
                    {
                        Debug.Log("锚点导出失败，低于限制！再次尝试...");
                        UploadWorldAnchor();
                    }
                }));
#else
            Debug.Log("不是WSA平台,无法同步世界坐标系");
#endif



        }
        public void DownloadWorldAnchor(byte[] data) {
#if UNITY_WSA
        WorldAnchorTransferBatch.ImportAsync(data, new WorldAnchorTransferBatch.DeserializationCompleteDelegate(
            (SerializationCompletionReason status, WorldAnchorTransferBatch wat) => {
                if (status == SerializationCompletionReason.Succeeded && wat.GetAllIds().Length > 0)
                {
                    // MYDialog.Instance.Write("\r\n导入完成！");
                    WorldAnchor existingAnchor = ResourceManager.Instance.World.GetComponent<WorldAnchor>();
                    if (existingAnchor != null)
                    {
                        //删除旧的锚点数据
                        DestroyImmediate(existingAnchor);
                    }
                    WorldAnchor anchor = wat.LockObject(wat.GetAllIds()[0], ResourceManager.Instance.World);
                    // MYDialog.Instance.Write("新锚点建立完成！\r\n待此文字位置与发送锚点的hololens看到的位置相同时\r\n锚点同步过程完成");
                    Message m = new Message();
                    m.MessageType = MessageType.SyncWorldAnchorComplete;
                    m.Msg = UserID + "已经同步坐标系完成";
                    string json = JsonConvert.SerializeObject(m);
                    Mqtt.PublishMsg(MessageTopic + MessageType.SyncWorldAnchorComplete, Encoding.UTF8.GetBytes(json), null);


                }
                else
                {
                    Debug.Log("锚点导入失败！");
                }



            }));
#else
            Debug.Log("非Hololens无法入锚点数据");
#endif  
        }

        #endregion
        #region 
        void receiveMsg(MqttHelper mqtt, MqttMsgPublishEventArgs e)
        {
            receviceMessages.Enqueue(e);
        }




        #endregion
        #region unity

        private void Awake()
        {
        }

        private void Update()
        {
            while (receviceMessages.Count() > 0)
            {
                MqttMsgPublishEventArgs e= receviceMessages.Dequeue();
                if (e.Topic.Contains(MessageTopic))
                {
                    if (e.Topic.Equals(TopicJoinRoom))
                    {
                        joinedRoom = true; //已经创建房间,直接加入房间
                    }
                    else if (e.Topic.Equals(MessageTopic + MessageType.LeaveRoom))
                    {
                        Debug.Log(Encoding.UTF8.GetString(e.Message));
                    }
                    else if (e.Topic.Equals(MessageTopic + MessageType.SyncCmd))
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(e.Message));

                        OnSyncCMD(m.Arg1, m.Arg2);
                    }
                    else if (e.Topic.Equals(MessageTopic + MessageType.UploadWorldAnchor))
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(e.Message));
                        DownloadWorldAnchor(m.Arg3);
                        //OnDownloadWorldAnchor();
                    }
                    else if (e.Topic.Equals(MessageTopic + MessageType.SyncWorldAnchorComplete))
                    {
                        Message m = JsonConvert.DeserializeObject<Message>(Encoding.UTF8.GetString(e.Message));
                        Debug.Log(m.Msg);
                    }
                    //else if (e.Topic.Equals(MessageTopic + MessageType.UploadWorldAnchor))
                    //{


                    //} 
                }
                else if (e.Topic.Contains(Topic1))
                {

                }
            }
            
        }
        #endregion

    }
    public enum MessageType
    {
        Invalid=0,
        JoinRoom,
        LeaveRoom,
        SyncCmd,
        UploadWorldAnchor,
        SyncWorldAnchorComplete,
    }
    public class Message
    {
        public MessageType MessageType=0;
        public string Msg=null;
        public string Arg1=null;
        public string[] Arg2=null;
        public byte[] Arg3=null;
    }

}



