using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button solveBtn;
    public Button replayBtn;
    public TMP_InputField widthInputField;
    public TMP_InputField heightInputField;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.solveBtn.onClick.AddListener(this.SolveButtonClick);
        this.replayBtn.onClick.AddListener(this.ReplayButtonClick);
        this.widthInputField.onEndEdit.AddListener(this.OnWidthHeightModified);
        this.heightInputField.onEndEdit.AddListener(this.OnWidthHeightModified);
    }

    private void SolveButtonClick()
    {
        Observer.Instance.Notify(GameConstants.ObserverKey.OnSolveNotify);
    }

    private void ReplayButtonClick()
    {
        Observer.Instance.Notify(GameConstants.ObserverKey.OnReplayNotify);
    }

    private void OnWidthHeightModified(string val)
    {
        var width = int.TryParse(this.widthInputField.text, out var w) ? w : 40;
        var height = int.TryParse(this.heightInputField.text, out var h) ? h : 40;
        Observer.Instance.Notify(GameConstants.ObserverKey.OnWidthHeightModified, new Vector2Int(width, height));
    }
}
