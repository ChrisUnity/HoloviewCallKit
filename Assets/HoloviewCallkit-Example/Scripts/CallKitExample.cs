using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;  
using Holoview;
using AOT;
using System;

#if UNITY_EDITOR ||  UNITY_WSA

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA.Sharing;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
#else
using UnityEngine.VR.WSA.Sharing;
using UnityEngine.VR.WSA;
using UnityEngine.VR.WSA.Persistence;
#endif
#endif

public class CallKitExample : MonoBehaviour {
    public GameObject localVideoParent;
    public GameObject remoteVideoParent;

    public long SelfUserId;
    public long TargetUserId;

    public string LoginAccount;
    public string LoginPassword;

	public Text HintText;

    private static VideoRender localVideoRender;
    private static VideoRender remoteVideoRender;

	private static string userToken;
	private User loginUser;
	private long inviterId;
	private string callid = "";

    private string showMsg = "";
	public enum MineState
	{
		NONE,
		INIT,
		TOKEN,
		LOGIN,
		CONNECT,
		DISCONNECT,
		CALLCONN,
		CALLDISCONN,
		CALLCOMMING,
		KICKEDOFF,
		ACCEPT,
		HANG,
        LIVECONN,
        LIVEDISCONN,
        LIVEBULLUT,
        ROOMNOTIFY,
        RECVMSG,
        JOINROOM,

	}
	private MineState state = MineState.NONE;


	// Use this for initialization
	void Start () {


        remoteVideoRender = remoteVideoParent.GetComponent<VideoRender>();
        localVideoRender = localVideoParent.GetComponent<VideoRender>();

        HoloviewCallKit.instance().onInitResult += MP_OnInitResult;
        HoloviewCallKit.instance().onStateChange += MP_OnStateChange;
        HoloviewCallKit.instance().onGetTokenResult += MP_OnGetTokenResult;

        HoloviewCallKit.instance().onConnectResult += MP_onConnectResult; ;
        HoloviewCallKit.instance().onRecvTextMessage += MP_OnRecvTextMessage;
        HoloviewCallKit.instance().onRecvImageMessage += MP_OnRecvImageMessage;
        HoloviewCallKit.instance().onRecvVideoMessage += MP_OnRecvVideoMessage;
        HoloviewCallKit.instance().onRecvVoiceMessage += MP_onRecvVoiceMessage;
        HoloviewCallKit.instance().onRecvLocationMessage += MP_onRecvLocationMessage;

        //HoloviewCallKit.instance().onRecvCall += MP_OnRecvCall;
        HoloviewCallKit.instance().onCallConnect += MP_OnCallConnect;
        HoloviewCallKit.instance().onCallDisconnect += MP_OnCallDisconnect;
        HoloviewCallKit.instance().onRemoteUserEnter += MP_OnRemoteUserEnter;
        HoloviewCallKit.instance().onRemoteUserLeft += MP_OnRemoteUserLeft;


        HoloviewCallKit.instance().onRecvCallFull += MPFULL_onRecvCallFull;
        HoloviewCallKit.instance().onLoginResult += MPFULL_onLoginResult;
        HoloviewCallKit.instance().onFriendResult += MPFULL_onFriendResult;
        HoloviewCallKit.instance().onGroupResult += MPFULL_onGroupResult;
#if !UNITY_IPHONE && !UNITY_ANDROID
		//TODO:添加安卓和ios接口
        HoloviewCallKit.instance().onAssetList += MPFULL_onAssetList;
        HoloviewCallKit.instance().onAssetRealTimeData += MPFULL_onAssetRealTimeData;
        HoloviewCallKit.instance().onRecvCmdMessage += MPFULL_onRecvCmdMessage;
        
#endif

		HoloviewCallKit.instance().onCallRoomNotify += MPFULL_onCallRoomNotify;
		HoloviewCallKit.instance().onJoinRoomResult += MPFULL_onJoinRoomResult;

#if !UNITY_EDITOR && UNITY_WSA
        HoloviewCallKit.instance().onSelfTranslate += MPFULL_onSelfTranslate;
        HoloviewCallKit.instance().onCallAudioData += MPFULL_onCallAudioData;
        HoloviewCallKit.instance().onRecvArRectWithMartix += MPFULL_onRecvArRectWithMartix;
        HoloviewCallKit.instance().onRecvArCircleWithMartix += MPFULL_onRecvArCircleWithMartix;
        HoloviewCallKit.instance().onRecvArArrowWithMartix += MPFULL_onRecvArArrowWithMartix;
        HoloviewCallKit.instance().onLiveConnect += MPFULL_onLiveConnect;
        HoloviewCallKit.instance().onLiveDisconnect += MPFULL_onLiveDisconnect;
        HoloviewCallKit.instance().onLiveBullet += MPFULL_onLiveBullet;
#endif
        OnTestInit();
	}

   

#if !UNITY_EDITOR && UNITY_WSA

