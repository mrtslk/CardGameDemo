using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;

public class Reward : MonoBehaviour
{
    #region Variables & Properties
    [SerializeField]
    private Image rewardImage;
    [SerializeField]
    private TMP_Text rewardValue;
    [SerializeField]
    private Transform animRoot;
    private RewardConfig mConfig;
    public RewardConfig Config { get { return mConfig; } private set { mConfig = value; } }
    #endregion

    #region Public Methods
    public void UpdateReward( Sprite itemImage,RewardConfig config,int index,bool animated)
    {
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, -45f * index));

        Config = config;
        animRoot.localScale = Vector3.zero;
        rewardImage.sprite = itemImage;
        rewardValue.text = config.AmountValue;
        rewardValue.enabled = Config.RewardType != RewardTypes.DeadZone;
        if (animated)
            animRoot.DOScale(Vector3.one, .25f).SetEase(Ease.OutBounce).SetDelay(index * .125f);
        else
            animRoot.localScale = Vector3.one;
    }
    public IEnumerator RunRewardedAnimation()
    {
       transform.GetChild(0).DOScale(1.2f, .35f).SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                transform.GetChild(0).DOScale(1f, .15f).SetEase(Ease.InQuart).SetDelay(.25f);
            });
        yield return new WaitForSeconds(.75f);
    }
    #endregion
}
