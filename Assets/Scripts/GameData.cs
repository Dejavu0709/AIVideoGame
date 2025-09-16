using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public GameMeta meta;
    public List<GameNode> nodes;
}

[System.Serializable]
public class GameMeta
{
    public string title;
    public string cdnBase;
    public string startNodeId;
}

[System.Serializable]
public class GameNode
{
    public string id;
    public string video;
    public string question;
    public List<Choice> choices;
    public QTEData qte; // Optional QTE data
}

[System.Serializable]
public class Choice
{
    public string label;
    public string next;
}

[System.Serializable]
public class QTEData
{
    public string type; // "button", "sequence", "timing"
    public float duration;
    public List<string> sequence; // For sequence QTEs
    public string successNext;
    public string failNext;
}
