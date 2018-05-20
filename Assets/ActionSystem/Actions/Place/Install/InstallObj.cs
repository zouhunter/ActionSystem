﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Internal;

namespace WorldActionSystem
{
    /// <summary>
    /// 模拟安装坐标功能
    /// </summary>
    [AddComponentMenu(MenuName.InstallObj)]
    public class InstallObj : PlaceObj
    {
        public override ControllerType CtrlType
        {
            get
            {
                return ControllerType.Place;
            }
        }
        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);

            if (!AlreadyPlaced)
            {
                PlaceElement obj = GetUnInstalledObj(Name);
                Attach(obj);
                obj.QuickInstall(this, true);
                obj.StepComplete();
            }
        }

        /// <summary>
        /// 找出一个没有安装的元素
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public PlaceElement GetUnInstalledObj(string elementName)
        {
            var elements = elementCtrl.GetElements<PlaceElement>(elementName);
            if (elements != null)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    if (!elements[i].HaveBinding)
                    {
                        return elements[i];
                    }
                }
            }
            throw new Exception("配制错误,缺少" + elementName);
        }

        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();

            if (AlreadyPlaced)
            {
                var detachedObj = Detach();
                detachedObj.QuickUnInstall();
                detachedObj.StepUnDo();
            }
        }

        protected override void OnInstallComplete()
        {
            if (!Complete)
            {
                OnEndExecute(false);
            }
        }

        protected override void OnUnInstallComplete()
        {
            if (Started)
            {
                if (AlreadyPlaced)
                {
                    var obj = Detach();
                    obj.PickUpAble = true;
                }
                this.obj = null;
            }
        }

        protected override void OnAutoInstall()
        {
            PlaceElement obj = GetUnInstalledObj(Name);
            Attach(obj);
            obj.StepActive();
            if (Config.quickMoveElement && !ignorePass)
            {
                obj.QuickInstall(this, true);
            }
            else
            {
                obj.NormalInstall(this, true);
            }
        }

        public override void PlaceObject(PlaceElement pickup)
        {
            Attach(pickup);
            pickup.QuickInstall(this, true);
            pickup.PickUpAble = false;
        }

        public override bool CanPlace(PickUpAbleItem element, out string why)
        {
            why = null;
            var canplace = true;
            if (!this.Started)
            {
                canplace = false;
                why = "操作顺序错误";
            }
            else if (this.AlreadyPlaced)
            {
                canplace = false;
                why = "已经安装";
            }

            else if (element.Name != this.Name)
            {
                canplace = false;
                why = "零件不匹配";
            }
            else
            {
                canplace = true;
            }
            return canplace;
        }

    }
}