    private void MPFULL_onSelfTranslate(string msg)
    {
    }

    private void MPFULL_onCallAudioData(long uId, byte[] buf, int bits_per_sample, int sample_rate, int number_of_channels, int number_of_frames)
    {
    }

    private void MPFULL_onRecvArRectWithMartix(float[] martixCoodinate, float[] martixViewTransform, float[] martixProjection, double left, double top, double right, double bottom, long timestamp)
    {
    }

    private void MPFULL_onRecvArCircleWithMartix(float[] martixCoodinate, float[] martixViewTransform, float[] martixProjection, double cx, double cy, double rad, long timestamp)
    {
    }

    private void MPFULL_onRecvArArrowWithMartix(float[] martixCoodinate, float[] martixViewTransform, float[] martixProjection, double startx, double starty, double endx, double endy, long timestamp)
    {
    }

    private void MPFULL_onLiveConnect(long roomId)
    {
    }

    private void MPFULL_onLiveDisconnect()
    {
    }

    private void MPFULL_onLiveBullet(ConversationType conversationType, long targetId, long senderId, string text)
    {
    }
#endif

    
    private void MPFULL_onRecvCallFull(long inviterId, string callId, long targetId, ConversationType conversationType, CallMediaType mediaType, CallEngineType engineType, long[] participates)
    {
        Debug.Log("MPFULL_onRecvCallFull callId: " + callId);
        this.callid = callId;
        this.inviterId = inviterId;
        state = MineState.CALLCOMMING;
        List<long> subList = new List<long>();
        subList.Add(inviterId);
        subList.Add(_longinUser);

#if UNITY_IPHONE || UNITY_ANDROID
        HoloviewCallKit.instance().AcceptCall(callid);
#else
        HoloviewCallKit.instance().AcceptCall(callid, subList, true, true);
#endif
    }

    private void MPFULL_onLoginResult(bool result, User user)
    {
		Debug.Log("MPFULL_onLoginResult result: " + result + " userId:" + (user == null? 0 : user.id));
        if (result)
        {
            _longinUser = user.id;
            state = MineState.CONNECT;
        }
    }

    private void MPFULL_onFriendResult(bool result, List<User> friendList)
    {
    }

    private void MPFULL_onGroupResult(bool result, Group grp, List<User> memberList)
    {
        Debug.Log("MPFULL_onGroupResult:" + grp.id + " " + grp.name +" " + grp.portrait);
        String memlist = "";
        foreach(User u in memberList)
        {
            memlist += " id:" + u.id + " name:" + u.name + " portrait:" + u.portrait + "\n";
            
        }
        Debug.Log("userlist:\n" + memlist);

    }
#if !UNITY_IPHONE && !UNITY_ANDROID
    private void MPFULL_onAssetList(bool result, List<ShowNowSdk.model.Asset> assetList)
    {
    }

