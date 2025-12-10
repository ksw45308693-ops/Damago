using UnityEngine;

[CreateAssetMenu(menuName = "Tamagotchi/Species", fileName = "NewSpeciesData")]
public class TamagotchiData : ScriptableObject
{
    [Header("기본 설정")]
    public string speciesName = "Tama";
    public float maxStat = 100f;

    [Header("시간당 감소량 (Decay/Hour)")]
    public float hungerDecayPerHour = 10f;
    public float energyDecayPerHour = 5f;
    public float happinessDecayPerHour = 5f;
    public float hygieneDecayPerHour = 2f;

    [Header("행동당 회복량")]
    public float feedRestore = 30f;
    public float playRestore = 25f;
    public float sleepRestorePerHour = 40f; // 자는 동안 시간당 회복량
}