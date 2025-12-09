using UnityEngine;

namespace WBC.Engine
{
    [RequireComponent(typeof(Controller))]
    public class EngineCore : MonoBehaviour
    {
        protected int id { private set; get; }

        public virtual void Started()
        {
            Controller controller = GetComponent<Controller>();
            id = controller.Id(); //IDを各機能で使えるように保存しておく。
        }

         public virtual void Updated() { }
    }
}
