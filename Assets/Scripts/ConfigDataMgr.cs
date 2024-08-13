using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ConfigDatas
{
    public string ProductName;
    public string BookName;
    public string UnitName;
    public string LessonName;
    public ActivityData Activity;
    public JObject Owner;
    public string Key;
    public string CreatedStamp;
    public string LastUpdatedStamp;
    public int State;
}

[Serializable]
public class ActivityData
{
    public string Title;
    public string Key;
    public JObject Tags;
    public string Type;
    public JObject activityQuestionMd5;
    public JObject part;
    public JObject ContentId;
    public JObject ContentRevision;
    public List<StimulusData> Stimulus;
    public List<QuestionsData> Questions;
    public BodyData Body;
}


[Serializable]
public class StimulusData
{
    public string Key;
    public StimulusBodyData Body;
    public JObject Tags;
    public JObject Type;
    public JObject stimulusOfQuestion;
    public float questionAnchor;
    public float answerAnchor;
    public bool isForModeling;

}

[Serializable]
public class StimulusBodyData
{
    public BodyItemData item;
    public JObject version;
    public JObject tags;
    public JObject skillSet;
    public JObject tests;
    public JObject options;
    public JObject layoutMode;
    public JObject answers;
    public JObject mode;
    public JObject background;
    public JObject asrEngine;
    public float hintTime;
}

[Serializable]
public class BodyItemData
{
    public JObject type;
    public JObject id;
    public JObject text;
    public JObject prompt;
    public JObject hideText;
    public JObject image;
    public AudioData audio;
    public JObject pdf;
    public JObject video;
    public JObject audioLocal;
    public JObject academic;
    public JObject showMode;
    public JObject subtitles;
    public int rowIndex;
    public int colIndex;
    public bool lockedPosition;
    public JObject expected;
    public JObject speaker;
    public JObject table;
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;
    public JObject cells;
}

[Serializable]
public class AudioData
{
    public string id;
    public string url;
    public float size;
    public string sha1;
    public string mimeType;
    public float width;
    public float height;
    public JObject language;
    public JObject title;
    public float duration;
    public JObject thumbnails;
}

[Serializable]
public class QuestionsData
{
    public string Key;
    public QuestionsBodyData Body;
    public JObject Tags;
    public JObject Type;
    public StimulusOfQuestionData stimulusOfQuestion;
    public float questionAnchor;
    public float answerAnchor;
    public bool isForModeling;
}

[Serializable]
public class QuestionsBodyData
{
    public JObject item;
    public string version;
    public JArray tags;
    public JObject skillSet;
    public JArray tests;
    public List<QuestionsBodyOptionsData> options;
    public string layoutMode;
    public List<List<string>> answers;
    public JObject mode;
    public JObject background;
    public JObject asrEngine;
    public float hintTime;
}


[Serializable]
public class QuestionsBodyOptionsData
{
    public string type;
    public string id;
    public string text;
    public JObject prompt;
    public JObject hideText;
    public ImageData image;
    public JObject audio;
    public JObject pdf;
    public JObject video;
    public JObject audioLocal;
    public JObject academic;
    public JObject showMode;
    public JObject subtitles;
    public int rowIndex;
    public int colIndex;
    public bool lockedPosition;
    public JObject expected;
    public JObject speaker;
    public JObject table;
    public int startRow;
    public int endRow;
    public int startCol;
    public int endCol;
    public JObject cells;
}

[Serializable]
public class ImageData
{
    public string id;
    public string url;
    public float  size;
    public string sha1;
    public string mimeType;
    public float width;
    public float height;
    public JObject language;
    public JObject title;
    public float duration;
    public JObject thumbnails;
}


[Serializable]
public class StimulusOfQuestionData
{
    public string Key;
    public StimulusBodyData Body;
    public JObject Tags;
    public JObject Type;
    public JObject stimulusOfQuestion;
    public float questionAnchor;
    public float answerAnchor;
    public bool isForModeling;
}

[Serializable]
public class BodyData
{
    public JArray  mappings;
    public JObject tags;
}

public class ConfigDataMgr : MonoBehaviour
{
    public TextAsset Data;
    public static ConfigDataMgr Instance;

    public List<ConfigDatas> ConfigDatas = new List<ConfigDatas>();

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }
    private void OnDestroy()
    {
        Instance = null;
    }
    // Start is called before the first frame update
    void Start()
    {
        Load();
    }


    void Load()
    {
        ConfigDatas = JsonConvert.DeserializeObject<List<ConfigDatas>>(Data.text);

        if (ConfigDatas == null)
        {
            Debug.LogError("Config data is null!! Name is Data");
            return;
        }

    }

    public ConfigDatas  GetDatas(int index)
    {
        if (index >= ConfigDatas.Count)
        {
            Debug.LogError("index >= ConfigDatas.Count");
            return null;
        }
        return ConfigDatas[index];
    }
}
