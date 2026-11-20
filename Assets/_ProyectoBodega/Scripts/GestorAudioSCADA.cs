using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Motor singleton de audio procedural SCADA para alertas 2D, confirmaciones UX y síntesis de loops 3D.
/// </summary>
public class GestorAudioSCADA : MonoBehaviour
{
    public static GestorAudioSCADA Instance { get; private set; }

    [Header("Salida de Audio 2D")]
    public AudioSource fuenteAudio;

    [Header("Controles UI")]
    public Button btnMuteAudio;
    public TextMeshProUGUI txtBtnMuteAudio;
    public Slider sliderVolumen;
    public TextMeshProUGUI txtVolumenPct;

    [Header("Configuración Maestro")]
    [Range(0f, 1f)]
    public float volumenMaestro = 0.75f;
    public bool silenciado = false;

    [Header("Clips 2D Opcionales (Override)")]
    [SerializeField] private AudioClip clipAlertaPreventiva;
    [SerializeField] private AudioClip clipAlarmaCritica;
    [SerializeField] private AudioClip clipConfirmacionEntrega;
    [SerializeField] private AudioClip clipAcopleNeumaticoVerde;
    [SerializeField] private AudioClip clipClickUI;

    [Header("Clips 3D Loop Opcionales (Override)")]
    [SerializeField] private AudioClip clipMotorAMR;
    [SerializeField] private AudioClip clipRotoresEVE;
    [SerializeField] private AudioClip clipRotoresDronSoporte;

    public AudioClip ClipMotorAMR
    {
        get
        {
            if (clipMotorAMR == null)
            {
                clipMotorAMR = RegistrarClipProcedural(CrearClipLoopMotorAMR());
            }
            return clipMotorAMR;
        }
    }

    public AudioClip ClipRotoresEVE
    {
        get
        {
            if (clipRotoresEVE == null)
            {
                clipRotoresEVE = RegistrarClipProcedural(CrearClipLoopRotores("SCADA_RotoresEVE", 85f, 18f, 0.28f, 0.055f, 137));
            }
            return clipRotoresEVE;
        }
    }

    public AudioClip ClipRotoresDronSoporte
    {
        get
        {
            if (clipRotoresDronSoporte == null)
            {
                clipRotoresDronSoporte = RegistrarClipProcedural(CrearClipLoopRotores("SCADA_RotoresDronSoporte", 72f, 14f, 0.34f, 0.075f, 251));
            }
            return clipRotoresDronSoporte;
        }
    }

    private const string PrefVolumenMaestro = "SCADA_VolumenMaestro";
    private const string PrefSilenciado = "SCADA_Silenciado";
    private const int SampleRate = 44100;
    private const float CooldownAlertas = 0.12f;

    private static readonly Color ColorTextoSonidoOn = new Color(0.220f, 0.741f, 0.973f, 1f);
    private static readonly Color ColorTextoMute = new Color(0.973f, 0.443f, 0.443f, 1f);

