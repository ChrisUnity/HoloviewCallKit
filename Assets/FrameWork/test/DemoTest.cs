using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFramework;
using UnityEngine;



namespace demo
{
    public class TestLogic : MonoBehaviour {
        FsmState<TestLogic>[] States = new FsmState<TestLogic>[] { new Demo_IdleState() };



    public void test()
    {
        Fsm<TestLogic> demoFsm= FrameworkEntry.Instance.GetManager<FsmManager>().CreateFsm<TestLogic>(this, "", States);
        demoFsm.Start<Demo_IdleState>();
        demoFsm.ChangeState<Demo_IdleState>();
    }
}

    public class Demo_IdleState : FsmState<TestLogic> { }




}
