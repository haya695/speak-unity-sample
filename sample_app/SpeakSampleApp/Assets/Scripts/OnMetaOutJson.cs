using System.Collections.Generic;
using UnityEngine;

//Json定義
[System.Serializable]
public class OnMetaOutJson
{
    public string speaker;
    public SystemText systemText;
    public string version;
    public string type;
    public Option option;

    [System.Serializable]
    public class SystemText
    {
        public string utterance;
        public string expression;
    }

    [System.Serializable]
    public class Option
    {
    }

    public static OnMetaOutJson CreateFromJSON(string json)
    {
        return JsonUtility.FromJson<OnMetaOutJson>(json);
    }
}
