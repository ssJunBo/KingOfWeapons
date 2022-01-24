using bFrame.Game.ResourceFrame;
using DG.Tweening;
using MVC.Controller;
using UnityEngine.UI;

public class TipsItem : BaseItem
{
    public Text content;
    private void OnEnable()
    {
        Tools.TimeCallback(this, 1f, () =>
        {
            ObjectManager.Instance.ReleaseObject(gameObject);
        });
        transform.DOLocalMoveY(transform.localPosition.y + 100, 0.5f);
    }
}
