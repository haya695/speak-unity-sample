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
using UnityEngine.UI;
using System.Threading;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class SpeakSDKManager : MonoBehaviour
{
    // ログ表示欄
    private ScrollRect mScrollRect;
    //　SDKStartボタン
    private Button mStartButton;
    //　SDKStopボタン
    private Button mStopButton;
    // 表示するログの保持用変数
    private Text mTextLog;
    private string mLogs = "";
    private string mOldLogs = "";
    // SDKの稼動状態フラグ　true:稼動中
    private bool mSpeakingFlag = false;

    // 選択状態の色
    private Color mOnColor;
    // 未選択状態の色
    private Color mOffColor;

    // 自動停止
    private static readonly float TIMEOUT = 10.000f;//10000/ms
    private int mDialogCounter  = 0;
    private SynchronizationContext mContext;

    // AudioSource
    private AudioSource mAudioSource;

    public void Start()
    {
#if (PLATFORM_ANDROID)
        // Androidのパーミッションが有効になっていない場合、パーミッションの許可を求める
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif

        // AudioSourceコンポーネントの取得
        mAudioSource = gameObject.GetComponent<AudioSource>();

        //Speakの初期化
        InitializeSpeakSDK();

        //UIの取得
        mScrollRect = GameObject.Find("ScrollView").GetComponent<ScrollRect>();
        mTextLog = mScrollRect.content.GetComponentInChildren<Text>();
        mStartButton = GameObject.Find("SDKStartButton").GetComponent<Button>();
        mStopButton = GameObject.Find("SDKStopButton").GetComponent<Button>();

        // ボタンの色を定義
        ColorUtility.TryParseHtmlString("#98FB98", out mOnColor);
        ColorUtility.TryParseHtmlString("#DCDCDC", out mOffColor);
        SetSpeakingFlag(false);

        //コンテキストの取得
        mContext = SynchronizationContext.Current;
    }

    public void Update()
    {
        try
        {
            Speak.Instance().Poll();
        }
        catch
        {
        }

        //  mLogsとmOldLogsが異なるときにTextを更新
        if (mScrollRect != null && mLogs != mOldLogs)
        {
            mTextLog.text = mLogs;
            // Textが追加されたとき自動でScrollViewのBottomに移動する
            mScrollRect.verticalNormalizedPosition = 0;
            mOldLogs = mLogs;
        }
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

    public void SetSpeakingFlag(bool speakingFlag)
    {
        if (speakingFlag)
        {
            mStartButton.image.color = mOnColor;
            mStopButton.image.color = mOffColor;
        }
        else
        {
            mStartButton.image.color = mOffColor;
            mStopButton.image.color = mOnColor;
        }
        mSpeakingFlag = speakingFlag;
    }

    // ---------------------------------------------------------------------------- //
    //  SDK初期化処理
    // ---------------------------------------------------------------------------- //
    private void InitializeSpeakSDK()
    {
        Speak.Instance().SetURL("wss://hostname.domain:443/path");
        Speak.Instance().SetDeviceToken("PUT_YOUR_DEVICE_TOKEN");

        // Callback.
        Speak.Instance().SetOnTextOut(OnTextOut);
        Speak.Instance().SetOnMetaOut(OnMetaOut);
        Speak.Instance().SetOnPlayEnd(OnPlayEnd);

        // AudioSource
        Speak.Instance().SetAudioSource(mAudioSource);

    }

    // ---------------------------------------------------------------------------- //
    //
    //  ボタンイベントで使用する関数
    //
    // ---------------------------------------------------------------------------- //

    // ---------------------------------------------------------------------------- //
    //  SDKstartを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void StartSpeakSDK()
    {
        // Speakの実行
        Speak.Instance().Start(OnStart, OnFailed);
    }

    // ---------------------------------------------------------------------------- //
    //  SDKstopボタンを押下した時に呼び出される
    // ---------------------------------------------------------------------------- //
    public void StopSpeakSDK()
    {
        Speak.Instance().Stop(OnStop);
    }


    // ---------------------------------------------------------------------------- //
    //
    //  Speak で使用するコールバック関数
    //
    // ---------------------------------------------------------------------------- //
    public void OnStart()
    {
        SetSpeakingFlag(true);
        Invoke("AutoStopTask", TIMEOUT);
    }


    public void OnStop()
    {
        SetSpeakingFlag(false);
        CancelInvoke("AutoStopTask");
    }

    public void OnFailed(int ecode, string failstr)
    {
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
    public void OnMetaOut(string metaText)
    {
        var metaData = OnMetaOutJson.CreateFromJSON(metaText);
        // 再生テキスト内容
        if (!String.IsNullOrEmpty(metaData.systemText.utterance))
        {
            // スクロールビューにテキストを表示する
            LogView(metaData.systemText.utterance);
        }
        // 再生テキスト取得失敗時の表示内容
        else if (!String.IsNullOrEmpty(metaData.systemText.expression))
        {
            // スクロールビューにテキストを表示する
            LogView(metaData.systemText.expression);
        }

        if (metaData.type == "speech_recognition_result")
        {
            // 対話の開始
            CancelInvoke("AutoStopTask");
            Interlocked.Increment(ref mDialogCounter);
        }
        else if (String.IsNullOrEmpty(metaData.systemText.utterance))
        {
            if (metaData.type == "nlu_result" &&
                Interlocked.Decrement(ref mDialogCounter) == 0)
            {
                // 対話の終了
                Invoke("AutoStopTask", TIMEOUT);
            }
        }
    }

    // ---------------------------------------------------------------------------- //
    //  対話テキストを受信した時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnTextOut(OnTextOut);
    //  引数(string) : JSON形式の対話テキスト情報
    // ---------------------------------------------------------------------------- //
    public void OnTextOut(string metaText)
    {
        // スクロールビューにテキストを表示する
        // 発話内容
        var speechMetaData = OnTextOutJson.CreateFromJSON(metaText);
        string viewText = "";
        viewText = MetaFindVoiceText(speechMetaData);
        if (!String.IsNullOrEmpty(viewText))
        {
            LogView(viewText);
        }
    }

    // ---------------------------------------------------------------------------- //
    //  合成音声再生終了時に呼ばれるメソッド
    //  
    //  登録例 : Speak.Instance().SetOnPlayEnd(OnPlayEnd);
    //  引数(string) : 空文字
    // ---------------------------------------------------------------------------- //
    public void OnPlayEnd(string text)
    {
        if (Interlocked.Decrement(ref mDialogCounter) == 0)
        {
            Invoke("AutoStopTask", TIMEOUT);
        }
    }

    // ---------------------------------------------------------------------------- //
    //
    //  画面上に表示するための関数
    //
    // ---------------------------------------------------------------------------- //

    // ---------------------------------------------------------------------------- //
    // ログを表示するメソッド 
    // ---------------------------------------------------------------------------- //
    private void LogView(string viewText)
    {
        if (!String.IsNullOrEmpty(viewText))
        {
            mLogs += viewText;
            mLogs += "\n";
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
        mContext?.Post(__ =>
        {
            Speak.Instance().Stop(OnStop);
        }, null);
    }
}