    private readonly List<AudioClip> clipsGeneradosEnMemoria = new List<AudioClip>(8);
    private float ultimoDisparoPreventiva = -10f;
    private float ultimoDisparoCritica = -10f;
    private float ultimoDisparoEntrega = -10f;
    private float ultimoDisparoNeumatico = -10f;
    private float ultimoDisparoClick = -10f;
    private int ultimoFrameToggleMute = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (PlayerPrefs.HasKey(PrefVolumenMaestro))
        {
            volumenMaestro = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefVolumenMaestro, 0.75f));
        }
        else
        {
            volumenMaestro = Mathf.Clamp01(volumenMaestro);
        }

        if (PlayerPrefs.HasKey(PrefSilenciado))
        {
            silenciado = PlayerPrefs.GetInt(PrefSilenciado, 0) == 1;
        }

        if (fuenteAudio == null)
        {
            fuenteAudio = GetComponent<AudioSource>();
            if (fuenteAudio == null)
            {
                fuenteAudio = gameObject.AddComponent<AudioSource>();
            }
        }
        fuenteAudio.playOnAwake = false;
        fuenteAudio.loop = false;
        fuenteAudio.spatialBlend = 0f;
        fuenteAudio.volume = 1f;

        SintetizarClipsFaltantes();
        AplicarVolumenGlobal();
    }

    void Start()
    {
        VincularControlesUI();
        AplicarVolumenGlobal();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        for (int i = 0; i < clipsGeneradosEnMemoria.Count; i++)
        {
            if (clipsGeneradosEnMemoria[i] != null)
            {
                Destroy(clipsGeneradosEnMemoria[i]);
            }
        }
        clipsGeneradosEnMemoria.Clear();
    }

    private AudioClip RegistrarClipProcedural(AudioClip clip)
    {
        if (clip != null)
        {
            clipsGeneradosEnMemoria.Add(clip);
        }
        return clip;
    }

    private void VincularControlesUI()
    {
        Transform filaAudio = transform.Find("HUD_Operativo/Panel_Controles_SCADA/Fila_Control_Audio");
        if (filaAudio != null)
        {
            if (btnMuteAudio == null)
            {
                Transform tBtn = filaAudio.Find("Btn_Mute_Audio");
                if (tBtn != null) btnMuteAudio = tBtn.GetComponent<Button>();
            }
            if (txtBtnMuteAudio == null && btnMuteAudio != null)
            {
                txtBtnMuteAudio = btnMuteAudio.GetComponentInChildren<TextMeshProUGUI>(true);
            }
            if (sliderVolumen == null)
            {
                Transform tSlider = filaAudio.Find("Slider_Volumen_SCADA");
                if (tSlider != null) sliderVolumen = tSlider.GetComponent<Slider>();
            }
            if (txtVolumenPct == null)
            {
                Transform tPct = filaAudio.Find("Txt_Volumen_Pct");
                if (tPct != null) txtVolumenPct = tPct.GetComponent<TextMeshProUGUI>();
            }
        }

        if (btnMuteAudio != null)
        {
            btnMuteAudio.onClick.RemoveListener(ToggleSilencio);
            if (btnMuteAudio.onClick.GetPersistentEventCount() == 0)
            {
                btnMuteAudio.onClick.AddListener(ToggleSilencio);
            }
        }

        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.RemoveListener(SetVolumenMaestro);
            sliderVolumen.onValueChanged.AddListener(SetVolumenMaestro);
        }
    }

    public void SetVolumenMaestro(float valor)
    {
        volumenMaestro = Mathf.Clamp01(valor);
        PlayerPrefs.SetFloat(PrefVolumenMaestro, volumenMaestro);
        PlayerPrefs.Save();
        AplicarVolumenGlobal();
    }

    public void ToggleSilencio()
    {
        if (ultimoFrameToggleMute == Time.frameCount) return;
        ultimoFrameToggleMute = Time.frameCount;

        SetSilenciado(!silenciado);
    }

    public void SetSilenciado(bool mute)
    {
        silenciado = mute;
        PlayerPrefs.SetInt(PrefSilenciado, silenciado ? 1 : 0);
        PlayerPrefs.Save();
        AplicarVolumenGlobal();
    }

    public void AplicarVolumenGlobal()
    {
        volumenMaestro = Mathf.Clamp01(volumenMaestro);
        AudioListener.volume = silenciado ? 0f : volumenMaestro;
        RefrescarControlesUI();
    }

    public void RefrescarControlesUI()
    {
        if (sliderVolumen != null)
        {
            sliderVolumen.SetValueWithoutNotify(volumenMaestro);
        }

        if (txtVolumenPct != null)
        {
            txtVolumenPct.text = silenciado ? "0%" : $"{Mathf.RoundToInt(volumenMaestro * 100f)}%";
        }

        if (txtBtnMuteAudio != null)
        {
            txtBtnMuteAudio.text = silenciado ? "MUTE" : "SONIDO: ON";
            txtBtnMuteAudio.color = silenciado ? ColorTextoMute : ColorTextoSonidoOn;
        }
    }

    public void ReproducirAlertaPreventiva()
    {
        ReproducirClipConCooldown(clipAlertaPreventiva, ref ultimoDisparoPreventiva, CooldownAlertas, 0.85f);
    }

    public void ReproducirAlarmaCritica()
    {
        ReproducirClipConCooldown(clipAlarmaCritica, ref ultimoDisparoCritica, CooldownAlertas, 1.0f);
    }

    public void ReproducirConfirmacionEntrega()
    {
        ReproducirClipConCooldown(clipConfirmacionEntrega, ref ultimoDisparoEntrega, 0.08f, 0.80f);
    }

    public void ReproducirAcopleNeumatico(Vector3? posMundo = null)
    {
        ReproducirClipConCooldown(clipAcopleNeumaticoVerde, ref ultimoDisparoNeumatico, 0.10f, 0.80f);
    }

    public void ReproducirClickUI()
    {
        ReproducirClipConCooldown(clipClickUI, ref ultimoDisparoClick, 0.03f, 0.35f);
    }

    private void ReproducirClipConCooldown(AudioClip clip, ref float ultimoDisparo, float cooldown, float escalaRelativa)
    {
        if (!Application.isPlaying || silenciado || volumenMaestro <= 0.001f || fuenteAudio == null || clip == null)
        {
            return;
        }

        float ahora = Time.unscaledTime;
        if (ahora - ultimoDisparo < cooldown)
        {
            return;
        }

        ultimoDisparo = ahora;
        fuenteAudio.PlayOneShot(clip, Mathf.Clamp01(escalaRelativa));
    }

    private void SintetizarClipsFaltantes()
    {
        if (clipAlertaPreventiva == null) clipAlertaPreventiva = RegistrarClipProcedural(CrearClipAlertaPreventiva());
        if (clipAlarmaCritica == null) clipAlarmaCritica = RegistrarClipProcedural(CrearClipAlarmaCritica());
        if (clipConfirmacionEntrega == null) clipConfirmacionEntrega = RegistrarClipProcedural(CrearClipConfirmacionEntrega());
        if (clipAcopleNeumaticoVerde == null) clipAcopleNeumaticoVerde = RegistrarClipProcedural(CrearClipAcopleNeumaticoVerde());
        if (clipClickUI == null) clipClickUI = RegistrarClipProcedural(CrearClipClickUI());
        if (clipMotorAMR == null) clipMotorAMR = RegistrarClipProcedural(CrearClipLoopMotorAMR());
        if (clipRotoresEVE == null) clipRotoresEVE = RegistrarClipProcedural(CrearClipLoopRotores("SCADA_RotoresEVE", 85f, 18f, 0.28f, 0.055f, 137));
        if (clipRotoresDronSoporte == null) clipRotoresDronSoporte = RegistrarClipProcedural(CrearClipLoopRotores("SCADA_RotoresDronSoporte", 72f, 14f, 0.34f, 0.075f, 251));
    }

    private static float EvaluarEnvolvente(float tiempoLocal, float duracionSegmento, float ataqueSeg, float liberacionSeg)
    {
        if (tiempoLocal < 0f || tiempoLocal > duracionSegmento) return 0f;

        float factorAtaque = ataqueSeg > 0.0001f ? Mathf.Clamp01(tiempoLocal / ataqueSeg) : 1f;
        float tiempoRestante = duracionSegmento - tiempoLocal;
        float factorLiberacion = liberacionSeg > 0.0001f ? Mathf.Clamp01(tiempoRestante / liberacionSeg) : 1f;
        return Mathf.Min(factorAtaque, factorLiberacion);
    }

    private static AudioClip ConstruirClipPCM(string nombre, float[] muestras)
    {
        AudioClip clip = AudioClip.Create(nombre, muestras.Length, 1, SampleRate, false);
        clip.SetData(muestras, 0);
        return clip;
    }

    private static AudioClip CrearClipAlertaPreventiva()
    {
        const float duracion = 0.280f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];
        float fase = 0f;

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            float env = 0f;
            float freq = 880f;

            if (t <= 0.130f)
            {
                freq = 880f;
                env = EvaluarEnvolvente(t, 0.130f, 0.008f, 0.018f);
            }
            else if (t >= 0.145f)
            {
                freq = 660f;
                env = EvaluarEnvolvente(t - 0.145f, 0.135f, 0.008f, 0.018f);
            }

            fase += 2f * Mathf.PI * freq / SampleRate;
            float onda = Mathf.Sin(fase) + 0.18f * Mathf.Sin(2f * fase);
            datos[i] = Mathf.Clamp(onda * 0.45f * env, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_AlertaPreventiva", datos);
    }

    private static AudioClip CrearClipAlarmaCritica()
    {
        const float duracion = 0.360f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];
        float fase = 0f;
        const float freq = 1100f;

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            float env = 0f;

            if (t <= 0.105f)
            {
                env = EvaluarEnvolvente(t, 0.105f, 0.008f, 0.018f);
            }
            else if (t >= 0.127f && t <= 0.232f)
            {
                env = EvaluarEnvolvente(t - 0.127f, 0.105f, 0.008f, 0.018f);
            }
            else if (t >= 0.254f)
            {
                env = EvaluarEnvolvente(t - 0.254f, 0.106f, 0.008f, 0.018f);
            }

            fase += 2f * Mathf.PI * freq / SampleRate;
            float onda = Mathf.Sin(fase) + 0.22f * Mathf.Sin(2f * fase);
            datos[i] = Mathf.Clamp(onda * 0.48f * env, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_AlarmaCritica", datos);
    }

    private static AudioClip CrearClipConfirmacionEntrega()
    {
        const float duracion = 0.240f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];
        float fase = 0f;

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            bool segundoTono = t >= 0.115f;
            float freq = segundoTono ? 783.99f : 523.25f;
            float tSeg = segundoTono ? (t - 0.115f) : t;
            float durSeg = segundoTono ? 0.125f : 0.115f;
            float env = EvaluarEnvolvente(tSeg, durSeg, 0.008f, 0.018f);

            fase += 2f * Mathf.PI * freq / SampleRate;
            float onda = Mathf.Sin(fase) + 0.14f * Mathf.Sin(2f * fase);
            datos[i] = Mathf.Clamp(onda * 0.42f * env, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_ConfirmacionEntrega", datos);
    }

    private static AudioClip CrearClipAcopleNeumaticoVerde()
    {
        const float duracion = 0.320f;
        const float finSuccion = 0.160f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];
        float faseSuccion = 0f;
        float faseBloqueo = 0f;
        float ruidoFiltrado = 0f;
        System.Random rng = new System.Random(4096);

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            float blanco = (float)(rng.NextDouble() * 2.0 - 1.0);
            ruidoFiltrado = ruidoFiltrado * 0.86f + blanco * 0.14f;

            float muestra = 0f;
            if (t < finSuccion)
            {
                float prog = Mathf.Clamp01(t / finSuccion);
                float freqSuccion = Mathf.Lerp(240f, 420f, prog);
                float env = EvaluarEnvolvente(t, finSuccion, 0.010f, 0.022f);
                faseSuccion += 2f * Mathf.PI * freqSuccion / SampleRate;
                float tonoVacio = Mathf.Sin(faseSuccion) * 0.35f + ruidoFiltrado * 0.65f * (1f - 0.4f * prog);
                muestra = tonoVacio * 0.50f * env;
            }
            else
            {
                float tSeg = t - finSuccion;
                float durSeg = duracion - finSuccion;
                float prog = Mathf.Clamp01(tSeg / durSeg);
                float freqBloqueo = Mathf.Lerp(660f, 990f, Mathf.SmoothStep(0f, 1f, prog));
                float env = EvaluarEnvolvente(tSeg, durSeg, 0.008f, 0.024f);
                faseBloqueo += 2f * Mathf.PI * freqBloqueo / SampleRate;
                float tono = Mathf.Sin(faseBloqueo) + 0.15f * Mathf.Sin(2f * faseBloqueo);
                muestra = tono * 0.42f * env;
            }

            datos[i] = Mathf.Clamp(muestra, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_AcopleNeumaticoVerde", datos);
    }

    private static AudioClip CrearClipClickUI()
    {
        const float duracion = 0.028f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];
        float fase = 0f;
        const float freq = 1200f;

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            float envAtaque = EvaluarEnvolvente(t, duracion, 0.002f, 0.006f);
            float decaimiento = Mathf.Exp(-t * 140f);
            fase += 2f * Mathf.PI * freq / SampleRate;
            datos[i] = Mathf.Clamp(Mathf.Sin(fase) * 0.45f * envAtaque * decaimiento, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_ClickUI", datos);
    }

    private static AudioClip CrearClipLoopMotorAMR()
    {
        const float duracion = 2.0f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] datos = new float[totalMuestras];

        for (int i = 0; i < totalMuestras; i++)
        {
            float t = (float)i / SampleRate;
            float faseBase = 2f * Mathf.PI * 110f * t;
            float onda = 0.72f * Mathf.Sin(faseBase)
                       + 0.22f * Mathf.Sin(2f * faseBase)
                       + 0.08f * Mathf.Sin(3f * faseBase);
            datos[i] = Mathf.Clamp(onda * 0.36f, -1f, 1f);
        }

        return ConstruirClipPCM("SCADA_MotorAMR_Loop", datos);
    }

    private static AudioClip CrearClipLoopRotores(string nombre, float freqFundamental, float freqAspas, float amplitudBase, float amplitudAire, int semilla)
    {
        const float duracion = 2.0f;
        int totalMuestras = Mathf.RoundToInt(SampleRate * duracion);
        float[] ruidoBase = new float[totalMuestras];
        System.Random rng = new System.Random(semilla);
        for (int i = 0; i < totalMuestras; i++)
        {
            ruidoBase[i] = (float)(rng.NextDouble() * 2.0 - 1.0);
        }

        // Suavizado circular periódico para evitar clics al repetir el loop en t = 2.0s -> 0.0s
        const int ventanaFiltro = 10;
        float[] datos = new float[totalMuestras];
        for (int i = 0; i < totalMuestras; i++)
        {
            float suma = 0f;
            for (int k = 0; k < ventanaFiltro; k++)
            {
                suma += ruidoBase[(i + k) % totalMuestras];
            }
            float aireFiltrado = suma / ventanaFiltro;

            float t = (float)i / SampleRate;
            float modulacionAspas = 0.78f + 0.22f * Mathf.Sin(2f * Mathf.PI * freqAspas * t);
            float faseFund = 2f * Mathf.PI * freqFundamental * t;
            float motor = (0.75f * Mathf.Sin(faseFund) + 0.25f * Mathf.Sin(2f * faseFund)) * modulacionAspas;

            datos[i] = Mathf.Clamp(motor * amplitudBase + aireFiltrado * amplitudAire * modulacionAspas, -1f, 1f);
        }

        return ConstruirClipPCM(nombre, datos);
    }
}
