using Fenrir.Multiplayer.Simulation.Exceptions;
using Fenrir.Multiplayer.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Fenrir.Multiplayer.Simulation
{
    /// <summary>
    /// Helper class that hashes component information and provides
    /// quality of life methods to invoke component RPC, update component state etc
    /// </summary>
    class ComponentTypeWrapper
    {
        // Server RPC hashes
        private Dictionary<ulong, ServerRpcMethodInfo> _hashToServerRpcMethodInfoDictionary = new Dictionary<ulong, ServerRpcMethodInfo>();
        private Dictionary<MethodInfo, ulong> _serverRpcMethodToHashDictionary = new Dictionary<MethodInfo, ulong>();

        // Client RPC hashes
        private Dictionary<ulong, ClientRpcMethodInfo> _hashToClientRpcMethodInfoDictionary = new Dictionary<ulong, ClientRpcMethodInfo>();
        private Dictionary<MethodInfo, ulong> _clientRpcMethodToHashDictionary = new Dictionary<MethodInfo, ulong>();

        // State fields
        private Dictionary<ulong, FieldInfo> _hashToStateFieldDictionary = new Dictionary<ulong, FieldInfo>();
        private Dictionary<FieldInfo, ulong> _stateFieldToHashDictionary = new Dictionary<FieldInfo, ulong>();

        // State properties
        private Dictionary<ulong, PropertyInfo> _hashToStatePropertyDictionary = new Dictionary<ulong, PropertyInfo>();
        private Dictionary<PropertyInfo, ulong> _statePropertyToHashDictionary = new Dictionary<PropertyInfo, ulong>();

        /// <summary>
        /// Component Type
        /// </summary>
        public Type ComponentType { get; private set; }

        /// <summary>
        /// Component Type Hash
        /// </summary>
        public ulong TypeHash { get; private set; }

        /// <summary>
        /// Creates new component type helper for a given component type
        /// </summary>
        /// <param name="componentType"></param>
        public ComponentTypeWrapper(Type componentType)
        {
            if(!typeof(SimulationComponent).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"Invalid component type: {componentType.Name}; does not derive from {typeof(SimulationComponent).Name}");
            }

            ComponentType = componentType;
            TypeHash = DeterministicHashUtility.CalculateHash(componentType.FullName);

            HashServerRpcMethods(componentType);
            HashClientRpcMethods(componentType);
            HashStateFields(componentType);
            HashStateProperties(componentType);
        }

        #region Registration
        private void HashServerRpcMethods(Type componentType)
        {
            MethodInfo[] methods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            foreach(MethodInfo methodInfo in methods)
            {
                ServerRpcAttribute serverRpcAttr = methodInfo.GetCustomAttribute<ServerRpcAttribute>();

                if (serverRpcAttr == null)
                {
                    return; // We don't need this method
                }

                if (methodInfo.IsGenericMethod)
                {
                    throw new InvalidOperationException($"{componentType.Name}.{methodInfo.Name} is generic. {typeof(ServerRpcAttribute).Name} is not supported for generic methods.");
                }

                ulong methodHash = CalculateMethodHash(methodInfo);

                // Get parameters
                ParameterInfo[] parameters = methodInfo.GetParameters();

                RpcParameterInfo[] rpcParameterInfo = new RpcParameterInfo[parameters.Length];
                
                for(int numParam=0; numParam < parameters.Length; numParam++)
                {
                    ParameterInfo parameter = parameters[numParam];

                    // TODO: Validate parameter type is serializable?
                    rpcParameterInfo[numParam] = new RpcParameterInfo(numParam, parameter.ParameterType);
                }

                // Get return type
                Type returnType = methodInfo.ReturnType; // TODO: Validate return type is serializable?

                // Build rpc method info
                ServerRpcMethodInfo rpcMethodInfo = new ServerRpcMethodInfo(methodInfo, rpcParameterInfo, methodHash, returnType);

                _hashToServerRpcMethodInfoDictionary.Add(methodHash, rpcMethodInfo);
                _serverRpcMethodToHashDictionary.Add(methodInfo, methodHash);

                // Add more sofisticated info such as reliable, unreliable etc
            }
        }


        private void HashClientRpcMethods(Type componentType)
        {
            MethodInfo[] methods = componentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            foreach (MethodInfo methodInfo in methods)
            {
                ClientRpcAttribute clientRpcAttr = methodInfo.GetCustomAttribute<ClientRpcAttribute>();

                if (clientRpcAttr == null)
                {
                    return; // We don't need this method
                }

                if (methodInfo.IsGenericMethod)
                {
                    throw new InvalidOperationException($"{componentType.Name}.{methodInfo.Name} is generic. RPC is not supported for generic methods.");
                }

                if(methodInfo.ReturnType != typeof(void))
                {
                    throw new InvalidOperationException($"{componentType.Name}.{methodInfo.Name} returns {methodInfo.ReturnType}. Client RPC is not supported methods with non-void return type.");
                }

                ulong methodHash = CalculateMethodHash(methodInfo);

                // Get parameters
                ParameterInfo[] parameters = methodInfo.GetParameters();

                RpcParameterInfo[] rpcParameterInfo = new RpcParameterInfo[parameters.Length];

                for (int numParam = 0; numParam < parameters.Length; numParam++)
                {
                    ParameterInfo parameter = parameters[numParam];

                    // TODO: Validate parameter type is serializable?
                    rpcParameterInfo[numParam] = new RpcParameterInfo(numParam, parameter.ParameterType);
                }

                // Get return type
                Type returnType = methodInfo.ReturnType; // TODO: Validate return type is serializable

                // Build rpc method info
                ClientRpcMethodInfo rpcMethodInfo = new ClientRpcMethodInfo(methodInfo, rpcParameterInfo, methodHash);

                _hashToClientRpcMethodInfoDictionary.Add(methodHash, rpcMethodInfo);
                _clientRpcMethodToHashDictionary.Add(methodInfo, methodHash);

                // Add more sofisticated info such as reliable, unreliable etc
            }
        }

        private void HashStateFields(Type componentType)
        {
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo fieldInfo in fields)
            {
                StateVarAttribute stateVarAttr = fieldInfo.GetCustomAttribute<StateVarAttribute>();

                if (stateVarAttr == null)
                {
                    return; // We don't need this field
                }

                ulong fieldHash = CalculateFieldHash(fieldInfo);

                _hashToStateFieldDictionary.Add(fieldHash, fieldInfo);
                _stateFieldToHashDictionary.Add(fieldInfo, fieldHash);

                // Add more sofisticated info such as reliable, unreliable sync etc
            }
        }


        private void HashStateProperties(Type componentType)
        {
            PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            foreach (PropertyInfo propertyInfo in properties)
            {
                StateVarAttribute stateVarAttr = propertyInfo.GetCustomAttribute<StateVarAttribute>();

                if (stateVarAttr == null)
                {
                    return; // We don't need this field
                }

                ulong propertyHash = CalculatePropertyHash(propertyInfo);

                _hashToStatePropertyDictionary.Add(propertyHash, propertyInfo);
                _statePropertyToHashDictionary.Add(propertyInfo, propertyHash);

                // Add more sofisticated info such as reliable, unreliable sync etc
            }
        }

        public ulong GetClientRpcMethodHash(MethodInfo methodInfo)
        {
            if(!_clientRpcMethodToHashDictionary.TryGetValue(methodInfo, out ulong methodHash))
            {
                throw new ArgumentException($"RPC method {ComponentType.Name}.{methodInfo.Name} hash is not found. Did you forget to register component, or mark method with {typeof(ClientRpcAttribute).Name}?");
            }

            return methodHash;
        }

        public ulong GetServerRpcMethodHash(MethodInfo methodInfo)
        {
            if (!_serverRpcMethodToHashDictionary.TryGetValue(methodInfo, out ulong methodHash))
            {
                throw new ArgumentException($"RPC method {ComponentType.Name}.{methodInfo.Name} hash is not found. Did you forget to register component, or mark method with {typeof(ServerRpcAttribute).Name}?");
            }

            return methodHash;
        }


        internal ulong CalculateMethodHash(MethodInfo methodInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(methodInfo.Name);

            // Append return type
            sb.Append(methodInfo.ReturnType.FullName);

            // Append parameter types
            ParameterInfo[] methodParameters = methodInfo.GetParameters();
            foreach (ParameterInfo parameterInfo in methodParameters)
            {
                sb.Append(parameterInfo.ParameterType.FullName);
            }

            string uniqueName = sb.ToString();

            return DeterministicHashUtility.CalculateHash(uniqueName);
        }

        private ulong CalculateFieldHash(FieldInfo fieldInfo)
        {
            return DeterministicHashUtility.CalculateHash(fieldInfo.Name);
        }

        private ulong CalculatePropertyHash(PropertyInfo propertyInfo)
        {
            return DeterministicHashUtility.CalculateHash(propertyInfo.Name);
        }
        #endregion

        #region Rpc
        public bool TryInvokeClientRpc(object component, ulong methodHash, object[] parameters)
        {
            if(!_hashToClientRpcMethodInfoDictionary.TryGetValue(methodHash, out ClientRpcMethodInfo rpcMethodInfo))
            {
                return false;
            }

            // Validate parameter types
            for (int numParam=0; numParam < rpcMethodInfo.Parameters.Length; numParam++)
            {
                RpcParameterInfo parameterInfo = rpcMethodInfo.Parameters[numParam];
                object parameter = parameters[0];

                Type parameterType = parameter.GetType();

                if (!parameterInfo.ParameterType.IsAssignableFrom(parameterType))
                {
                    return false; // Incorrect parameter type
                }
            }

            MethodInfo methodInfo = rpcMethodInfo.MethodInfo;

            // Invoke method
            methodInfo.Invoke(component, parameters);

            return true;
        }

        internal ServerRpcMethodInfo GetServerRpcMethodInfo(ulong methodHash)
        {
            if(!_hashToServerRpcMethodInfoDictionary.TryGetValue(methodHash, out ServerRpcMethodInfo methodInfo))
            {
                throw new SimulationException("Unknown server rpc method hash: " + methodHash);
            }

            return methodInfo;
        }

        internal ClientRpcMethodInfo GetClientRpcMethodInfo(ulong methodHash)
        {
            if (!_hashToClientRpcMethodInfoDictionary.TryGetValue(methodHash, out ClientRpcMethodInfo methodInfo))
            {
                throw new SimulationException("Unknown client rpc method hash: " + methodHash);
            }

            return methodInfo;
        }
        #endregion

        internal struct RpcParameterInfo
        {
            public int Index;

            public Type ParameterType;

            public RpcParameterInfo(int index, Type parameterType)
            {
                Index = index;
                ParameterType = parameterType;
            }
        }

        internal struct ClientRpcMethodInfo
        {
            public MethodInfo MethodInfo;

            public RpcParameterInfo[] Parameters;

            public ulong MethodHash;

            public ClientRpcMethodInfo(MethodInfo methodInfo, RpcParameterInfo[] parameters, ulong methodHash)
            {
                MethodInfo = methodInfo;
                Parameters = parameters;
                MethodHash = methodHash;
            }
        }

        internal struct ServerRpcMethodInfo
        {
            public MethodInfo MethodInfo;

            public RpcParameterInfo[] Parameters;

            public ulong MethodHash;

            public Type ReturnType;

            public ServerRpcMethodInfo(MethodInfo methodInfo, RpcParameterInfo[] parameters, ulong methodHash, Type returnType)
            {
                MethodInfo = methodInfo;
                Parameters = parameters;
                MethodHash = methodHash;
                ReturnType = returnType;
            }
        }
    }
}
