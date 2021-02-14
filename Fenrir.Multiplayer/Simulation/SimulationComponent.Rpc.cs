namespace Fenrir.Multiplayer.Simulation
{
    public abstract partial class SimulationComponent
    {
        // RPC Delegates
        public delegate void RpcMethod();

        public delegate void RpcMethod<T1>(T1 t1);

        public delegate void RpcMethod<T1, T2>(T1 t1, T2 t2);

        public delegate void RpcMethod<T1, T2, T3>(T1 t1, T2 t2, T3 t3);

        public delegate void RpcMethod<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);

        public delegate void RpcMethod<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

        public delegate void RpcMethod<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

        public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);

        public delegate void RpcMethod<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);

        // Invoke RPC
        public void InvokeClientRpc(RpcMethod method)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method));
        }

        public void InvokeClientRpc<T1>(RpcMethod method, T1 t1)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1);
        }

        public void InvokeClientRpc<T1, T2>(RpcMethod method, T1 t1, T2 t2)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2);
        }

        public void InvokeClientRpc<T1, T2, T3>(RpcMethod method, T1 t1, T2 t2, T3 t3)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3);
        }

        public void InvokeClientRpc<T1, T2, T3, T4>(RpcMethod method, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3, t4);
        }

        public void InvokeClientRpc<T1, T2, T3, T4, T5>(RpcMethod method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3, t4, t5);
        }

        public void InvokeClientRpc<T1, T2, T3, T4, T5, T6>(RpcMethod method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3, t4, t5, t6);
        }

        public void InvokeClientRpc<T1, T2, T3, T4, T5, T6, T7>(RpcMethod method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3, t4, t5, t6, t7);
        }

        public void InvokeClientRpc<T1, T2, T3, T4, T5, T6, T7, T8>(RpcMethod method, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
        {
            Simulation.InvokeClientRpc(this, TypeWrapper.GetClientRpcMethodHash(method.Method), t1, t2, t3, t4, t5, t6, t7, t8);
        }
    }
}
