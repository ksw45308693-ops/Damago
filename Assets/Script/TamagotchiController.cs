using System;
using UnityEngine;

public enum TamaState { Idle, Eating, Sleeping, Playing, Sick, Dead }

public class TamagotchiController : MonoBehaviour
{
    public event Action OnStatusChanged;

    [Header("설정")]
    public TamagotchiData species;

    [Header("현재 상태")]
    public float hunger;
    public float energy;
    public float happiness;
    public float hygiene;
    public int ageDays;
    public TamaState state = TamaState.Idle;

    private DateTime lastUpdate;
    private bool isInitialized = false;

    void Start()
    {
        if (species == null)
        {
            Debug.LogError("🚨 Error: Species Data가 연결되지 않았습니다!");
            return;
        }

        var savedData = SaveLoadManager.Load();
        if (savedData != null)
        {
            FromSaveData(savedData);

            // [수정] 죽지 않았을 때만 오프라인 시간 경과 적용
            if (state != TamaState.Dead)
            {
                long lastTime = savedData.lastSavedUnix;
                DateTime lastDateTime = DateTimeOffset.FromUnixTimeSeconds(lastTime).UtcDateTime;
                float timePassed = (float)(DateTime.UtcNow - lastDateTime).TotalSeconds;
                if (timePassed > 0) ApplyPassiveDecay(timePassed);
            }
        }
        else
        {
            InitStats(); // 초기화 로직 분리
        }

        lastUpdate = DateTime.UtcNow;
        isInitialized = true;
        OnStatusChanged?.Invoke();
    }

    // [추가] 스탯 초기화 함수 (시작/부활 공용)
    void InitStats()
    {
        hunger = species.maxStat;
        energy = species.maxStat;
        happiness = species.maxStat;
        hygiene = species.maxStat;
        ageDays = 0;
        state = TamaState.Idle;
    }

    void Update()
    {
        if (!isInitialized || state == TamaState.Dead) return; // [수정] 죽으면 시간 경과 중지

        DateTime now = DateTime.UtcNow;
        float deltaSec = (float)(now - lastUpdate).TotalSeconds;

        if (deltaSec >= 1f)
        {
            ApplyPassiveDecay(deltaSec);
            lastUpdate = now;
            OnStatusChanged?.Invoke();
        }
    }

    void ApplyPassiveDecay(float deltaSeconds)
    {
        float hours = deltaSeconds / 3600f;

        hunger -= species.hungerDecayPerHour * hours;
        energy -= species.energyDecayPerHour * hours;
        happiness -= species.happinessDecayPerHour * hours;
        hygiene -= species.hygieneDecayPerHour * hours;

        // [수정] 상태 체크 로직 강화 (죽음 조건 추가)
        if (hunger <= 0 || energy <= 0) // 배고픔이나 에너지가 0이면 사망
        {
            state = TamaState.Dead;
            hunger = 0; energy = 0; // 보기 좋게 0으로 고정
        }
        else if (hunger < 30 || energy < 30 || hygiene < 30) // 30 미만이면 아픔
        {
            state = TamaState.Sick;
        }
        else
        {
            // 아프거나 죽지 않았다면 기본 상태 (행동 중일 때는 상태 유지 필요하지만 여기선 단순화)
            if (state != TamaState.Sleeping && state != TamaState.Eating && state != TamaState.Playing)
                state = TamaState.Idle;
        }

        ClampAll();
    }

    void ClampAll()
    {
        hunger = Mathf.Clamp(hunger, 0f, species.maxStat);
        energy = Mathf.Clamp(energy, 0f, species.maxStat);
        happiness = Mathf.Clamp(happiness, 0f, species.maxStat);
        hygiene = Mathf.Clamp(hygiene, 0f, species.maxStat);
    }

    // [추가] 부활 기능
    public void Revive()
    {
        InitStats(); // 스탯 꽉 채우기
        lastUpdate = DateTime.UtcNow; // 시간 재설정
        SaveLoadManager.Save(ToSaveData(species.name)); // 즉시 저장
        OnStatusChanged?.Invoke(); // UI 갱신
        Debug.Log("✨ 부활했습니다!");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!isInitialized) return;

        if (pauseStatus)
        {
            SaveLoadManager.Save(ToSaveData(species.name));
        }
        else
        {
            if (state == TamaState.Dead) return; // 죽어있으면 계산 스킵

            DateTime now = DateTime.UtcNow;
            float deltaSec = (float)(now - lastUpdate).TotalSeconds;
            if (deltaSec > 0)
            {
                ApplyPassiveDecay(deltaSec);
                lastUpdate = now;
                OnStatusChanged?.Invoke();
            }
        }
    }

    void OnApplicationQuit()
    {
        if (isInitialized) SaveLoadManager.Save(ToSaveData(species.name));
    }

    // ---- 행동 함수 (죽음 체크 추가됨) ----
    public void Feed()
    {
        if (state == TamaState.Dead) return; // [추가] 죽으면 실행 불가
        hunger += species.feedRestore;
        state = TamaState.Eating;
        ClampAll();
        OnStatusChanged?.Invoke();
    }

    public void Play()
    {
        if (state == TamaState.Dead) return; // [추가]
        happiness += species.playRestore;
        energy -= 10f;
        state = TamaState.Playing;
        ClampAll();
        OnStatusChanged?.Invoke();
    }

    public void Sleep()
    {
        if (state == TamaState.Dead) return; // [추가]
        energy += 50f;
        state = TamaState.Sleeping;
        ClampAll();
        OnStatusChanged?.Invoke();
    }

    public void Wash()
    {
        if (state == TamaState.Dead) return; // [추가]
        hygiene = species.maxStat;
        ClampAll();
        OnStatusChanged?.Invoke();
    }

    public TamagotchiSaveData ToSaveData(string id)
    {
        return new TamagotchiSaveData
        {
            speciesId = id,
            hunger = this.hunger,
            energy = this.energy,
            happiness = this.happiness,
            hygiene = this.hygiene,
            ageDays = this.ageDays,
            state = this.state.ToString(),
            lastSavedUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    public void FromSaveData(TamagotchiSaveData data)
    {
        this.hunger = data.hunger;
        this.energy = data.energy;
        this.happiness = data.happiness;
        this.hygiene = data.hygiene;
        this.ageDays = data.ageDays;
        if (Enum.TryParse(data.state, out TamaState parsedState)) this.state = parsedState;
    }
}