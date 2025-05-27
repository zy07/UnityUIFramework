using System.Collections;
using UnityEngine;

namespace CommonUnity
{
    public class MyWaitForSeconds
    {
        public float seconds;

        public MyWaitForSeconds(float seconds)
        {
            this.seconds = seconds;
        }
    }
    
    public class CustomTask
    {
        private IEnumerator _enumerator;
        private Coroutine _coroutine;
        private bool _isRunning;
        public bool IsRunning => _isRunning;
        private bool _isPause;
        public int Id;

        public CustomTask(int id, IEnumerator enumerator)
        {
            this.Id = id;
            this._enumerator = enumerator;
            _isPause = false;
            _isRunning = false;
        }

        public void Start()
        {
            _isRunning = true;
        }

        public void Pause()
        {
            _isPause = true;
        }

        public void Resume()
        {
            _isPause = false;
        }

        public void Stop()
        {
            _isRunning = false;
            _isPause = false;
            Release();
        }

        public void Release()
        {
            _enumerator = null;
            _coroutine = null;
        }

        public bool CheckIsOver()
        {
            return _enumerator == null;
        }

        public IEnumerator CallWrapper()
        {
            while (_isRunning)
            {
                if (_isPause) yield return null;

                if (_enumerator != null && _enumerator.MoveNext())
                {
                    yield return _enumerator.Current;
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}