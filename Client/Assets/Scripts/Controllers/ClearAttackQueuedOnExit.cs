using UnityEngine;

public class ClearAttackQueuedOnExit : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("애니메이션 종료 감지!");
        // 추가 동작 실행
    }
}
