using System.Collections.Generic;
using UnityEngine;

namespace WBC.Engine
{
    public class Controller : MonoBehaviour
    {
        [SerializeField] private List<EngineCore> _engines = new List<EngineCore>();
        [Header("Test")]
        [SerializeField] private int _id = 0;

        public int Id() { return _id; }

        private void Start()
        {
            if (_engines == null || _engines.Count == 0)
                return;

            foreach (EngineCore engine in _engines)
                engine.Started();
        }

        private void Update()
        {
            if (_engines == null || _engines.Count == 0)
                return;

            foreach (EngineCore engine in _engines)
                engine.Updated();
        }
    }
}
