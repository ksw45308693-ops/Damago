using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public TamagotchiController tama;

    [Header("UI 연결")]
    public Slider hungerSlider;
    public Slider energySlider;
    public Slider happinessSlider;
    public Slider hygieneSlider;
    public Text stateText;

    [Header("게임오버 UI")]
    public GameObject gameOverPanel; // [추가] 인스펙터에서 패널 연결하세요

    void Start()
    {
        if (tama == null) return;
        tama.OnStatusChanged += UpdateUI;
        UpdateUI();
    }

    void OnDestroy()
    {
        if (tama != null)
            tama.OnStatusChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        if (tama.species == null) return;

        float max = tama.species.maxStat;

        if (hungerSlider) hungerSlider.value = tama.hunger / max;
        if (energySlider) energySlider.value = tama.energy / max;
        if (happinessSlider) happinessSlider.value = tama.happiness / max;
        if (hygieneSlider) hygieneSlider.value = tama.hygiene / max;

        if (stateText) stateText.text = $"State: {tama.state}";

        // [추가] 죽었으면 게임오버 패널 켜기, 아니면 끄기
        if (gameOverPanel != null)
        {
            bool isDead = (tama.state == TamaState.Dead);
            gameOverPanel.SetActive(isDead);
        }
    }

    // 버튼 연결 함수들
    public void OnClickFeed() => tama.Feed();
    public void OnClickPlay() => tama.Play();
    public void OnClickSleep() => tama.Sleep();
    public void OnClickWash() => tama.Wash();

    // [추가] 다시하기 버튼용
    public void OnClickRestart()
    {
        tama.Revive();
    }
}