using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

public class SpinWheel : MonoBehaviour
{
    #region Variables & Properties
    private const float rewardCollectAnimDuration = .25f;
    private const float spinMinDuration = 4f;
    private const float speed = 300;
    private readonly float poolAnimDist = Screen.height * .1f;

    [Header("Set Zone index <Starts with 0 :)>\nto review target zone configuration on edit mode")]
    [SerializeField]
    private int zone = 0;
    [Space(20), Header("Images"), SerializeField]
    private Image spinImage;
    [SerializeField]
    private Image spinIndicatorImage;
    [SerializeField]
    private Image spinButtonImage;
    [SerializeField]
    private Image frameImage;
    [Header("Text"), SerializeField]
    private TMP_Text headerText;
    [SerializeField]
    private TMP_Text noteText;
    [Header("Scripts"), SerializeField]
    private ZoneLine zoneLine;
    [SerializeField]
    private RewardPanel rewardPanel;
    [SerializeField]
    private SpinButton spinButton;
    [Header("Buttons"), SerializeField]
    private Button reviveButton_gold;
    [SerializeField]
    private Button reviveButton_ad;
    [SerializeField]
    private Button giveUpButton;
    [SerializeField]
    private Button exitButton;
    [SerializeField]
    private Button fakeAdRewardedButton;
    [SerializeField]
    private Button fakeAdCancelledButton;
    [SerializeField]
    private Button playAgainButton;
    [Header("GameObjects"), SerializeField]
    private GameObject failPanel;
    [SerializeField]
    private GameObject fakeAdPanel;
    [SerializeField]
    private GameObject pool, poolItemPrefab;

    private enum GameState { Alive, Dead, Reveived, GaveUp }
    private GameState gameState;
    private SpinTypes spinType;
    TextureStack textureStack;
    SpinConfiguration spinConfiguration;
    Reward[] rewards;
    List<Image> objectPool = new();
    #endregion

    #region MonoBehaviour
    private void OnValidate()
    {
        zone = Mathf.Clamp(zone, 0, 29);
        SelectSpinType();
        UpdateSpinWheel(false);
    }
    private void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;//keep screen alive
        zone = 0;
        //Add button listeners
        spinButton.AddOnClickListener(OnSpinButtonClicked);
        reviveButton_gold.onClick.AddListener(OnReveivedByGold);
        reviveButton_ad.onClick.AddListener(OnReveivedByAd);
        giveUpButton.onClick.AddListener(OnGavedUp);
        exitButton.onClick.AddListener(OnExit);
        fakeAdRewardedButton.onClick.AddListener(() => OnFakeAdClosed(true));
        fakeAdCancelledButton.onClick.AddListener(() => OnFakeAdClosed(false));
        playAgainButton.onClick.AddListener(() => SceneManager.LoadScene(0));//we assume that player paid the price successfully, simply reloaded scene for this demo

