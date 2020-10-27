
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace TDSBridge
{
    public class BridgeIOS : IBridge
    {
        private static BridgeIOS sInstance = new BridgeIOS();

        private Dictionary<string, Action<Result>> dic;

        private Action<Result> callback;

        public static BridgeIOS GetInstance()
        {
            return sInstance;
        }

        private BridgeIOS()
        {
            dic = new Dictionary<string, Action<Result>>();
        }

        public Dictionary<string, Action<Result>> GetDictionary()
        {
            return dic;
        }

        public Action<Result> GetCallback()
        {
            return callback;
        }

        private delegate void EngineBridgeDelegate(string result);
        [AOT.MonoPInvokeCallbackAttribute(typeof(EngineBridgeDelegate))]
        static void engineBridgeDelegate(string resultJson)
        {

            Result result = new Result(resultJson);

            Dictionary<string, Action<Result>> actionDic = BridgeIOS.GetInstance().GetDictionary();

            Action<Result> action = null;

            if (actionDic != null && actionDic.ContainsKey(result.callbackId))
            {
                action = actionDic[result.callbackId];
            }
            else
            {
                action = BridgeIOS.GetInstance().GetCallback();
            }
            if (action != null)
            {
                action(result);
            }
        }

        public void Register(string serviceClz, string serviceImp)
        {
            //IOS无需注册
        }

        public void Register(Action<Result> action)
        {
#if UNITY_IOS && !UNITY_EDITOR
            this.callback = action;
            registerCallback(engineBridgeDelegate);
#endif
        }

        public void Call(Command command)
        {
#if UNITY_IOS && !UNITY_EDITOR
            callHandler(command.toJSON());
#endif
        }

        public void Call(Command command, Action<Result> action)
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (command.callback && !string.IsNullOrEmpty(command.callbackId))
            {
                if (!dic.ContainsKey(command.callbackId))
                {
                    dic.Add(command.callbackId, action);
                }
            }
            registerHandler(command.toJSON(), engineBridgeDelegate);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void callHandler(string command);

    [DllImport("__Internal")]
    private static extern void registerCallback(EngineBridgeDelegate engineBridgeDelegate);

    [DllImport("__Internal")]
    private static extern void registerHandler(string command,EngineBridgeDelegate engineBridgeDelegate);
#endif

    }
}