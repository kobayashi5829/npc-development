using System.Collections.Generic;
using UnityEngine;

namespace WBC.Engine
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private List<EngineCore> _engines = new List<EngineCore>();
        public bool interactionDistance = false;

        private void Update()
        {
            if (_engines == null || _engines.Count == 0)
                return;

            foreach (EngineCore engine in _engines)
            {
                engine.Scheduled();
            }
        }
    }
}
