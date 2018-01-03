﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{

    public class RotateCtrl : OperateController
    {
        public override ControllerType CtrlType { get { return ControllerType.Rotate; } }

        private RotObj selectedObj;
        private RaycastHit hit;
        private Ray ray;
        private Vector3 originalTargetPosition;
        private Vector3 axis;
        private Vector3 previousMousePosition;

        private float distence { get { return Config.hitDistence; } }

        public override void Update()
        {
            if (TrySelectRotateObj())
            {
                TransformSelected();
            }
        }
        private bool TrySelectRotateObj()
        {
            if (viewCamera == null) return false;

            ray = viewCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, distence, (1 << Layers.rotateItemLayer)))
            {
                selectedObj = hit.collider.GetComponent<RotObj>();
            }

            return selectedObj != null;
        }

      

        void TransformSelected()
        {
            if (Input.GetMouseButtonDown(0))
            {
                originalTargetPosition = selectedObj.transform.position;
                axis = selectedObj.Direction;
                previousMousePosition = Vector3.zero;
            }

            if (Input.GetMouseButton(0))
            {
                if (selectedObj.Started)
                {
                    ray = viewCamera.ScreenPointToRay(Input.mousePosition);
                    Vector3 mousePosition = GeometryUtil.LinePlaneIntersect(ray.origin, ray.direction, originalTargetPosition, axis);
                    if (previousMousePosition != Vector3.zero && mousePosition != Vector3.zero && IsInCercle(mousePosition))
                    {
                        var vec1 = previousMousePosition - selectedObj.transform.position;
                        var vec2 = mousePosition - selectedObj.transform.position;
                        float rotateAmount = (Vector3.Angle(Vector3.Cross(vec1, vec2), axis) < 180f ? 1 : -1)
                            * Vector3.Angle(vec1, vec2) * selectedObj.rotSpeed;
                        selectedObj.Rotate(rotateAmount);
                    }

                    previousMousePosition = mousePosition;
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                selectedObj.ClampAsync(()=> {
                    if (selectedObj.TryMarchRot()) {
                        selectedObj.OnEndExecute(false);
                    }
                });               
            }
        }
        private bool IsInCercle(Vector3 pos)
        {
            return Vector3.Distance(selectedObj.transform.position, pos) < selectedObj.triggerRadius;
        }

    }

}