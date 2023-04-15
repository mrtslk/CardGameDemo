using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SpinButton : MonoBehaviour
{
    #region Variables & Properties
    Button button;
    public bool IsActive => button.interactable;
    #endregion

    #region Private Methods
    private void Awake()
    {
        button = GetComponent<Button>();
    }
    #endregion

    #region Public Methods
    public void AddOnClickListener(UnityAction action)
    {
        button.onClick.AddListener(action);
    }
    public void SetActive(bool isActive,bool animated=true)
    {
        button.interactable = isActive;
        if (animated)
            transform.DOScale(isActive ? 1.0f : 0.0f, isActive ? .25f : 0.1f );
        else
            transform.localScale = isActive ? Vector3.one : Vector3.zero;
    }
    #endregion
}
