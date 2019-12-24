using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;

namespace ShowNow
{
    public delegate void MqttReceiveMsgDelegate(MqttHelper mqtt, MqttMsgPublishEventArgs e);
    public delegate void MqttReceiveByteMsgDelegate(MqttHelper mqtt, byte[] msg);

    public delegate void MqttSendMsgDelegate(bool result);
    public delegate void MqttConnectDelegate(MqttHelper mqtt, bool result);
    public delegate void ResultDelegate<T>(T t);
    public delegate void ResultDelegate<T, M>(T t, M m);
    public class MqttHelper
    {
        #region 单例
        private static readonly object InstanceLock = new object();
        private static MqttHelper _instance;

        public static MqttHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MqttHelper();
                        }
                    }
                }
                return _instance;
            }
        }

        public MqttHelper()
        {
            Trace.TraceLevel = TraceLevel.Verbose;
            Trace.TraceListener = new WriteTrace(WriteTrace);
        }
        public static void WriteTrace(string format, params object[] args)
        {
            //Debug.Log(String.Format(format, args));
        }
        #endregion

        #region 收发消息
        private static readonly object MessageQueueLock = new object();
        private static readonly object MessageSendingQueueLock = new object();
        private static readonly object SubscribeingQueueLock = new object();

        private Queue<MessageBean> messageQueue = new Queue<MessageBean>();
        private List<MessageBean> messageSendingQueue = new List<MessageBean>();
        private List<SubscribeBean> subscribeingQueue = new List<SubscribeBean>();
        
        public event MqttReceiveMsgDelegate OnReceiveMsg;
        public event ResultDelegate<MqttHelper, bool> onConnectStateChange;
        #endregion

        private MqttClient mqttClient = null;
        private string clientId;
        #region 通信逻辑
        public void Connect(string serverIp, int serverPort, string userName, string PassWord, string clientId, MqttConnectDelegate onConnect)
        {
            this.clientId = clientId;
            new Task(() =>
            {
            try
            {
#if !UNITY_EDITOR && UNITY_WSA
                    mqttClient = new MqttClient(serverIp, serverPort, false, MqttSslProtocols.None);
#else
                    mqttClient = new MqttClient(serverIp, serverPort, false, null, null, MqttSslProtocols.None);
#endif

                    mqttClient.MqttMsgPublishReceived += MsgPublishReceived;
                    mqttClient.MqttMsgPublished += MsgPublished;
                    mqttClient.ConnectionClosed += ConnectionClosed;
                    mqttClient.MqttMsgSubscribed += MqttMsgSubscribed;

                    byte ret = mqttClient.Connect(clientId, userName, PassWord, true, 30);
                    onConnect?.Invoke(this, true);
                    onConnectStateChange?.Invoke(this, true);
                    Debug.Log("Mqtt Connect:" + ret);
                }
                catch (Exception e)
                {
                    onConnect?.Invoke(this, false);
                    onConnectStateChange?.Invoke(this, false);

 
                }

            }).Start();
        }


       
        private void MsgPublished(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishedEventArgs e)
        {
            ushort messageId = e.MessageId;
            lock (MessageSendingQueueLock)
            {
                foreach (MessageBean m in messageSendingQueue)
                {
                    if (m.msgId == messageId)
                    {
                        m.onSend?.Invoke(e.IsPublished);
                        messageSendingQueue.Remove(m);
                        break;
                    }
                }
            }
        }
        private void ConnectionClosed(object sender, EventArgs e)
        {
            Debug.Log("MqttClient_ConnectionClosed");

            onConnectStateChange?.Invoke(this, false);
        }
        private void MqttMsgSubscribed(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {
            lock (SubscribeingQueueLock)
            {
                foreach (SubscribeBean bean in subscribeingQueue)
                {
                    if (bean.msgId == e.MessageId)
                    {
                        bean.resultDelegate?.Invoke(true);
                        subscribeingQueue.Remove(bean);
                        break;
                    }
                }
            }
        }
        public void Disconnect()
        {
            this.clientId = null;

            try
            {
                if (mqttClient != null)
                {
                    if (mqttClient.IsConnected)
                    {
                        mqttClient.Disconnect();
                    }
                    mqttClient = null;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        public void SubscribeMsg(string topic, ResultDelegate<bool> resultDelegate)
        {
            string[] topics = { topic };
            byte[] qosLevels = { 0 };
            ushort messageId = mqttClient.Subscribe(topics, qosLevels);

            SubscribeBean bean = new SubscribeBean();
            bean.topic = topic;
            bean.msgId = messageId;
            bean.resultDelegate = resultDelegate;
            lock (SubscribeingQueueLock)
            {
                subscribeingQueue.Add(bean);
            }
        }
        public void UnSubscribeMsg(string topic)
        {
            string[] topics = { topic };
            if (mqttClient != null && mqttClient.IsConnected)
                mqttClient.Unsubscribe(topics);
        }
        public void PublishMsg(string topic,byte[] msg, MqttSendMsgDelegate onSend)
        {
            //Debug.Log("PublishMsg:" + msg);
            if (mqttClient == null) { return; }
            MessageBean m = new MessageBean();
           
            m.msg =  msg; ;
            m.onSend = onSend;

            ushort msgid = mqttClient.Publish(topic, m.msg, (byte)0, false);
            m.msgId = msgid;

            lock (MessageSendingQueueLock)
            {
                messageSendingQueue.Add(m);
            }
        }
        private void MsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            OnReceiveMsg?.Invoke(this,e);
        }
        #endregion




        #region 消息体
        private class MessageBean
        {
            public ushort msgId { set; get; }
            public string topic { set; get; }
            public byte[] msg { set; get; }

            public MqttSendMsgDelegate onSend { set; get; }
        }
        private class SubscribeBean
        {
            public string topic { set; get; }
            public ResultDelegate<bool> resultDelegate { set; get; }
            public ushort msgId { set; get; }
        }
        #endregion
    }
}