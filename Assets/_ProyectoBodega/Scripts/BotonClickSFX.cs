using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Emite feedback sonoro táctil al hacer clic sobre botones UI interactuables.
/// </summary>
[DisallowMultipleComponent]
public class BotonClickSFX : MonoBehaviour, IPointerClickHandler
{
    private Button boton;

    void Awake()
    {
        boton = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Application.isPlaying) return;
        if (boton == null) boton = GetComponent<Button>();
        if (boton != null && !boton.IsInteractable()) return;

        GestorAudioSCADA.Instance?.ReproducirClickUI();
    }
}
