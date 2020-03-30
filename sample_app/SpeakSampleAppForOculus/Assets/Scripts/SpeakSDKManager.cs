using UnityEngine;
using NTTDocomo.Speak;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
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

    public void Start()
    {
#if (PLATFORM_ANDROID)
        // Androidのパーミッションが有効になっていない場合、パーミッションの許可を求める
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif

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
        setSpeakingFlag(false);
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

    public void OnDestroy()
    {
        Speak.Instance().Dispose();
    }

    public void setSpeakingFlag(bool speakingFlag)
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
        setSpeakingFlag(true);
    }


    public void OnStop()
    {
        setSpeakingFlag(false);
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
    //  登録例 : Speak.setOnMetaOut += this.onMetaOut;
    //  引数(string) : JSON形式のメタ情報
    // ---------------------------------------------------------------------------- //
    public void OnMetaOut(string mateText)
    {
        var metaData = OnMetaOutJson.CreateFromJSON(mateText);
        // 再生テキスト内容
        if (metaData.systemText.utterance != null && metaData.systemText.utterance != "")
        {
            // スクロールビューにテキストを表示する
            LogView(metaData.systemText.utterance);
        }
        // 再生テキスト取得失敗時の表示内容
        else if (metaData.systemText.expression != null && metaData.systemText.expression != "")
        {
            // スクロールビューにテキストを表示する
            LogView(metaData.systemText.expression);
        }
    }

    // ---------------------------------------------------------------------------- //
    //  対話テキストを受信した時に呼ばれるメソッド
    //  
    //  登録例 : Speak.setOnTextOut += this.onTextOut;
    //  引数(string) : JSON形式の対話テキスト情報
    // ---------------------------------------------------------------------------- //
    public void OnTextOut(string mateText)
    {
        // スクロールビューにテキストを表示する
        // 発話内容
        var speecMetaData = OnTextOutJson.CreateFromJSON(mateText);
        string viewText = "";
        viewText = MetaFindVoiceText(speecMetaData);
        if (viewText != null && viewText != "")
        {
            LogView(viewText);
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
        if (viewText != null && viewText != "")
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
                if (sentence.converter_result != null && sentence.converter_result != "")
                {
                    return sentence.converter_result;
                }
            }
        }
        return null;
    }
}
