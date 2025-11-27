using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public GameMeta meta;
    public List<GameNode> nodes;
    // Initial stats configured in game.json, e.g. { "favor": 0, "money": 100 }
    public Dictionary<string, int> stats;
    // Optional: per-stat flag indicating whether to show toast when this stat changes
    // Example JSON: "statShowToast": { "favor": true, "money": false }
    public Dictionary<string, bool> statShowToast;
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
    public string title;
    public List<Choice> choices;
    public QTEData qte; // Optional single QTE data
    public List<QTEData> qteGroup; // Optional grouped QTEs played during video
    public Dictionary<int, string> qteNextNodeMap; // Group-level total score -> next node
    public bool isEnd;
    public string deathdesc; // Death description shown on death screen
    public string next;
}

[System.Serializable]
public class Choice
{
    public string label;
    public string next;
    // Optional stat effects applied when this choice is selected, e.g. { "favor": +5 }
    public Dictionary<string, int> statEffects;
}

[System.Serializable]
public class QTEData
{
    public string type; // "button", "sequence", "timing"
    public float duration;
    public Dictionary<int, string> NextNodeMap;
    public float startDelayFromStartSeconds;
    // Normalized UI position for QTE content (x,y in [0..1])
    public string position;
    public string param1;
    public string param2;
    public string param3;
    // Optional: per-score stat effects, mapping a score (or threshold) to stat changes
    // Example: { 1: { "favor": +3 }, 0: { "favor": -2 } }
    public Dictionary<int, Dictionary<string, int>> scoreEffects;

}
