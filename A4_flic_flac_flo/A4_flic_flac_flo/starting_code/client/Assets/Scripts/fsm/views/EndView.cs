using UnityEngine.UI;
using TMPro;
using UnityEngine;

/**
 * Wraps all elements and functionality required for the LoginView.
 */
public class EndView : View
{       
    [SerializeField] private Button buttonLobby = null;
    public Button ButtonLobby { get => buttonLobby; }

    [SerializeField] private TMP_Text _text = null;
    public TMP_Text text => _text;

}
