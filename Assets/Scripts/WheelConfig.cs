using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class WheelConfig
{
    #region Variables & Properties
    [SerializeField]
    private string header;
    [SerializeField, Tooltip("Message displayed below the spinning wheel")]
    private string note;
    [SerializeField]
    private Color textColor;
    [SerializeField, Header("Zone Reward Design"), Tooltip("Zone List")]
    private List<ZoneRewards> zoneRewards = new();
    public string Header=>header;
    public string Note=>note;
    public Color TextColor => textColor;
    #endregion

    #region Public Methods
    public RewardConfig[] GetZoneRewards(int zone) => zoneRewards[zone % zoneRewards.Count].rewards;//zone % zoneRewards.Count => to handle index>=count
    #endregion
}

#region Required Classes Definitions to Organize Asset Hierarchy
[Serializable]
public class ZoneRewards
{
    [Header("Must have 8 elements to match with spin slices")]
    public RewardConfig[] rewards = new RewardConfig[8];
}
#endregion