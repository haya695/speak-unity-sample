using System.Collections.Generic;
using UnityEngine;

//Json定義
[System.Serializable]
public class OnTextOutJson
{
    public string recognition_id;
    public int result_status;
    public List<Sentence> sentences;

    [System.Serializable]
    public class Word
    {
        public double start_time;
        public double score;
        public double end_time;
        public string label;
    }

    [System.Serializable]
    public class Sentence
    {
        public string converter_result;
        public double score;
        public List<Word> words;
    }

    public static OnTextOutJson CreateFromJSON(string json)
    {
        return JsonUtility.FromJson<OnTextOutJson>(json);
    }

}
