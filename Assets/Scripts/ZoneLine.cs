using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZoneLine : MonoBehaviour
{
    private const int layoutFrameDist = -90;
    [SerializeField]
    private HorizontalLayoutGroup listLayout, frameLayout;
    [SerializeField]
    private RectTransform listRect, frameRect;

    public void UpdateZoneLine(int zone, bool animated = true)
    {
        if (animated)
        {
            DOTween.To(() => listLayout.padding.left, x => listLayout.padding.left = x, layoutFrameDist * zone, .5f)
                .OnUpdate(() =>
                {
                    frameLayout.padding.left = listLayout.padding.left;
                    LayoutRebuilder.MarkLayoutForRebuild(listRect);
                    LayoutRebuilder.MarkLayoutForRebuild(frameRect);
                });
        }
        else
        {
            frameLayout.padding.left = listLayout.padding.left = layoutFrameDist * zone;
            LayoutRebuilder.MarkLayoutForRebuild(listRect);
            LayoutRebuilder.MarkLayoutForRebuild(frameRect);
        }

    }
}
