using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct RewardConfig
{
    [SerializeField]
    private RewardTypes rewardType;
    [SerializeField]
    private uint amount;
    public uint Amount=>amount;
    public RewardTypes RewardType => rewardType;
    public string AmountValue=>amount.ToShortString();
}
