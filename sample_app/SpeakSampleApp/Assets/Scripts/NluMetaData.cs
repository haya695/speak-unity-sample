using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NluMetaData
{
    public ClientData clientData;
    public string clientVer;
    public string language;
    public string voiceText;

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class DeviceInfo
{
    public string playTTS;
}

[System.Serializable]
public class ClientData
{
    public DeviceInfo deviceInfo;
}
