using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Grabber : MonoBehaviour
{

    public InputActionReference grabAction;
    public InputActionReference triggerAction;
    public InputActionReference primaryAction;
    public InputActionReference secondaryAction;

    [SerializeField]
    private Transform grabTransform;

    public Transform GrabTransform { 
        get {
            if (grabTransform == null)
            {
                return transform;
            }
            else
            {
                return grabTransform;
            }
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
