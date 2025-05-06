using UnityEngine;

// This combines your existing FridgeVisual with the container system
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(FridgeContainer))]
public class FridgeVisualIntegration : MonoBehaviour, IInteractable
{
    [SerializeField] private FridgeContainer fridgeContainer;
    [SerializeField] private Animator animator;
    [SerializeField] private string animatorOpenParameter = "isOpen";

    private bool isOpen = false;

    private void Awake()
    {
        // Get required components
        if (animator == null)
            animator = GetComponent<Animator>();

        if (fridgeContainer == null)
            fridgeContainer = GetComponent<FridgeContainer>();
    }

    private void Start()
    {
        // Subscribe to container state change events
        if (fridgeContainer != null)
        {
            fridgeContainer.OnFridgeStateChanged += FridgeContainer_OnStateChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (fridgeContainer != null)
        {
            fridgeContainer.OnFridgeStateChanged -= FridgeContainer_OnStateChanged;
        }
    }

    // IInteractable implementation
    public void Interact(Player player)
    {
        ToggleDoor();
    }

    public string GetInteractText()
    {
        return isOpen ? "Close Fridge" : "Open Fridge";
    }

    // Control methods
    public void ToggleDoor()
    {
        if (fridgeContainer != null)
        {
            fridgeContainer.ToggleOpen();
        }
        else
        {
            // Fallback if container is not available
            isOpen = !isOpen;
            UpdateAnimator();
        }
    }

    private void FridgeContainer_OnStateChanged(object sender, System.EventArgs e)
    {
        isOpen = fridgeContainer.IsOpen();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool(animatorOpenParameter, isOpen);
        }
    }
}