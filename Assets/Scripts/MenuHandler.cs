using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    [NotNull] [SerializeField] private Image fadeImage;
    [NotNull] [SerializeField] private Text title;
    [NotNull] [SerializeField] private CanvasGroup mainMenu;
    [NotNull] [SerializeField] private CanvasGroup optionsMenu;

    [NotNull] [SerializeField] private CanvasGroup gameplayMenu;
    [NotNull] [SerializeField] private CanvasGroup videoMenu;
    [NotNull] [SerializeField] private CanvasGroup audioMenu;

    [NotNull] [SerializeField] private Dropdown resolutionDropdown;
    [NotNull] [SerializeField] private Dropdown refreshRateDropdown;
    [NotNull] [SerializeField] private Dropdown windowModeDropdown;
    [NotNull] [SerializeField] private Dropdown qualityDropdown;

    [NotNull] [SerializeField] private Slider masterSlider;
    [NotNull] [SerializeField] private Slider musicSlider;
    [NotNull] [SerializeField] private Slider sfxSlider;
    [NotNull] [SerializeField] private Slider screenShakeSlider;
    [NotNull] [SerializeField] private Slider flashEffectsSlider;

    [NotNull] private readonly Dictionary<GameObject, Text> keys = new Dictionary<GameObject, Text>();
    [NotNull] private readonly Dictionary<GameObject, string> keysActions = new Dictionary<GameObject, string>();

    [NotNull] [SerializeField] private Button jumpKeyButton;
    [NotNull] [SerializeField] private Button leftKeyButton;
    [NotNull] [SerializeField] private Button rightKeyButton;

    [NotNull] [SerializeField] private Button playButton;
    [NotNull] [SerializeField] private Button resumeButton;

    [NotNull] [SerializeField] private Image darknessImage;

    [NotNull] [SerializeField] private AudioSource menuClick;

    // To get keycode do this
    // Enum.TryParse(keys[selectedKey].text, true, out KeyCode key);

    [CanBeNull] private GameObject selectedKey;
    [CanBeNull] private PlayerController player;

    [NotNull] private List<Resolution> resolutions = new List<Resolution>();
    [NotNull] private List<int> refreshRates = new List<int>();

    private FullScreenMode fullScreenMode = FullScreenMode.Windowed;

    [NotNull] public AudioMixer audioMixer;

    public void PlayMenuClick()
    {
        menuClick.Play();
    }

    void AddRefreshRates()
    {
        refreshRates.Clear();

        var temp = Screen.resolutions.ToList().Distinct();
        foreach (var resolution in temp)
        {
            refreshRates.Add(resolution.refreshRate);
        }

        refreshRates = refreshRates.OrderByDescending(i => i).Distinct().ToList();
    }

    void FixRefreshRateDropdown()
    {
        var options = new List<string>();
        var currentRefreshIndex = 0;

        var i = 0;
        foreach (var option in refreshRates)
        {
            options.Add(option.ToString());
            if (refreshRates[i] == Screen.currentResolution.refreshRate)
                currentRefreshIndex = i;
            i++;
        }
        refreshRateDropdown.ClearOptions();
        refreshRateDropdown.AddOptions(options);
        if(!PlayerPrefs.HasKey("RefreshIndex"))
            refreshRateDropdown.value = currentRefreshIndex;
        refreshRateDropdown.RefreshShownValue();
    }

    void AddResolutions()
    {
        resolutions.Clear();
        resolutions.AddRange(Screen.resolutions.ToList().Distinct());
        
 
        resolutions = resolutions.OrderByDescending(i => i.width).ToList();
    }

    void FixResolutionDropdown()
    {
        var options = new List<string>();
        var currentResolutionIndex = 0;

        var refreshRate = refreshRates[refreshRateDropdown.value];
        var resolutionsOptions = new List<Resolution>();
        resolutionsOptions.AddRange(resolutions.Where(resolution => resolution.refreshRate == refreshRate).ToList());
 
        var i = 0;
        foreach (var resolution in resolutionsOptions)
        {
            options.Add(resolution.width + " x " + resolution.height);
            if (resolutionsOptions[i].width == Screen.width && resolutionsOptions[i].height == Screen.height)
                currentResolutionIndex = i;
            i++;
        }

        resolutions = resolutionsOptions;

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        if(!PlayerPrefs.HasKey("ResolutionIndex"))
            resolutionDropdown.value = currentResolutionIndex;

        resolutionDropdown.RefreshShownValue();
    }

    // Start is called before the first frame update
    void Start()
    {
        AddRefreshRates();
        FixRefreshRateDropdown();
        AddResolutions();
        FixResolutionDropdown();

        resolutionDropdown.onValueChanged.AddListener(delegate
        {
            PlayMenuClick();
            Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, fullScreenMode , refreshRates[refreshRateDropdown.value]);
            PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        });

        refreshRateDropdown.onValueChanged.AddListener(delegate
        {
            PlayMenuClick();
            Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, fullScreenMode, refreshRates[refreshRateDropdown.value]);
            PlayerPrefs.SetInt("RefreshIndex", refreshRateDropdown.value);
        });

        keys.Add(jumpKeyButton.gameObject, jumpKeyButton.GetComponentInChildren<Text>());
        keys.Add(leftKeyButton.gameObject, leftKeyButton.GetComponentInChildren<Text>());
        keys.Add(rightKeyButton.gameObject, rightKeyButton.GetComponentInChildren<Text>());

        keysActions.Add(jumpKeyButton.gameObject, "jump");
        keysActions.Add(leftKeyButton.gameObject, "left");
        keysActions.Add(rightKeyButton.gameObject, "right");

        jumpKeyButton.onClick.AddListener(delegate
        {
            PlayMenuClick();
            selectedKey = jumpKeyButton.gameObject;
            keys[selectedKey].text = "Press any key";
        });

        leftKeyButton.onClick.AddListener(delegate
        {
            PlayMenuClick();
            selectedKey = leftKeyButton.gameObject;
            keys[selectedKey].text = "Press any key";

        });

        rightKeyButton.onClick.AddListener(delegate
        {
            PlayMenuClick();
            selectedKey = rightKeyButton.gameObject;
            keys[selectedKey].text = "Press any key";

        });

        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
        refreshRateDropdown.value = PlayerPrefs.GetInt("RefreshIndex");

        if(!PlayerPrefs.HasKey("WindowMode"))
            PlayerPrefs.SetInt("WindowMode", 0);
        if(!PlayerPrefs.HasKey("Quality"))
            PlayerPrefs.SetInt("Quality", 0);
        if(!PlayerPrefs.HasKey("Master"))
            PlayerPrefs.SetFloat("Master", 1);
        if(!PlayerPrefs.HasKey("Music"))
            PlayerPrefs.SetFloat("Music", 1);
        if(!PlayerPrefs.HasKey("SFX"))
            PlayerPrefs.SetFloat("SFX", 1);
        if(!PlayerPrefs.HasKey("ScreenShake"))
            PlayerPrefs.SetFloat("ScreenShake", 1);
        if(!PlayerPrefs.HasKey("FlashEffects"))
            PlayerPrefs.SetFloat("FlashEffects", 1);

        windowModeDropdown.value = PlayerPrefs.GetInt("WindowMode");
        qualityDropdown.value = PlayerPrefs.GetInt("Quality");
        masterSlider.value = PlayerPrefs.GetFloat("Master");
        musicSlider.value = PlayerPrefs.GetFloat("Music");
        sfxSlider.value = PlayerPrefs.GetFloat("SFX");
        screenShakeSlider.value = PlayerPrefs.GetFloat("ScreenShake");
        flashEffectsSlider.value = PlayerPrefs.GetFloat("FlashEffects");

        keys[jumpKeyButton.gameObject].text = PlayerPrefs.GetString(keysActions[jumpKeyButton.gameObject].ToString());
        keys[leftKeyButton.gameObject].text = PlayerPrefs.GetString(keysActions[leftKeyButton.gameObject].ToString());
        keys[rightKeyButton.gameObject].text = PlayerPrefs.GetString(keysActions[rightKeyButton.gameObject].ToString());
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackButton()
    {
        if(!player)
            darknessImage.gameObject.SetActive(false);
        mainMenu.alpha = 1;
        mainMenu.interactable = true;
        mainMenu.blocksRaycasts = true;

        optionsMenu.alpha = 0;
        optionsMenu.interactable = false;
        optionsMenu.blocksRaycasts = false;
    }

    public void OptionsButton()
    {
        darknessImage.gameObject.SetActive(true);
        mainMenu.alpha = 0;
        mainMenu.interactable = false;
        mainMenu.blocksRaycasts = false;

        optionsMenu.alpha = 1;
        optionsMenu.interactable = true;
        optionsMenu.blocksRaycasts = true;
    }

    public void GameplayButton()
    {
        gameplayMenu.alpha = 1;
        gameplayMenu.interactable = true;
        gameplayMenu.blocksRaycasts = true;

        videoMenu.alpha = 0;
        videoMenu.interactable = false;
        videoMenu.blocksRaycasts = false;

        audioMenu.alpha = 0;
        audioMenu.interactable = false;
        audioMenu.blocksRaycasts = false;
    }

    public void VideoButton()
    {
        gameplayMenu.alpha = 0;
        gameplayMenu.interactable = false;
        gameplayMenu.blocksRaycasts = false;

        videoMenu.alpha = 1;
        videoMenu.interactable = true;
        videoMenu.blocksRaycasts = true;

        audioMenu.alpha = 0;
        audioMenu.interactable = false;
        audioMenu.blocksRaycasts = false;
    }

    public void AudioButton()
    {
        gameplayMenu.alpha = 0;
        gameplayMenu.interactable = false;
        gameplayMenu.blocksRaycasts = false;

        videoMenu.alpha = 0;
        videoMenu.interactable = false;
        videoMenu.blocksRaycasts = false;

        audioMenu.alpha = 1;
        audioMenu.interactable = true;
        audioMenu.blocksRaycasts = true;
    }

    public void PlayButton()
    {
        StartCoroutine(Play());
    }

    public void HideMenu()
    {
        if(player)
            player.inMenu = false;

        darknessImage.gameObject.SetActive(false);
        mainMenu.alpha = 0;
        mainMenu.interactable = false;
        mainMenu.blocksRaycasts = false;

        optionsMenu.alpha = 0;
        optionsMenu.interactable = false;
        optionsMenu.blocksRaycasts = false;

        title.gameObject.SetActive(false);
    }

    public void ShowMenu()
    {
        if(player)
            player.inMenu = true;

        darknessImage.gameObject.SetActive(true);
        mainMenu.alpha = 1;
        mainMenu.interactable = true;
        mainMenu.blocksRaycasts = true;

        optionsMenu.alpha = 0;
        optionsMenu.interactable = false;
        optionsMenu.blocksRaycasts = false;

        title.gameObject.SetActive(true);

    }

    IEnumerator Play()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        asyncLoad.allowSceneActivation = false;
        yield return StartCoroutine(FadeOut(1.5f, Color.black));

        yield return new WaitForSeconds(0.1f);
        asyncLoad.allowSceneActivation = true;
        HideMenu();
        playButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);

        yield return StartCoroutine(FadeOut(1.0f, Color.clear));

        player = FindObjectOfType<PlayerController>();
        player.menu = this;
        GetComponent<Canvas>().worldCamera = FindObjectOfType<Camera>();
        Enum.TryParse(keys[leftKeyButton.gameObject].text, true, out KeyCode leftKey);
        player.leftKeyCode = leftKey;
        Enum.TryParse(keys[rightKeyButton.gameObject].text, true, out KeyCode rightKey);
        player.rightKeyCode = rightKey;
        Enum.TryParse(keys[jumpKeyButton.gameObject].text, true, out KeyCode jumpKey);
        player.jumpKeyCode = jumpKey;


    }

    public void QuitButton()
    {
        StartCoroutine(Quit());
    }

    IEnumerator Quit()
    {
        yield return StartCoroutine(FadeOut(2, Color.black));
        yield return new WaitForSeconds(0.1f);
        Application.Quit();
    }

    IEnumerator FadeOut(float duration, Color fadeColor)
    { 
        float time = 0;
        var startValue = fadeImage.color;
        var endValue = fadeColor;
        while (time < duration)
        {
            var t = time / duration;

            t = t * t * t * (t * (6f * t - 15f) + 10f); // Ease in and out smoothly

            fadeImage.color = Color.Lerp(startValue, endValue, t);
            time += Time.deltaTime;

            yield return null;
        }
        fadeImage.color = endValue;
    }

    public void SetWindowMode(Int32 index)
    {
        fullScreenMode = index switch
        {
            0 => FullScreenMode.Windowed,
            1 => FullScreenMode.FullScreenWindow,
            2 => FullScreenMode.ExclusiveFullScreen,
            _ => fullScreenMode
        };

        Cursor.lockState = fullScreenMode == FullScreenMode.Windowed ? CursorLockMode.None : CursorLockMode.Confined;

        Screen.SetResolution(resolutions[resolutionDropdown.value].width, resolutions[resolutionDropdown.value].height, fullScreenMode , refreshRates[refreshRateDropdown.value]);
        PlayerPrefs.SetInt("WindowMode", index);
    }

    public void SetQualityMode(Int32 index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("Quality", index);
    }

    public void SetMasterLevel(float level)
    {
        PlayerPrefs.SetFloat("Master", level);
        var multi = 20;
        if (level <= 0)
            multi = 0;

        level = Mathf.Log10(level) * multi;
        audioMixer.SetFloat("Master", level);
    }

    public void SetSFXLevel(float level)
    {
        PlayerPrefs.SetFloat("SFX", level);
        var multi = 20;
        if (level <= 0)
            multi = 0;

        level = Mathf.Log10(level) * multi;
        audioMixer.SetFloat("SFX", level);
    }
 
    public void SetMusicLevel(float level) 
    {
        PlayerPrefs.SetFloat("Music", level);
        var multi = 20;
        if (level <= 0)
            multi = 0;
        
        level = Mathf.Log10(level) * multi;
        audioMixer.SetFloat("Music", level);
    }

    public void SetScreenShake(float level) 
    {
        PlayerPrefs.SetFloat("ScreenShake", level);
    }

    public void SetFlashEffects(float level) 
    {
        PlayerPrefs.SetFloat("FlashEffects", level);
    }

    private void OnGUI()
    {
        if (selectedKey == null) 
            return;

        var e = Event.current;
        if (!e.isKey) 
            return;

        keys[selectedKey].text = e.keyCode.ToString();
        PlayerPrefs.SetString(keysActions[selectedKey].ToString(), e.keyCode.ToString());

        if (player)
        {
            Enum.TryParse(keys[leftKeyButton.gameObject].text, true, out KeyCode leftKey);
            player.leftKeyCode = leftKey;
            Enum.TryParse(keys[rightKeyButton.gameObject].text, true, out KeyCode rightKey);
            player.rightKeyCode = rightKey;
            Enum.TryParse(keys[jumpKeyButton.gameObject].text, true, out KeyCode jumpKey);
            player.jumpKeyCode = jumpKey;
        }

        selectedKey = null;
    }
}
