using UnityEngine;
using System;
using System.IO;

[Serializable]
public class TamagotchiSaveData
{
    public string speciesId;
    public float hunger;
    public float energy;
    public float happiness;
    public float hygiene;
    public int ageDays;
    public string state;
    public long lastSavedUnix; // 저장된 시점의 유닉스 시간 (초)
}

public static class SaveLoadManager
{
    // 저장 경로 설정 (PC/모바일 모두 작동)
    private static string SavePath => Path.Combine(Application.persistentDataPath, "tama_save.json");

    public static void Save(TamagotchiSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true); // true: 보기 좋게 줄바꿈
            File.WriteAllText(SavePath, json);
            Debug.Log($"[Save] 저장 완료: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 저장 실패: {e.Message}");
        }
    }

    public static TamagotchiSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Load] 저장된 파일이 없습니다. 새로 시작합니다.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<TamagotchiSaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Load] 불러오기 실패: {e.Message}");
            return null;
        }
    }

    // 개발용: 데이터 삭제 기능
    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}