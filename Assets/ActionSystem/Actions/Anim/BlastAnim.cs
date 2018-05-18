﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class BlastAnim : CodeAnimItem
    {
        [SerializeField]
        protected float targetRange = 1;
        [SerializeField]
        protected Transform center;
        [SerializeField]
        protected Transform[] blastItems;
        [SerializeField, Range(-10, 10)]
        protected float rotateSpeed;

        protected Vector3[] startPositions;
        protected Vector3[] targetPositions;
        protected Quaternion[] startRotations;
       
        protected override void InitState()
        {
            var length = blastItems.Length;
            startPositions = new Vector3[length];
            targetPositions = new Vector3[length];
            startRotations = new Quaternion[length];

            for (int i = 0; i < length; i++)
            {
                startPositions[i] = blastItems[i].transform.position;
                targetPositions[i] = blastItems[i].transform.position + (blastItems[i].transform.position - center.transform.position).normalized * targetRange;
                startRotations[i] = blastItems[i].transform.rotation;
            }
        }

        public override void SetVisible(bool visible)
        {
            foreach (var item in blastItems)
            {
                item.gameObject.SetActive(visible);
            }
        }

        public override void StepUnDo()
        {
            base.StepUnDo();
            for (int i = 0; i < blastItems.Length; i++)
            {
                var item = blastItems[i];
                item.localPosition = startPositions[i];
            }
        }

        public override void StepComplete()
        {
            base.StepComplete();
            for (int i = 0; i < blastItems.Length; i++)
            {
                var item = blastItems[i];
                item.localPosition = targetPositions[i];
            }
        }

        protected override IEnumerator PlayAnim(UnityAction onComplete)
        {
           
            Vector3[] startPos = from ? targetPositions : startPositions;
            Vector3[] targetPos = from ? startPositions : targetPositions;
            Quaternion[] targetRot = startRotations;

            Vector3[] dir = from ? GetDirs(targetPositions, startPositions) : GetDirs(startPositions, targetPositions);
            Quaternion[] rot = GetRots(dir, rotateSpeed);

            for (float i = 0; i < time; i += Time.deltaTime)
            {
                for (int j = 0; j < blastItems.Length; j++)
                {
                    var item = blastItems[j];
                    item.localPosition = Vector3.Lerp(startPos[j], targetPos[j], i / time);
                    item.localRotation = rot[j] * item.localRotation;
                }

               
                yield return null;
            }

            for (int i = 0; i < blastItems.Length; i++)
            {
                var item = blastItems[i];
                item.localRotation = targetRot[i];
            }

            if (onComplete != null)
            {
                onComplete.Invoke();
                onComplete = null;
            }
        }

        private static Vector3[] GetDirs(Vector3[] start,Vector3[] target)
        {
            var dirs = new Vector3[start.Length];
            for (int i = 0; i < start.Length; i++)
            {
                dirs[i] = target[i] - start[i];
            }
            return dirs;
        }

        private static Quaternion[] GetRots(Vector3[] dirs,float rotateSpeed)
        {
            var rots = new Quaternion[dirs.Length];
            for (int i = 0; i < rots.Length; i++)
            {
                rots[i] = Quaternion.AngleAxis(rotateSpeed, dirs[i]); 
            }
            return rots;
        }
    }
}
