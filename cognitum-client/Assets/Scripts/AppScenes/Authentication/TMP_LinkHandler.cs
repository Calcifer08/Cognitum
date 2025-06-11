using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TMP_LinkHandler : MonoBehaviour, IPointerClickHandler
{
  [SerializeField] private AuthUI _authUI;

  private TextMeshProUGUI _switchAuthText;

  private void Awake()
  {
    _switchAuthText = GetComponent<TextMeshProUGUI>();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    int linkIndex = TMP_TextUtilities.FindIntersectingLink(_switchAuthText, eventData.position, null);

    if (linkIndex != -1)
    {
      TMP_LinkInfo linkInfo = _switchAuthText.textInfo.linkInfo[linkIndex];
      string linkID = linkInfo.GetLinkID();

      if (linkID == "login")
      {
        _authUI.SwitchToLogin();
      }
      else if (linkID == "register")
      {
        _authUI.SwitchToRegis();
      }
      else if (linkID == "reset")
      {
        _authUI.SwitchToResetPassword();
      }
    }
  }
}
