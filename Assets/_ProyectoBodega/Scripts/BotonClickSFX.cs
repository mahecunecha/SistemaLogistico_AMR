using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Emite feedback sonoro táctil al hacer clic sobre botones UI interactuables o bloqueados.
/// </summary>
[DisallowMultipleComponent]
public class BotonClickSFX : MonoBehaviour, IPointerClickHandler
{
    public enum ModoAudioBoton
    {
        ClickEstandar,
        IngestaJSON
    }

    [SerializeField] private ModoAudioBoton modoAudio = ModoAudioBoton.ClickEstandar;
    private Button boton;

    public void ConfigurarModo(ModoAudioBoton modo)
    {
        modoAudio = modo;
    }

    void Awake()
    {
        boton = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Application.isPlaying) return;
        if (boton == null) boton = GetComponent<Button>();

        if (modoAudio == ModoAudioBoton.IngestaJSON)
        {
            if (boton != null && !boton.IsInteractable())
            {
                GestorAudioSCADA.Instance?.ReproducirBloqueoIngestaJSON();
            }
            else
            {
                GestorAudioSCADA.Instance?.ReproducirIngresoPermitidoJSON();
            }
            return;
        }

        if (boton != null && !boton.IsInteractable()) return;

        GestorAudioSCADA.Instance?.ReproducirClickUI();
    }
}
