using System;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

public class TextureStack : ScriptableObject
{
    #region Variables & Properties
    [SerializeField]
    private SpinWheelData spinWheelData;
    [SerializeField]
    private RewardData rewardData;
    #endregion

    #region Public Methods
    public Sprite GetSpinWheel(SpinTypes type)
    {
        return spinWheelData[type].spinWheel;
    }
    public Sprite GetSpinIndicator(SpinTypes type)
    {
        return spinWheelData[type].spinIndicator;
    }
    public Sprite GetSpinButton(SpinTypes type)
    {
        return spinWheelData[type].spinButtonBG;
    }
    public Sprite GetFrame(SpinTypes type)
    {
        return spinWheelData[type].frame;
    }
    public Sprite GetRewardSprite(RewardTypes type)
    {
        return rewardData[type];
    }
    #endregion
}

#region Required Classes Definitions to Organize Asset Hierarchy
[Serializable]
public class SpinWheelStack
{
    public Sprite spinWheel;
    public Sprite spinIndicator;
    public Sprite spinButtonBG;
    public Sprite frame;
}
[Serializable]
public class SpinWheelData : SerializableDictionaryBase<SpinTypes, SpinWheelStack> { }
[Serializable]
public class RewardData : SerializableDictionaryBase<RewardTypes, Sprite> { }
#endregion