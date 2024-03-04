using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace SpreadSheetImporter
{
    public class UnityWebRequestAwaiter : INotifyCompletion
    {
        public bool IsCompleted => _asyncOp.isDone;
        
        private readonly UnityWebRequestAsyncOperation _asyncOp;
        private Action _continuation;

        public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
        {
            _asyncOp = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }
        
        public void GetResult() { }

        public void OnCompleted(Action continuation)
        {
            
            _continuation = continuation;
        }

        private void OnRequestCompleted(AsyncOperation obj)
        {
            _continuation();
        }
    }

    public static class UnityWebRequestAwaiterExtension
    {
        public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            return new UnityWebRequestAwaiter(asyncOp);
        }
    }

}