    private void MPFULL_onAssetRealTimeData(ShowNowSdk.model.AssetRealTimeData assetRealTimeData)
    {
    }

    private void MPFULL_onRecvCmdMessage(long senderId, long targetId, ConversationType conversationType, string cmd)
    {
    }
#endif

    private void MPFULL_onJoinRoomResult(bool result, long rid, string roomName)
    {
        this.state = MineState.JOINROOM;
        this.showMsg = "JoinRoom result:" + result + " rid:" + rid + " roomName:" + roomName;
    }

    private void MPFULL_onCallRoomNotify(long senderId, long targetId, User operatorUser, List<User> targetUser, string operation, string showText)
    {
        this.state = MineState.JOINROOM;
        this.showMsg = showText;
    }

    long _longinUser = 0;
    private void MP_onConnectResult(bool result, long userId)
    {
        Debug.Log("MP_OnConnectResult result: " + result + " userId:" + userId);
        if (result)
        {
            _longinUser = userId;
            state = MineState.CONNECT;
        }
    }
    

    void  MP_OnInitResult(bool result)
	{
		Debug.Log("MP_OnInitResult result: " + result);
		if (result) {
			state = MineState.INIT;
		}
	}

	void MP_OnStateChange(PluginState state){
		Debug.Log("MP_OnStateChange state: " + state.ToString());

		switch (state) {
		case PluginState.INITED:
			this.state = MineState.INIT;
			break;
		case PluginState.UNINITED:
			break;
		case PluginState.CONNECT:
			this.state = MineState.CONNECT;
			break;
		case PluginState.DISCONNECT:
			this.state = MineState.DISCONNECT;
			break;
		case PluginState.ERROR:
			break;
		case PluginState.KICKEDOFF:
			this.state = MineState.KICKEDOFF;
			break;

		}
	}
	void MP_OnGetTokenResult (bool result, string token)
	{
		Debug.Log("MP_OnGetTokenResult result: " + result + " token:" + token);

		userToken = token;
		if (result) {
			state = MineState.TOKEN;
		}
	}
	void MP_OnRecvTextMessage (long senderId, long targetId, ConversationType conversationType, string content)
	{
        this.state = MineState.RECVMSG;
        this.showMsg = "TextMessage:" + content + " from:" + senderId;
		Debug.Log("MP_OnRecvTextMessage showMsg: " + showMsg );

    }

	void MP_OnRecvImageMessage (long senderId, long targetId, ConversationType conversationType, string localThumbUrl, string localUrl, string remoteThumbUrl, string remoteUrl)
	{
        this.state = MineState.RECVMSG;
        this.showMsg = "ImageMessage:" + remoteUrl + " from:" + senderId;
		Debug.Log("MP_OnRecvImageMessage showMsg: " + showMsg );

    }

	void MP_OnRecvVideoMessage (long senderId, long targetId, ConversationType conversationType, string thumbUrl, string videoUrl)
	{
        this.state = MineState.RECVMSG;
        this.showMsg = "VideoMessage:" + videoUrl + " from:" + senderId;
		Debug.Log("MP_OnRecvVideoMessage showMsg: " + showMsg );


    }
	private void MP_onRecvLocationMessage(long sendId, long targetId, ConversationType conversationType, double lat, double lng, string poi, string thumbUrl)
    {
        this.state = MineState.RECVMSG;
        this.showMsg = "LocationMessage :(lat:" + lat +",lng:"+ lng+")" + " from:" + sendId + " poi:" + poi;
		Debug.Log("MP_onRecvLocationMessage showMsg: " + showMsg );

    }

	private void MP_onRecvVoiceMessage(long sendId, long targetId, ConversationType conversationType, string audioUrl, int audioLength)
    {
        this.state = MineState.RECVMSG;
        this.showMsg = "VoiceMessage:" + audioUrl + " from:" + sendId;
		Debug.Log("MP_onRecvVoiceMessage showMsg: " + showMsg );


    }
    //void MP_OnRecvCall (long inviterId, string callId, CallMediaType mediaType)
    //{

