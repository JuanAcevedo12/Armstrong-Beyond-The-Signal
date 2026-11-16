using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class HomeIntro : MonoBehaviour
{
    [Header("Logos")]
    [SerializeField] private Sprite logo1;
    [SerializeField] private Sprite logo2;
    [SerializeField] private Sprite logo3;
    [SerializeField, Min(0f)] private float secondsPerLogo = 5f;
    [SerializeField, Min(0f)] private float fadeSeconds = 1f;

    [Header("Transition")]
    [Tooltip("Path of the destination scene in the active build profile.")]
    [SerializeField] private string destinationScene = "Assets/Scenes/Prototipo.unity";

    [Header("Canvas references")]
    [SerializeField] private Image logoImage;
    [SerializeField] private Text placeholder;
    [SerializeField] private CanvasGroup blackOverlay;

    [Header("Intro audio")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip englishIntroClip;
    [SerializeField, Range(0.05f,0.5f)] private float languageFadeSeconds = .16f;
    private AudioSource introSource, englishIntroSource, activeNarration;
    private float spanishTarget, englishTarget, narrationProgress;
    private bool narrationStarted;
    public bool IntroFinished { get; private set; }
    public bool TransitionStarted { get; private set; }


    private Text languageHint;
    private void Awake()
    {
        var root = new GameObject("HomeLanguageControls", typeof(Canvas), typeof(CanvasScaler));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, gameObject.scene);
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 6000;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080);
        var panel = new GameObject("LanguageHint", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(root.transform,false);
        var pr=panel.GetComponent<RectTransform>();pr.anchorMin=new Vector2(0,0);pr.anchorMax=new Vector2(1,0);pr.pivot=new Vector2(.5f,0);pr.offsetMin=new Vector2(24,18);pr.offsetMax=new Vector2(-24,112);
        panel.GetComponent<Image>().color=new Color(0,0,0,.7f);panel.GetComponent<Image>().raycastTarget=false;
        var label=new GameObject("HintText",typeof(RectTransform),typeof(Caronte.LocalizedText));label.transform.SetParent(panel.transform,false);
        var rt=label.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(12,4);rt.offsetMax=new Vector2(-12,-4);
        languageHint=label.GetComponent<Text>();languageHint.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");languageHint.fontSize=26;languageHint.resizeTextForBestFit=true;languageHint.resizeTextMinSize=16;languageHint.resizeTextMaxSize=26;languageHint.alignment=TextAnchor.MiddleCenter;languageHint.color=Color.white;languageHint.raycastTarget=false;
        languageHint.text=Caronte.MobileRuntime.Active?"Toca Idioma para cambiar entre español e inglés · Saltar intro para ir al taller\nIdioma: Español":"Pulsa Intro para cambiar idioma: Español / Inglés · Esc para saltar al taller\nIdioma: Español";
        if(Caronte.MobileRuntime.Active)BuildTouchButtons(root);
    }
    public void SkipIntro(){if(TransitionStarted)return;TransitionStarted=true;IntroFinished=true;StopAllCoroutines();StopNarration();Time.timeScale=1;AudioListener.pause=false;SceneManager.LoadSceneAsync(destinationScene,LoadSceneMode.Single);}
    private void BuildTouchButtons(GameObject root){
        Caronte.MobileRuntime.EnsureEvents();root.AddComponent<GraphicRaycaster>();
        string[] labels={"Idioma: Español","Saltar intro"};
        for(int i=0;i<2;i++){int option=i;var b=new GameObject("TouchHomeButton",typeof(RectTransform),typeof(Image),typeof(Button));b.transform.SetParent(root.transform,false);var r=b.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,0);r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(i==0?-200:200,140);r.sizeDelta=new Vector2(370,120);b.GetComponent<Image>().color=new Color(.03f,.23f,.33f,.97f);b.GetComponent<Button>().onClick.AddListener(()=>{if(option==0)SwitchLanguage();else SkipIntro();});var g=new GameObject("Label",typeof(RectTransform),typeof(Caronte.LocalizedText));g.transform.SetParent(b.transform,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=new Vector2(8,4);rt.offsetMax=new Vector2(-8,-4);var text=g.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=36;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.text=labels[i];text.raycastTarget=false;}
    }
    private void Update()
    {
        if(TransitionStarted)return;
        if(activeNarration&&activeNarration.isPlaying&&activeNarration.clip)narrationProgress=Mathf.Clamp01(activeNarration.time/activeNarration.clip.length);
        float step=.85f*Time.unscaledDeltaTime/Mathf.Max(.05f,languageFadeSeconds);
        if(introSource)introSource.volume=Mathf.MoveTowards(introSource.volume,spanishTarget,step);
        if(englishIntroSource)englishIntroSource.volume=Mathf.MoveTowards(englishIntroSource.volume,englishTarget,step);
        if(IntroFinished)return;
        var keyboard=UnityEngine.InputSystem.Keyboard.current;if(keyboard==null)return;
        if(keyboard.enterKey.wasPressedThisFrame||keyboard.numpadEnterKey.wasPressedThisFrame)SwitchLanguage();
        if(keyboard.escapeKey.wasPressedThisFrame)
        {
            SkipIntro();
        }
    }


    private void OnEnable(){Caronte.GameLanguage.Changed+=ChangeNarrationLanguage;}
    private void OnDisable(){Caronte.GameLanguage.Changed-=ChangeNarrationLanguage;StopNarration();}
    private void SwitchLanguage(){if(!IntroFinished&&!TransitionStarted)Caronte.GameLanguage.Toggle();}
    private AudioSource NarrationSource(string name,AudioClip clip)
    {
        if(!clip)return null;
        var g=new GameObject(name);g.transform.SetParent(transform,false);
        var source=g.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;source.volume=0;source.clip=clip;source.loop=false;source.pitch=1;
        return source;
    }
    private void ChangeNarrationLanguage()
    {
        if(!narrationStarted||IntroFinished||TransitionStarted)return;
        var next=Caronte.GameLanguage.English&&englishIntroSource?englishIntroSource:introSource;
        if(!next)next=englishIntroSource;
        if(next&&activeNarration&&next!=activeNarration)
        {
            float progress=activeNarration.isPlaying&&activeNarration.clip.length>0?Mathf.Clamp01(activeNarration.time/activeNarration.clip.length):narrationProgress;
            if(progress<.999f){if(!next.isPlaying)next.Play();next.time=Mathf.Min(next.clip.length-.02f,progress*next.clip.length);}
        }
        activeNarration=next;
        spanishTarget=activeNarration==introSource?.85f:0;
        englishTarget=activeNarration==englishIntroSource?.85f:0;
    }
    private void StopNarration()
    {
        narrationStarted=false;if(introSource)introSource.Stop();if(englishIntroSource)englishIntroSource.Stop();
    }

    private IEnumerator Start()
    {
        blackOverlay.alpha = 1f;
if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
        bool entered = false;
        var gate = new GameObject("StartAudioGate", typeof(RectTransform), typeof(Image), typeof(Button));
        gate.transform.SetParent(blackOverlay.transform.parent, false);
        var gr = gate.GetComponent<RectTransform>(); gr.anchorMin = Vector2.zero; gr.anchorMax = Vector2.one; gr.offsetMin = gr.offsetMax = Vector2.zero;
        gate.GetComponent<Image>().color = Color.black;
        var label = new GameObject("StartLabel", typeof(RectTransform), typeof(Caronte.LocalizedText));label.transform.SetParent(gate.transform, false);
        var lr = label.GetComponent<RectTransform>();lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
        var text = label.GetComponent<Text>();text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=32;text.color=Color.white;text.alignment=TextAnchor.MiddleCenter;text.text="CARONTE — MISSION ARCHITECT\n\nHaz clic para iniciar con audio";text.raycastTarget=false;
        var canvas = gate.GetComponentInParent<Canvas>();
        if (!canvas.GetComponent<GraphicRaycaster>()) canvas.gameObject.AddComponent<GraphicRaycaster>();
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            var events = new GameObject("IntroEventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(events, gameObject.scene);
        }
        gate.GetComponent<Button>().onClick.AddListener(() => entered = true);
        while (!entered) yield return null;
        Destroy(gate);
        }
        if(introClip)introClip.LoadAudioData();
        if(englishIntroClip)englishIntroClip.LoadAudioData();
        while((introClip&&introClip.loadState==AudioDataLoadState.Loading)||(englishIntroClip&&englishIntroClip.loadState==AudioDataLoadState.Loading))yield return null;
        introSource=NarrationSource("CortanaSpanish",introClip);
        englishIntroSource=NarrationSource("CortanaEnglish",englishIntroClip);
        narrationStarted=true;ChangeNarrationLanguage();
        if(introSource){introSource.volume=spanishTarget;introSource.Play();}
        if(englishIntroSource){englishIntroSource.volume=englishTarget;englishIntroSource.Play();}
        ShowLogo(logo1, 1);
        yield return FadeTo(0f);
        yield return new WaitForSecondsRealtime(secondsPerLogo);
        ShowLogo(logo2, 2);
        yield return new WaitForSecondsRealtime(secondsPerLogo);
        ShowLogo(logo3, 3);
        yield return new WaitForSecondsRealtime(secondsPerLogo);

        if (SceneUtility.GetBuildIndexByScenePath(destinationScene) < 0)
        {
            Debug.LogError($"Home intro destination is missing from the build scene list: {destinationScene}", this);
            yield break;
        }

        while (activeNarration && activeNarration.isPlaying) yield return null;
        IntroFinished = true;StopNarration();
        yield return FadeTo(1f);
        TransitionStarted = true;
        yield return SceneManager.LoadSceneAsync(destinationScene, LoadSceneMode.Single);
    }

    private void ShowLogo(Sprite sprite, int number)
    {
        logoImage.sprite = sprite;
        logoImage.enabled = sprite != null;
        placeholder.text = $"Logo {number}";
        placeholder.enabled = sprite == null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = blackOverlay.alpha;
        for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            blackOverlay.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSeconds);
            yield return null;
        }

        blackOverlay.alpha = targetAlpha;
    }
}
