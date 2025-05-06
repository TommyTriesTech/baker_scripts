using UnityEngine;

public class FridgeVisual : MonoBehaviour
{
    private Animator animator;
    private const string IS_OPEN = "isOpen";
    private bool isOpen = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        animator.SetBool(IS_OPEN, isOpen);
    }
}