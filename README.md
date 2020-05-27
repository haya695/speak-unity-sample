# SpeakSDK for Unity サンプルアプリケーション

本ソースコードは株式会社NTTドコモが提供するドコモAIエージェントAPI [SpeakSDK](https://github.com/docomoDeveloperSupport/speak-unity-sdk)のサンプルコードです。

## アプリケーションの概要

AIエージェントAPI・SpeakSDKを利用することで実現可能な音声対話アプリケーションのサンプルコードです。
SpeakSDKを利用した音声対話処理やUIの実装例としてご利用ください。

## 利用方法

下記のチュートリアルをご参照ください。

[音声対話サンプルアプリ作成チュートリアル](https://aiagent-document.s3-ap-northeast-1.amazonaws.com/agentcraft_tutorial/Agentcraft_speak_sdk_for_unity_tutorial.pdf)

## 動作条件

1. 対象OS:Android, iOS, LuminOS, PC(macOS,Windows)
1. Speak SDK(1.14.0以上)
1. 対象Unityバージョン:2019.3.2f1

## VR機器向けアプリケーションのビルド

### Oculus Quest向け

Oculus向けUnityアプリをビルドする際は、Oculus Integrationをインポートする必要があります。
 
1. Unityから Window > Assets Store を選択し、AssetStoreを表示します。
2. AssetStore内を「Oculus Integration」で検索します。
3. 検索結果からOculus Integrationを選択して「Download」ボタンを押下します。
4. ダウンロード後、「Import」ボタンを押下しOculus Integrationをインポートします。

### MagicLeap向け

MagicLeap向けUnityアプリをビルドする際は、LuminSDKに含まれるMagicLeap.unitypackageのインポート、およびアプリケーション公開用証明書の設定を行う必要があります。
詳細な設定手順は下記ユーザーズガイドをご確認ください。

[Speak Unity SDK User's Guide](https://github.com/docomoDeveloperSupport/speak-unity-sdk/blob/master/speak_unity_sdk_users_guide.pdf)

## デバイストークンの取得

対話サービスを利用するにはデバイスIDの登録とデバイストークンの取得が必要です。 
Pythonスクリプトを利用してデバイストークンの取得を行ってください。

### トライアルサーバ向け手順

トライアルサーバ向けは GetTrialDeviceToken.py のスクリプトを使用します。
スクリプト中における `device_id` はダミー値であるため、[Agentcraft](http://agentcraft.sebastien.ai/)の設定タグから、ご自身で取得した値に書き換えて実行してください。
GetTrialDeviceToken.pyを実行すると以下の様にデバイスID登録用のURLを表示して登録の完了を待機します。

```
$ python3 GetTrialDeviceToken.py 
Device ID :XXXXXXXXXXXXXXXXXXXX
Please register above ID as your device on User Dashboard. https://users-v2.sebastien.ai
下記リンク（↓）を使ってブラウザ等でデバイスIDを自分のアカウントに登録して下さい。
https://users-v2.sebastien.ai/dashboard/device_registration?confirm=yes&device_id=XXXXXXXXXXXXXXXXXXXX

Press any key AFTER registration >>>
```

ブラウザでURLにアクセスしてデバイスID登録を完了させて下さい。  
登録にはGoogleアカウントまたはdアカウントによる認証が必要です。

登録が完了したらEnterを入力して下さい。
デバイストークンとリフレッシュトークンが標準出力に表示されます。

```
{
    "device_token": "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy", 
    "refresh_token": "oooooooo-oooo-oooo-oooo-oooooooooooo", 
    "status": "valid"
}
SAVE ./.trial_device_token : yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy
SAVE ./.trial_refresh_token : oooooooo-oooo-oooo-oooo-oooooooooooo
```

### 商用サーバ向け手順

商用サーバ向けは GetProductionDeviceToken.py のスクリプトを使用します。
スクリプト中における `client_secret` はダミー値であるため、ご自身で取得した値に書き換えて実行してください。

GetProductionDeviceToken.pyを実行すると以下の様にデバイスID登録用のURLを表示して登録の完了を待機します。

```
$ python3 GetProductionDeviceToken.py
Success to get DeviceID :xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
デバイスIDの取得に成功しました。
Please register DeviceID as your device on User Dashboard.
下記リンク（↓）を使ってブラウザ等でデバイスIDを自分のアカウントに登録して下さい。
https://doufr.aiplat.jp/device/regist?directAccess=true&deviceId=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

Press any key AFTER registration >>> 
```

ブラウザでURLにアクセスしてデバイスID登録を完了させて下さい。  
登録にはdアカウントによる認証が必要です。  

登録が完了したらEnterを入力して下さい。
デバイストークンとリフレッシュトークンが標準出力に表示されます。
```
Success to get DeviceToken : xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
デバイストークンの取得に成功しました。
Success to get RefreshToken : xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
リフレッシュトークンの取得に成功しました。
```

> リフレッシュトークンはデバイストークンの更新に使用します。  
スクリプトで取得したリフレッシュトークンを隠しファイルに保存しています。
デバイストークンの使用期限が切れた時は、スクリプトを再び実行して下さい。
保存したリフレッシュトークンでデバイストークンを更新します。

## 接続先の変更

サンプルコード中における接続先設定値はダミー値となっています。
SpeakSDKManager.cs の以下の行を、接続先に応じて変更してください。

```
    Speak.Instance().SetURL("wss://hostname.domain:443/path");
    Speak.Instance().SetDeviceToken("PUT_YOUR_DEVICE_TOKEN");
```

`wss://hostname.domain:443/path` を適切な接続先に置き換えます。
- トライアルサーバ : `wss://spf-v2.sebastien.ai/talk`
- 商用サーバ : `wss://dospf.aiplat.jp/ciel`

`PUT_YOUR_DEVICE_TOKEN` を取得したデバイストークンに置き換えます。

## License

本サンプルコードは以下の修正BSDライセンスが適用されます。

[LICENSE.txt](/LICENSE.txt)

また本サンプルアプリケーションの動作に必要となる[SpeakSDK](https://github.com/docomoDeveloperSupport/speak-unity-sdk)の利用にあたっては下記ソフトウェア開発キットの利用に関する規約が適用されます。規約をご確認のうえ利用をお願いいたします。

[ソフトウェア開発キットの利用に関する規約](https://github.com/docomoDeveloperSupport/speak-unity-sdk/blob/master/LICENSE.md)

## Acknowledgments

This product includes software developed by the OpenSSL Project for use in the OpenSSL Toolkit. (http://www.openssl.org/)  
This product includes cryptographic software written by Eric Young (eay@cryptsoft.com)

## Author

[NTT DOCOMO, INC.](https://docs.sebastien.ai/)
