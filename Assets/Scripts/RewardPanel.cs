using TMPro;
using System;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using DG.Tweening;

public class RewardPanel : MonoBehaviour
{
    #region Class Definitions
    [Serializable]
    internal class RewardObject
    {
        public GameObject self;
        public TMP_Text countText;
        int count;
        public int Count
        {
            get => count;
            private set
            {
                count = value;
                countText.text = count.ToShortString();
            }
        }
        public void Add (int add,bool animated,float delay=0)
        {
            int target = Count + add;
            if (!self.activeSelf && target > 0)
                self.SetActive(true);

            if(animated)
                DOTween.To(() => Count, x => Count = x, target, .5f).SetDelay(delay);
            else
                Count += add;
        }
        public void Reset()
        {
            self.SetActive(false);
            Count = 0;
        }
    }
    [Serializable]
    internal class RewardList : SerializableDictionaryBase<RewardTypes, RewardObject> { }
    #endregion

    #region Variables & Properties
    [SerializeField]
    private RewardList rewardList;
    Animator animator;
    private readonly int EndGameBool = Animator.StringToHash("EndGame");
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        foreach (RewardObject rewardObject in rewardList.Values)
            rewardObject.Reset();
        animator = GetComponent<Animator>();
        animator.SetBool(EndGameBool,false);
    }
    #endregion

    #region Public Methods
    public void AddReward(RewardTypes rewardType,int count,float delay)
    {
        rewardList[rewardType].Add(count,true,delay);
    }
    public void SwitchEndGameState(bool lostRewards=false)
    {
        foreach (RewardObject rewardObject in rewardList.Values)
            rewardObject.countText.color = lostRewards? Color.red:Color.green;
        animator.SetBool(EndGameBool, true);
    }
    public Vector3 GetStackTarget(RewardTypes rewardType)
    {
        return rewardList[rewardType].self.transform.position;
    }
    #endregion
}
