using System.Collections.Generic;
using UnityEngine;

namespace WBC
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private List<EngineCore> engines = new List<EngineCore>();

        private void Update()
        {
            if (engines == null || engines.Count == 0)
                return;

            foreach (EngineCore engine in engines)
            {
                engine.Scheduled();
            }
        }
    }
}
