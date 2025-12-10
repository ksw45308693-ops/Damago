using UnityEngine;

public class TamaVisualController : MonoBehaviour
{
    [Header("연결 정보")]
    public TamagotchiController tama; // GameManager 연결
    public Animator animator;         // 캐릭터에 붙어있는 Animator 컴포넌트

    // 기존의 Sprite 변수들은 이제 필요 없습니다! (Animator가 알아서 함)

    void Start()
    {
        if (tama != null)
        {
            tama.OnStatusChanged += UpdateVisual;
        }
    }

    void OnDestroy()
    {
        if (tama != null) tama.OnStatusChanged -= UpdateVisual;
    }

    void UpdateVisual()
    {
        if (animator == null) return;

        // Enum(상태)을 정수(int)로 바꿔서 애니메이터에게 전달
        // 예: Idle=0, Eating=1, Sleeping=2 ...
        animator.SetInteger("StateIndex", (int)tama.state);
    }
}