using UnityEngine;
using NTTDocomo.Speak;
using System;
using System.Collections.Generic;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SpeakSDKManager : MonoBehaviour
{

    void Start()
    {
        #if (PLATFORM_ANDROID)
        // Androidのパーミッションが有効になっていない場合、パーミッションの許可を求める
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
        #endif
        Speak.Instance().SetURL("wss://hostname.domain:443/path");
        Speak.Instance().SetDeviceToken("PUT_YOUR_DEVICE_TOKEN");

        // Callback.
        Speak.Instance().SetOnPlayStart(OnPlayStart);
        Speak.Instance().SetOnPlayEnd(OnPlayEnd);
        Speak.Instance().SetOnTextOut(OnTextOut);
        Speak.Instance().SetOnMetaOut(OnMetaOut);

    }

    void Update()
    {
        try
        {
            Speak.Instance().Poll();
        }
        catch
        {
            Debug.Log("SDK ERROR");
        }
    }

    public void StartSpeakSDK()
    {
        Debug.Log("StartSpeakSDK");
        Speak.Instance().Start(OnStart, OnFailed);
    }

    public void StopSpeakSDK()
    {
        Debug.Log("StopSpeakSDK");
        Speak.Instance().Stop(OnStop);
    }

    public void MicMuteOnStart() {
        Debug.Log("MicMuteOnStart");
        Speak.Instance().SetMicMute(true);
    }

    public void MicUnmuteOnStart() {
        Debug.Log("MicUnmuteOnStart");
        Speak.Instance().SetMicMute(false);
    }

    public void PutMeta() {
        NluMetaData data = new NluMetaData();
        data.clientData = new ClientData();
        data.clientData.deviceInfo = new DeviceInfo();
        data.clientData.deviceInfo.playTTS = "on";
        data.clientVer = "0.5.1";
        data.language = "ja-JP";
        data.voiceText = "こんにちは";

        string json = JsonUtility.ToJson(data);
        Debug.Log("PutMeta:" + json);
        Speak.Instance().PutMeta(json);
    }

    public void Mute() {
        Debug.Log("Mute");
        Speak.Instance().Mute();
    }

    public void Unmute() {
        Debug.Log("Unmute");
        Speak.Instance().Unmute();
    }

    public void CancelPlay() {
        Debug.Log("CancelPlay");
        Speak.Instance().CancelPlay();
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy");
        Speak.Instance().Dispose();
    }


    public void OnStart()
    {
        
        Debug.Log("------------SDK Start");
        
    }


    public void OnFailed(int ecode, string failstr)
    {
       Debug.Log(string.Format("error ({0}): {1}", ecode, failstr));
    }

    public void OnStop()
    {
       Debug.Log("------------SDK Stop");
    }

    public void OnMetaOut(string text)
    {
        Debug.Log("-------OnMetaOut:");
        Debug.Log(text);
    }

    public void OnPlayStart(string text) {
        Debug.Log("-------OnPlayStart:");
        Debug.Log(text);
    }

    public void OnPlayEnd(string text) {
        Debug.Log("-------OnPlayEnd:");
        Debug.Log(text);
    }

    public void OnTextOut(string text) {
        Debug.Log("-------OnTextOut:");
        Debug.Log(text);
    }
}
