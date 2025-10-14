using System.Collections.Generic;
using UnityEngine;
using WBC.Engine;

namespace WBC.Interaction
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerToCharacterInteraction : MonoBehaviour
    {
        private const int PLAYER_ID = -1;
        [SerializeField] private AudioSource _audioSource;
        private List<ConversationEngine> _conversationList = new List<ConversationEngine>();

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                //_conversationList.Add(engine);
                engine.SYN(PLAYER_ID);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<ConversationEngine>(out var engine))
            {
                //_conversationList.Remove(engine);
            }
        }
    }
}
