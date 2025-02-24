using UnityEngine;

public class HitStateBehaviour : StateMachineBehaviour
{
    public System.Action OnStateExitAction; // Permet de lier une action externe
    public System.Action OnStateEnterAction; // Permet de lier une action externe

    private bool hasTriggered = false;

    // Appelé quand on quitte cet état
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Enter state");
        OnStateEnterAction?.Invoke();
    }

    // Appelé quand on quitte cet état
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Exit state");
        OnStateExitAction?.Invoke();
    }

}

