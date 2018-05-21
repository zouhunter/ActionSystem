﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace WorldActionSystem
{
    public class ActionItem : MonoBehaviour, ISupportElement
    {
        [SerializeField]
        private bool startactive = true;
        [SerializeField]
        private bool endactive = true;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("m_name"), Attributes.DefultName]
        protected string _name;
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    return name;
                }
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public GameObject Body
        {
            get
            {
                return gameObject;
            }
        }
        public bool IsRuntimeCreated { get; set; }
        protected bool _active;
        public virtual bool Active { get { return _active; } protected set { _active = value; } }
        protected List<ActionObj> targets = new List<ActionObj>();

#if ActionSystem_G
        [HideInInspector]
#endif
        public UnityEvent onActive,onInActive;

        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void Start()
        {
            ElementController.Instence.RegistElement(this);
        }
        protected virtual void OnDestroy()
        {
            ElementController.Instence.RemoveElement(this);
        }
        protected virtual void Update() { }
        protected virtual void OnDisable() { }
        public virtual void SetVisible(bool visible)
        {
            Body.SetActive(visible);
        }

        public void StepActive()
        {
            Active = true;
            onActive.Invoke();
        }
        public void StepComplete()
        {
            Active = false;
            onInActive.Invoke();
        }

        public void StepUnDo()
        {
            Active = false;
            onInActive.Invoke();
        }
    }
}