    //   }

    void MP_OnCallConnect (string callId)
	{
		Debug.Log("MP_OnCallConnect callId: " + callId );
		this.callid = callId;
		state = MineState.CALLCONN;
	}
    CallDisconnectedReason mDisconnectReason = CallDisconnectedReason.HANGUP;

	void MP_OnCallDisconnect (CallDisconnectedReason reason)
	{
		Debug.Log("MP_OnCallDisconnect reason: " + reason.ToString() );
        mDisconnectReason = reason;
        state = MineState.CALLDISCONN;
	}

	void MP_OnRemoteUserEnter (long userId, CallMediaType mediaType)
	{
		Debug.Log("MP_OnRemoteUserEnter userId: " + userId);

	}

	void MP_OnRemoteUserLeft (long userId, CallDisconnectedReason reason)
	{
		Debug.Log("MP_OnRemoteUserLeft userId: " + userId);

	}
    
    void Update () {
		string showText = "";
		switch(state){
		    case MineState.NONE:
			    break;
		    case MineState.INIT:
			    showText = "init done";
			    break;
		    case MineState.TOKEN:
			    showText = "token:" + userToken;

			    break;
		    case MineState.LOGIN:
			    showText = "longin done token:" + userToken  + " id:" + loginUser.id + " name:" + loginUser.name + " portrait:" + loginUser.portrait;

			    break;
		    case MineState.CONNECT:
			    showText = "connected to msg server";
			    break;
		    case MineState.DISCONNECT:
			    showText = "disconnected from msg server";

			    break;
		    case MineState.CALLCONN:
			    showText = "call connect";

			    break;
		    case MineState.CALLDISCONN:
			    showText = "call disconnect :" + mDisconnectReason.ToString();

			    break;
		    case MineState.CALLCOMMING:
			    showText = "call comming callid:" + callid + "inviterId:" + inviterId;

			    break;
		    case MineState.KICKEDOFF:
			    showText = "you are kicked off.";

			    break;
		    case MineState.ACCEPT:
			    showText = "accept call.";

			    break;
		    case MineState.HANG:
			    showText = "hang call.";

			    break;
            case MineState.LIVECONN:
                showText = "live started.";
                break;
            case MineState.LIVEDISCONN:
                showText = "live stopped.";
                break;
            case MineState.LIVEBULLUT:
                //showText = "BULLET:" + liveBullet;
                break;
            case MineState.RECVMSG:
                showText = this.showMsg;
                break;
            case MineState.ROOMNOTIFY:
                showText = this.showMsg;

                break;
            case MineState.JOINROOM:
                showText = this.showMsg;

                break;
        }

		HintText.text =  showText;
	}

    private void OnDestroy()
    {
        if(_longinUser != 0)
        {
            HoloviewCallKit.instance().Disconnect();
        }
    }
    public void OnTestInit(){
       // string wifiName = HoloviewCallKit.instance().GetWifiName();
       // Debug.Log("wifiName :" + wifiName);

        Debug.Log ("OnTestInit");
#if !UNITY_EDITOR && UNITY_WSA
		HoloviewCallKit.instance().SetServerType(0);

        IntPtr ptr = WorldManager.GetNativeISpatialCoordinateSystemPtr();
        if (ptr != IntPtr.Zero)
            HoloviewCallKit.instance().SetNativeISpatialCoordinateSystemPtr(ptr);
#endif
        HoloviewCallKit.instance().Init ("000001", "1111111");
	}

	public void OnTestGetToken(){

        Debug.Log ("OnTestGetToken");
		long selfid = SelfUserId;
		Debug.Log ("OnTestGetToken selfId:" + selfid);
		HoloviewCallKit.instance ().GetToken (selfid, "yzh", "http://portrait.jpg");

    }