        //Set defaults
        spinButton.SetActive(false, false);
        gameState = GameState.Alive;
        //Start first zone
        StartCoroutine(StartNewZone());
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Load needed resources if not loaded
    /// </summary>
    private bool CheckResoures()
    {
        if (!textureStack)
        {
            try
            {
                textureStack = Resources.Load<TextureStack>("TextureStack");
            }
            catch
            {
                Debug.LogError("Texture Stack Couldn't Load! Please Check if it is exist in the Resources Folder!");
                return false;
            }
        }
        if (!spinConfiguration)
        {
            try
            {
                spinConfiguration = Resources.Load<SpinConfiguration>("SpinConfiguration");
            }
            catch
            {
                Debug.LogError("Spin Configuration Couldn't Load! Please Check if it is exist in the Resources folder!");
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// Set SpinType depended on zone 
    /// every 5th zone will silver, 
    /// every 30th zone will be golden
    /// all the rest will be bronze spin 
    /// </summary>
    private void SelectSpinType()
    {
        if (zone == 29)
            spinType = SpinTypes.Golden;
        else if (zone % 5 == 4 || zone == 0)
            spinType = SpinTypes.Silver;
        else
            spinType = SpinTypes.Bronze;
    }
    /// <summary>
    /// set spin textures and texts depended on spin type
    /// </summary>
    /// <param name="animated">if true reward images will be shown with animation. set false if you call from onvalidate(dotween will not work on edit mode)</param>
    private void UpdateSpinWheel(bool animated = true)
    {
        if (!CheckResoures())
            return;
        spinImage.sprite = textureStack.GetSpinWheel(spinType);
        spinIndicatorImage.sprite = textureStack.GetSpinIndicator(spinType);
        spinButtonImage.sprite = textureStack.GetSpinButton(spinType);
        spinButtonImage.enabled = spinButtonImage.sprite;   //disable if there is no sprite assigned
        frameImage.sprite = textureStack.GetFrame(spinType);
        frameImage.enabled = frameImage.sprite;     //disable if there is no sprite assigned
        zoneLine.UpdateZoneLine(zone, animated);

        WheelConfig wheelConfig = spinConfiguration.GetConfig(spinType);
        headerText.text = wheelConfig.Header;
        noteText.text = wheelConfig.Note;
        headerText.color = noteText.color = wheelConfig.TextColor;

        rewards = GetComponentsInChildren<Reward>();
        int index = GetRewardsIndex();
        RewardConfig[] rewardConfig = wheelConfig.GetZoneRewards(index);
        for (int i = 0; i < rewards.Length; i++)
            rewards[i].UpdateReward(textureStack.GetRewardSprite(rewardConfig[i].RewardType), rewardConfig[i], i, animated);
    }
    /// <summary>
    /// gets zone depended reward stack index
    /// this works depending on reward design structure at spinConfiguration 
    /// </summary>
    /// <returns></returns>
    private int GetRewardsIndex()
    {
        if (spinType == SpinTypes.Golden)
            return 0;
        if (spinType == SpinTypes.Silver)
        {
            if (zone == 0)
                return 0;// exception for first zone of the silver zone rule (zone % 5 == 4)
            return (zone + 1) / 5;
        }
        //we will receive zones for bronze as : 1,2,3,5,6,7,8,10,11,12,13,15...28 (0,4,9... are silver and 29 is golden )
        if (zone < 4)
            return zone - 1;
        if (zone < 9)
            return zone - 2;
        if (zone < 14)
            return zone - 3;
        if (zone < 19)
            return zone - 4;
        if (zone < 24)
            return zone - 5;
        return zone - 6;
    }
    private IEnumerator StartNewZone()
    {
        SelectSpinType();
        UpdateSpinWheel();
        gameState = GameState.Alive;
        yield return new WaitForSeconds(1);//wait for spin wheel update animations
        exitButton.interactable = spinType != SpinTypes.Bronze;//Player can choose to leave when wheel is not spinning and when the zone is safe or super zone
        spinButton.SetActive(true);
        yield return new WaitWhile(() => spinButton.IsActive);//it will be false on click
        //select a reward in advance
        int selectedReward = Random.Range(0, 8);//this can be a random or pre-selected reward depending on the game economy
        exitButton.interactable = false;
        yield return StartCoroutine(PlayRotationAnim(selectedReward));//wait for spin animation complete
        yield return new WaitForSeconds(.25f);
        yield return StartCoroutine(rewards[selectedReward].RunRewardedAnimation());

        RewardConfig result = rewards[selectedReward].Config;
        if (result.RewardType == RewardTypes.DeadZone)
        {
            //fail
            gameState = GameState.Dead;
            failPanel.SetActive(true);
        }
        else
        {
            int itemCount = (int)result.Amount;
            itemCount = Mathf.Min(itemCount, 5);//max 5 item
            for (int i = 0; i < itemCount; i++)
            {
                Image item = PickItemFromPool(result.RewardType);
                item.transform.localPosition = Vector3.zero;
                item.transform.DOLocalMove(new Vector3(Random.Range(-poolAnimDist, poolAnimDist), Random.Range(-poolAnimDist, poolAnimDist), 0), rewardCollectAnimDuration)
                    .OnComplete(() =>
                    {
                        item.transform.DOMove(rewardPanel.GetStackTarget(result.RewardType), 2 * rewardCollectAnimDuration).SetDelay(i * .1f)
                        .OnComplete(() => PutItemIntoPool(item));
                        item.transform.DOScale(.1f, rewardCollectAnimDuration).SetDelay(rewardCollectAnimDuration + i * .1f);
                    });
                //send to target tween in total rewardCollectAnimDuration
            }
            float totalDuration = 3 * rewardCollectAnimDuration ;
            rewardPanel.AddReward(result.RewardType, (int)result.Amount, totalDuration);
            totalDuration += itemCount * .1f;
            yield return new WaitForSeconds(totalDuration);//wait for animations
        }
        yield return new WaitWhile(() => gameState == GameState.Dead);//waiting for player input action
        if (gameState == GameState.GaveUp)
        {
            OnExit();
        }
        else
        {
            if (gameState == GameState.Alive)
                zone++;
            if (zone == 30)
                OnExit();//game over
            else
                StartCoroutine(StartNewZone());
        }
    }
    /// <summary>
    /// This is only animation to simulate fortune wheel
    /// it will be stop on the pre-selected reward slice
    /// </summary>
    /// <param name="selectedReward">it will stop on selected reward</param>
    /// <returns></returns>
    private IEnumerator PlayRotationAnim(int selectedReward)
    {
        float timer = 0;
        do
        {
            transform.Rotate(Vector3.forward, speed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        } while (timer < spinMinDuration);

        float targetSectorAngle = selectedReward * 45; //transform.TransformDirection(rewards[selectedReward].transform.up);
        float spinRegularSpeed = speed;
        do
        {
            transform.Rotate(Vector3.forward, spinRegularSpeed * Time.fixedDeltaTime);

            if (Mathf.Abs(transform.localEulerAngles.z - targetSectorAngle) > 60)
                spinRegularSpeed = Mathf.Lerp(spinRegularSpeed, .5f * speed, .01f);
            else
                spinRegularSpeed = Mathf.Lerp(spinRegularSpeed, .3f * speed, .01f);

            yield return new WaitForFixedUpdate();
        } while (Mathf.Abs(transform.localEulerAngles.z - targetSectorAngle) > 5);

        transform.localEulerAngles = targetSectorAngle * Vector3.forward;
    }
    #endregion

    #region Button Click Events
    private void OnSpinButtonClicked()
    {
        spinButton.SetActive(false);
    }
    private void OnReveivedByAd()
    {
        fakeAdPanel.SetActive(true);
    }
    private void OnReveivedByGold()
    {
        //if there is no enough gold open store options to purchase
        //we assume that there is enough gold to revive
        gameState = GameState.Reveived;
        failPanel.SetActive(false);
    }
    private void OnGavedUp()
    {
        gameState = GameState.GaveUp;
        failPanel.SetActive(false);
    }
    private void OnFakeAdClosed(bool adRewardReceived)
    {
        fakeAdPanel.SetActive(false);
        failPanel.SetActive(false);
        gameState = adRewardReceived ? GameState.Reveived : GameState.GaveUp;
    }
    private void OnExit()
    {
        rewardPanel.SwitchEndGameState(gameState == GameState.GaveUp);
    }
    #endregion

    #region ObjectPool
    private Image PickItemFromPool(RewardTypes type)
    {
        Image item;
        if (objectPool.Count == 0)
        {
            item = Instantiate(poolItemPrefab, pool.transform).GetComponent<Image>();
        }
        else
        {
            item = objectPool[0];
            objectPool.RemoveAt(0);
        }

        item.sprite = textureStack.GetRewardSprite(type);
        item.gameObject.SetActive(true);
        return item;
    }
    private void PutItemIntoPool(Image item)
    {
        item.gameObject.SetActive(false);
        item.transform.DOKill();
        item.transform.localScale = Vector3.one;
        objectPool.Add(item);
    }
    #endregion
}
