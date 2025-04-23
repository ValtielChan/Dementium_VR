using UnityEngine;

public class BodyContinuousFollow : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float minHeight = 1f;
    [SerializeField] private float maxHeight = 2.2f;

    private void LateUpdate()
    {
        if (!headTransform || !characterController) return;

        //‑‑ hauteur dynamique de la capsule
        float localHeadY = transform.InverseTransformPoint(headTransform.position).y;
        float height      = Mathf.Clamp(localHeadY, minHeight, maxHeight);
        characterController.height  = height;
        characterController.center  = new Vector3(0f, height * 0.5f, 0f);

        //‑‑ position horizontale : le corps reste exactement sous la tête
        Vector3 targetPos = new Vector3(headTransform.position.x,
                                        transform.position.y,
                                        headTransform.position.z);

        // on désactive brièvement le CC pour éviter la réduction de déplacement
        bool wasEnabled = characterController.enabled;
        characterController.enabled = false;
        transform.position = targetPos;
        characterController.enabled = wasEnabled;
    }
}
