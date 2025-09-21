using UnityEngine;
using System.Collections.Generic;

namespace NexgenDragon
{
    public abstract class NexgenBehaviour : MonoBehaviour, IObject
    {
        protected object _data;
        public object dataFeed => _data;
        
        public virtual void FeedData(object data)
        {
            if (_data != null && _data != data)
            {
                OnReleaseData(_data);
            }
            _data = data;
            OnFeedData(data);
        }

        protected virtual void OnFeedData(object data)
        {
        }

        protected virtual void OnReleaseData(object data)
        {
        }

        public virtual void Release()
        {
            if (_data != null)
            {
                OnReleaseData(_data);
                _data = null;
            }
        }

        protected virtual void Awake()
        {
       
        }
        //Todo:去掉Update所有都用tick统一管理，但是已经有不少页面不规范的用了update，后续需要改为tick
        public virtual void Update()
        {
        }
        public virtual void Tick(float deltaTime)
        {
        }
        public virtual void LateUpdate()
        {
        }

        public virtual void FixedUpdate()
        {
        }
    }
}