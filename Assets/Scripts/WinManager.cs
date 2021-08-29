using Mirror;
using TMPro;
using UnityEngine;

public class WinManager : NetworkBehaviour
{
    [SerializeField] TMP_Text _label;

    [ClientRpc]
    public void ShowLabel() => _label.text = "You Win!";

    [ClientRpc]
    public void HideLabel() => _label.text = "";
}
