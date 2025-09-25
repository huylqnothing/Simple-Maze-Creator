using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button solveBtn;
    public Button replayBtn;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.solveBtn.onClick.AddListener(this.SolveButtonClick);
        this.replayBtn.onClick.AddListener(this.ReplayButtonClick);
    }

    private void SolveButtonClick()
    {
        Observer.Instance.Notify(GameConstants.ObserverKey.OnSolveNotify);
    }

    private void ReplayButtonClick()
    {
        Observer.Instance.Notify(GameConstants.ObserverKey.OnReplayNotify);
    }
}
