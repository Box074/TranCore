using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TranCore
{
    public class TranAttach : MonoBehaviour
    {
        Dictionary<string, Action> actions = new Dictionary<string, Action>();
        List<(string, Func<bool>)> events = new List<(string, Func<bool>)>();
        public bool AutoDis { get; set; } = true;
        public int InvokeCount => invokeCount;
        int invokeCount = 0;
        public void RegisterAction(string name, Func<IEnumerator> c,params Func<bool>[] test)
        {
            actions[name] = new Action()
            {
                c = c,
                test = test
            };
        }
        public void StopAction(string name)
        {
            if(actions.TryGetValue(name,out var v))
            {
                foreach(var v2 in v.instances)
                {
                    StopCoroutine(v2.coroutine);
                }
                v.instances.Clear();
            }
        }
        public void StopAction(ActionInstance instance)
        {
            StopCoroutine(instance.coroutine);
            instance.action.instances.Remove(instance);
        }
        public void InvokeAction(string name)
        {
            if(actions.TryGetValue(name,out var v))
            {
                if (v.test != null && v.test.Length>0)
                {
                    if (!v.test.All(x => x())) return;
                }
                StartCoroutine(_invoke(v));
            }
        }
        public bool IsActionInvoking(string name)
        {
            if(actions.TryGetValue(name,out var v))
            {
                return v.invokeCount > 0;
            }
            return false;
        }
        public int ActionInvokeCount(string name)
        {
            if (actions.TryGetValue(name, out var v))
            {
                return v.invokeCount;
            }
            return 0;
        }
        public IEnumerator InvokeWait(string name)
        {
            if (actions.TryGetValue(name, out var v))
            {
                if (v.test != null && v.test.Length > 0)
                {
                    if (!v.test.All(x => x())) yield break;
                }
                yield return StartCoroutine(_invoke(v));
            }
        }
        public IEnumerator WaitAction(string name)
        {
            if (actions.TryGetValue(name, out var v))
            {
                while (v.invokeCount > 0) yield return null;
            }
        }
        IEnumerator _invoke(Action action)
        {
            action.invokeCount++;
            invokeCount++;
            try
            {
                Coroutine coroutine = StartCoroutine(action.c());
                ActionInstance instance = new ActionInstance(action, coroutine);
                action.instances.Add(instance);
                yield return coroutine;
                action.instances.Remove(instance);
            }
            finally
            {
                action.invokeCount--;
                invokeCount--;
            }
        }
        public void InvokeActionOn(string name, Func<bool> test)
        {
            if (test != null)
            {
                events.Add((name, test));
            }
        }
        void Awake()
        {
            gameObject.SetActive(false);
            if (GetComponent<ConstrainPosition>() != null) Destroy(GetComponent<ConstrainPosition>());
            try
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
            finally
            {
                foreach (var v in GetComponentsInChildren<Transform>()) v.gameObject.layer = (int)GlobalEnums.PhysLayers.HERO_BOX;
                gameObject.layer = (int)GlobalEnums.PhysLayers.PLAYER;
                foreach (var v in GetComponentsInChildren<DamageHero>()) Destroy(v);
                Destroy(GetComponent<HealthManager>());

                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            }
        }

        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (AutoDis)
            {
                gameObject.SetActive(false);
                return;
            }
        }

        void Update()
        {
            if (HeroController.instance == null)
            {
                gameObject.SetActive(false);
                return;
            }
            HeroController.instance.GetComponent<MeshRenderer>().enabled = false;
            HeroController.instance.hero_state = GlobalEnums.ActorStates.no_input;

            foreach(var v in events)
            {
                try
                {
                    if (v.Item2())
                    {
                        InvokeAction(v.Item1);
                    }
                }
                catch (Exception e)
                {
                    Modding.Logger.LogError(e);
                }
            }
        }

        void OnEnable()
        {
            gameObject.transform.position = HeroController.instance.transform.position;
        }
        void OnDisable()
        {
            if (HeroController.instance != null)
            {
                HeroController.instance.EnableRenderer();
                HeroController.instance.hero_state = GlobalEnums.ActorStates.idle;
            }
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
            DontDestroyOnLoad(gameObject);
            foreach (var v in actions)
            {
                v.Value.instances.Clear();
                v.Value.invokeCount = 0;
            }
            invokeCount = 0;
        }

        internal class Action
        {
            public string name = "";
            public Func<IEnumerator> c = null;
            public Func<bool>[] test = null;
            public int invokeCount = 0;
            public List<ActionInstance> instances = new List<ActionInstance>();
        }
        public class ActionInstance
        {
            internal ActionInstance(Action action,Coroutine coroutine)
            {
                this.action = action;
                this.coroutine = coroutine;
                guid = Guid.NewGuid().ToString();
            }
            internal Action action = null;
            public Coroutine coroutine = null;
            public readonly string guid = "";
            public override bool Equals(object obj)
            {
                if (obj == this) return true;
                if(obj is ActionInstance a)
                {
                    if (a.guid == guid) return true;
                    else return false;
                }
                else
                {
                    return false;
                }
            }
            public override string ToString()
            {
                return $"{action.name} - {guid}";
            }
            public override int GetHashCode()
            {
                return guid.GetHashCode();
            }
        }
    }
}
