/*
 * Copyright (c) 2020, NTT DOCOMO, INC.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *  Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 *  Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 *  Neither the name of the NTT DOCOMO, INC. nor the names of its contributors
 *   may be used to endorse or promote products derived from this software
 *   without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL NTT DOCOMO, INC. BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using UnityEngine;
using NTTDocomo.Speak;
using System;
using System.Collections.Generic;
using System.Threading;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SpeakSDKManager : MonoBehaviour
{
    // 自動停止
    private static readonly float TIMEOUT = 10.000f;//10000/ms
    private int mDialogCounter = 0;
    private SynchronizationContext mContext;

    void Start()
    {
        Speak.Instance().SetURL("wss://hostname.domain:443/path");
        Speak.Instance().SetDeviceToken("PUT_YOUR_DEVICE_TOKEN");

        // Callback.
        Speak.Instance().SetOnPlayStart(OnPlayStart);
        Speak.Instance().SetOnPlayEnd(OnPlayEnd);
        Speak.Instance().SetOnTextOut(OnTextOut);
        Speak.Instance().SetOnMetaOut(OnMetaOut);

        // Context.
        mContext = SynchronizationContext.Current;

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

    // ---------------------------------------------------------------------------- //
    //
    //  ボタンイベントで使用する関数
    //
    // ---------------------------------------------------------------------------- //

    // ---------------------------------------------------------------------------- //
    //  Start SDKを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void StartSpeakSDK()
    {
        Debug.Log("StartSpeakSDK");
        Speak.Instance().Start(OnStart, OnFailed);
    }

    // ---------------------------------------------------------------------------- //
    //  Stop SDKボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void StopSpeakSDK()
    {
        Debug.Log("StopSpeakSDK");
        Speak.Instance().Stop(OnStop);
    }

    // ---------------------------------------------------------------------------- //
    //  MicMuteOnStart ボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void MicMuteOnStart()
    {
        Debug.Log("MicMuteOnStart");
        Speak.Instance().SetMicMute(true);
    }

    // ---------------------------------------------------------------------------- //
    //  MicUnmuteOnStartボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void MicUnmuteOnStart()
    {
        Debug.Log("MicUnmuteOnStart");
        Speak.Instance().SetMicMute(false);
    }

    // ---------------------------------------------------------------------------- //
    //  PutMetaボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void PutMeta()
    {
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

        CancelInvoke("AutoStopTask");
        Interlocked.Increment(ref mDialogCounter);
    }

    // ---------------------------------------------------------------------------- //
    //  Muteボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void Mute()
    {
        Debug.Log("Mute");
        Speak.Instance().Mute();
    }

    // ---------------------------------------------------------------------------- //
    //  Unmuteボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void Unmute()
    {
        Debug.Log("Unmute");
        Speak.Instance().Unmute();
    }

    // ---------------------------------------------------------------------------- //
    //  CancelPlayボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void CancelPlay()
    {
        Debug.Log("CancelPlay");
        Speak.Instance().CancelPlay();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        Speak.Instance().Stop(OnStop);
        while (Speak.Instance().Poll(true)) { }
    }

    void OnApplicationQuit()
    {
        Speak.Instance().Stop(OnStop);
        while (Speak.Instance().Poll(true)) { }
    }

    // ---------------------------------------------------------------------------- //
    //
    //  Speak で使用するコールバック関数
    //
    // ---------------------------------------------------------------------------- //
    public void OnStart()
    {
        
        Debug.Log("------------SDK Start");
        Invoke("AutoStopTask", TIMEOUT);
    }


    public void OnFailed(int ecode, string failstr)
    {
       Debug.Log(string.Format("error ({0}): {1}", ecode, failstr));
    }

    public void OnStop()
    {
       Debug.Log("------------SDK Stop");
        CancelInvoke("AutoStopTask");
    }

    // ---------------------------------------------------------------------------- //
    //
    //  Speak に用意されているコールバック関数の受信側
    //
    // ---------------------------------------------------------------------------- //

    // ---------------------------------------------------------------------------- //
    //  メタ情報を受信した時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnMetaOut(OnMetaOut);
    //  引数(string) : JSON形式のメタ情報
    // ---------------------------------------------------------------------------- //
    public void OnMetaOut(string text)
    {
        var metaData = OnMetaOutJson.CreateFromJSON(text);
        if (!String.IsNullOrEmpty(metaData.systemText.utterance))
        {
            // システム発話文字列をログに出力
            Debug.Log("system text :" + metaData.systemText.utterance);
        }
        else
        {
            if (metaData.type == "nlu_result" &&
                Interlocked.Decrement(ref mDialogCounter) == 0)
            {
                // 対話の終了
                Invoke("AutoStopTask", TIMEOUT);
            }
        }

        if (metaData.type == "speech_recognition_result")
        {
            // 対話の開始
            CancelInvoke("AutoStopTask");
            Interlocked.Increment(ref mDialogCounter);
        }
    }

    // ---------------------------------------------------------------------------- //
    //  合成音声再生開始時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnPlayStart(OnPlayStart);
    //  引数(string) : 空文字
    // ---------------------------------------------------------------------------- //
    public void OnPlayStart(string text)
    {
        Debug.Log("-------OnPlayStart:");
    }

    // ---------------------------------------------------------------------------- //
    //  合成音声再生終了時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnPlayEnd(OnPlayEnd);
    //  引数(string) : 空文字
    // ---------------------------------------------------------------------------- //
    public void OnPlayEnd(string text)
    {
        Debug.Log("-------OnPlayEnd:");
        if (Interlocked.Decrement(ref mDialogCounter) == 0)
        {
            Invoke("AutoStopTask", TIMEOUT);
        }
    }

    // ---------------------------------------------------------------------------- //
    //  対話テキストを受信した時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnTextOut(OnTextOut);
    //  引数(string) : JSON形式の対話テキスト情報
    // ---------------------------------------------------------------------------- //
    public void OnTextOut(string text)
    {
        var speechMetaData = OnTextOutJson.CreateFromJSON(text);
        string voiceText = "";
        voiceText = MetaFindVoiceText(speechMetaData);
        if (!String.IsNullOrEmpty(voiceText))
        {
            Debug.Log("voice text :" + voiceText);
        }
    }

    // ---------------------------------------------------------------------------- //
    // JsonデータからTextを取得
    // ---------------------------------------------------------------------------- //
    private string MetaFindVoiceText(OnTextOutJson speechrec)
    {
        if (speechrec.sentences != null)
        {
            foreach (OnTextOutJson.Sentence sentence in speechrec.sentences)
            {
                if (!String.IsNullOrEmpty(sentence.converter_result))
                {
                    return sentence.converter_result;
                }
            }
        }
        return null;
    }

    // ---------------------------------------------------------------------------- //
    //
    // SDKを自動停止させるための関数
    //
    // ---------------------------------------------------------------------------- //
    private void AutoStopTask()
    {
        mContext.Post(__ =>
        {
            // メインスレッドで実行する
            Speak.Instance().Stop(OnStop);
        }, null);
    }
}
