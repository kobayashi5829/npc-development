using UnityEngine;
using WBC.Engine;

namespace WBC.Interaction
{
    public class Interaction : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Controller>(out var controller))
            {
                controller.interactionDistance = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Controller>(out var controller))
            {
                controller.interactionDistance = false;
            }
        }
    }
}