    public void OnTestLogin(){
		Debug.Log ("OnTestLogin");
		//string phone = AccountText.text;

        HoloviewCallKit.instance().Login(LoginAccount, LoginPassword);


    }

    public void OnTestLoginByToken()
    {
		SelfUserId = 1000149880598;
		TargetUserId = 1000149880627;

		if (localVideoRender)
		{
			localVideoRender.UserID = 1000149880598;
		}
		if (remoteVideoRender)
		{
			remoteVideoRender.UserID = 1000149880627;
		}

		HoloviewCallKit.instance().LoginByToken("MmV/9/dp/9OEqNkmS9q6MnmoYgJYJw563xOAVc55obND83KJLa8WRAG7jQ4fmHy4zmzyogcMSDROzsXsV+gAaoBAKSZXdZJoIaT+D/Io1YpE4Todi7M4WUBq6da3L12I", "000005");
    }

    public void OnTestJoinRoom()
    {
        HoloviewCallKit.instance().JoinCallRoom("SO000000002405");
    }
    public void OnTestConnect(){
		Debug.Log ("OnTestConnect");

        HoloviewCallKit.instance().Connect (userToken);

	}


	public void OnTestDisconnect(){
		Debug.Log ("OnTestDisconnect");

        HoloviewCallKit.instance ().Disconnect ();

	}

	public void OnTest(){
		Debug.Log ("OnTest");


	}

    public void OnTestCall()
    {
        Debug.Log("OnTestCall");

        long targetId = TargetUserId;
        List<long> inviteUsers = new List<long>();
        inviteUsers.Add(targetId);
        List<long> subUsers = new List<long>();
        subUsers.Add(targetId);
        subUsers.Add(_longinUser);
        Debug.Log("subUsers:" + _longinUser + ":" + targetId);
        
#if UNITY_IPHONE || UNITY_ANDROID
		long[] inviteUsersArr = {targetId};

		callid = HoloviewCallKit.instance().CallFull(ConversationType.P2P, targetId, inviteUsersArr, CallMediaType.VIDEO, CallEngineType.ENGINE_TYPE_AR);

#else
        callid = HoloviewCallKit.instance().CallFull(ConversationType.P2P, targetId, inviteUsers, subUsers, true, TranslateMode.NONE, CallMediaType.VIDEO, false, true);

#endif
    }

    public void OnTestHang(){
		Debug.Log ("OnTestHang");
		state = MineState.HANG;

		HoloviewCallKit.instance ().HangCall (callid);


    }
    public void OnTestLive()
    {
        Debug.Log("OnTestLive");
#if !UNITY_EDITOR && UNITY_WSA
        HoloviewCallKit.instance().StartLive("Hololens Live", true, true);
#endif
    }

    public void OnTestStopLive()
    {
        Debug.Log("OnTestStopLive");
#if !UNITY_EDITOR && UNITY_WSA
        HoloviewCallKit.instance().StopLive();
#endif
    }
    public void OnTestSendMessage(){
		Debug.Log ("OnTestSendMessage");

		//string target = TargetText.text;
		long targetId = TargetUserId;

		HoloviewCallKit.instance().SendTextMessage(ConversationType.P2P, targetId, "send from shownow for unity demo.");

        //		IosPlugin.instance().SendImageMessage(ConversationType.P2P, targetId, 
        //			"/storage/emulated/0/Android/data/cn.hv.im/cache/1516957289100.jpg", 
        //			"/storage/emulated/0/Android/data/cn.hv.im/cache/1516957289100_thumb.jpg");
        //		IosPlugin.instance().SendVideoMessage(ConversationType.P2P, targetId, 
        //			"/storage/emulated/0/DCIM/ShowNow/1524120996482/1524120996482.mp4", 
        //			"/storage/emulated/0/DCIM/ShowNow/1524120996482/1524120996482.jpg ", 2);
    }
    public void OnTestExit(){
		Debug.Log ("OnTestExit");
		Application.Quit ();
	}
}
