using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kellojo.ScopeShader.Demo {
    public class CameraMover : MonoBehaviour {

        public float Speed = 1f;
        public float LerpSpeed = 1f;
        public Vector3 Offset = Vector3.back;


        [SerializeField] Dropdown Dropdown;
        [SerializeField] List<ScopePreview> Scopes;
        ScopePreview CurrentPreview;

        private void Awake() {
            var options = new List<Dropdown.OptionData>();
            Scopes.ForEach(scope => options.Add(new Dropdown.OptionData(scope.name)));
            Dropdown.onValueChanged.AddListener(SwitchToScope);
            Dropdown.ClearOptions();
            Dropdown.AddOptions(options);

            SwitchToScope(0);
        }

        private void Update() {
            var offset = Mathf.PingPong(Time.time * Speed, CurrentPreview.OrbitWidth);
            var target = Offset + new Vector3(offset - CurrentPreview.OrbitWidth / 2, 0, 0) + CurrentPreview.position;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * LerpSpeed);
        }

        void SwitchToScope(int index) {
            CurrentPreview = Scopes[index];
        }


        [System.Serializable]
        protected class ScopePreview {
            public Transform Transform;
            public Vector3 Offset;
            public float OrbitWidth = 0.4f;

            public string name {
                get {
                    return Transform.name;
                }
            }

            public Vector3 position {
                get {
                    return Transform.position + Offset;
                }
            }
        }
    }

}
