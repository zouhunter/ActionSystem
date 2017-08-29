﻿using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public class DragAnimController 
    {
        private DragTarget endParent;
        private DragStart startParent;
        private DragShow animParent;
        IHighLightItems HighLight;

        public DragAnimController(DragStart startParent, DragTarget endParent, DragShow animParent)
        {
            this.startParent = startParent;
            this.endParent = endParent;
            this.animParent = animParent;
            HighLight = new ShaderHighLight();

            startParent.onInstallOk = PlayAnim;
        }


        private DragObj pickedUpObj;
        private bool pickedUp;
        private float distence { get { return startParent.Distence; } set { startParent.Distence = value; } }
        private DragPos installPos;

        public const string elementLayer = "dragStart";
        public const string elementInstallLayer = "dragEnd";
        private Ray ray;
        private RaycastHit hit;
        private RaycastHit[] hits;
        private bool installAble;
        private string resonwhy;
        private string currStepName;
        public event UserError InstallErr;

        private Ray disRay;
        private RaycastHit disHit;
        private string obstacle = "Obstacle";

        public void SwitchHighLight(bool open)
        {
            if (open) HighLight = new ShaderHighLight();
            else HighLight = null;
        }

        #region 鼠标操作事件
        public void Reflesh()
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnLeftMouseClicked();
            }
            else if (pickedUp)
            {
                UpdateInstallState();
                MoveWithMouse(distence += Input.GetAxis("Mouse ScrollWheel"));
            }

        }

        public void OnLeftMouseClicked()
        {
            if (!pickedUp)
            {
                SelectAnElement();
            }
            else
            {
                TryInstallObject();
            }
        }

        /// <summary>
        /// 在未屏幕锁的情况下选中一个没有元素
        /// </summary>
        void SelectAnElement()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask(elementLayer)))
            {
                pickedUpObj = hit.collider.GetComponent<DragObj>();
                if (pickedUpObj != null && startParent.PickUpObject(pickedUpObj))
                {
                    pickedUp = true;

                    if (!PickUpedCanInstall())
                    {
                        if (HighLight != null) HighLight.HighLightTarget(pickedUpObj.Render, Color.yellow);
                    }
                    else
                    {
                        if (HighLight != null) HighLight.HighLightTarget(pickedUpObj.Render, Color.cyan);
                    }
                }
            }
        }

        private bool PickUpedCanInstall()
        {
            bool canInstall = false;
            List<DragPos> poss = endParent.GetNotInstalledPosList();
            for (int i = 0; i < poss.Count; i++)
            {
                if (!endParent.HaveInstallPosInstalled(poss[i]) && endParent.IsInstallStep(poss[i]) && startParent.CanInstallToPos(poss[i]))
                {
                    canInstall = true;
                }
            }
            return canInstall;
        }


        public void UpdateInstallState()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hits = Physics.RaycastAll(ray, 100, LayerMask.GetMask(elementInstallLayer));
            if (hits != null || hits.Length > 0)
            {
                bool hited = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.name == pickedUpObj.name)
                    {
                        hited = true;
                        installPos = hits[i].collider.GetComponent<DragPos>();
                        if (installPos == null)
                        {
                            Debug.LogError("零件未挂InstallPos脚本");
                        }
                        else if (!endParent.IsInstallStep(installPos))
                        {
                            installAble = false;
                            resonwhy = "当前安装步骤并非" + installPos.stapName;
                        }
                        else if (endParent.HaveInstallPosInstalled(installPos))
                        {
                            installAble = false;
                            resonwhy = "安装点已经安装了其他零件";
                        }
                        else if (!startParent.CanInstallToPos(installPos))
                        {
                            installAble = false;
                            resonwhy = "拿起零件和安装点不对应";
                        }
                        else
                        {
                            installAble = true;
                        }
                    }
                }
                if (!hited)
                {
                    installAble = false;
                    resonwhy = "不要乱放零件";
                }
            }

            if (installAble)
            {
                //可安装显示绿色
                if (HighLight != null) HighLight.HighLightTarget(pickedUpObj.Render, Color.green);
            }
            else
            {
                //不可安装红色
                if (HighLight != null) HighLight.HighLightTarget(pickedUpObj.Render, Color.red);
            }
        }



        /// <summary>
        /// 尝试安装元素
        /// </summary>
        void TryInstallObject()
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (installAble)
            {
                startParent.InstallPickedUpObject(installPos);
            }
            else
            {
                OnInstallErr(resonwhy);
                startParent.PickDownPickedUpObject();
            }

            pickedUp = false;
            installAble = false;
            if (HighLight != null) HighLight.UnHighLightTarget(pickedUpObj.Render);
        }

        /// <summary>
        /// 跟随鼠标
        /// </summary>
        void MoveWithMouse(float dis)
        {
            disRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(disRay, out disHit, dis, LayerMask.GetMask(obstacle)))
            {
                pickedUpObj.transform.position = disHit.point;
            }
            else
            {
                pickedUpObj.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dis));
            }
        }
        #endregion

        /// <summary>
        /// 结束当前步骤安装
        /// </summary>
        /// <param name="stapName"></param>
        public void EndInstall(string stapName)
        {
            SetStapActive(stapName);
            List<DragPos> installed = endParent.GetInstalledPosList();
            startParent.QuickUnInstallPosListObjects(installed);
            List<DragPos> posList = endParent.GetNotInstalledPosList();
            startParent.QuickInstallPosListObjects(posList);

        }

        public bool CurrStapComplete()
        {
            return endParent.AllElementInstalled();
        }

        public void SetStapActive(string stapName)
        {
            currStepName = stapName;
            endParent.SetStapActive(stapName);
        }

        /// <summary>
        /// 自动安装部分需要进行自动安装的零件
        /// </summary>
        /// <param name="stapName"></param>
        public void AutoInstallWhenNeed(string stapName)
        {
            List<DragPos> posList = endParent.GetNeedAutoInstallPosList();

            startParent.InstallPosListObjects(posList);

            pickedUp = false;
        }

        public void UnInstall(string stapName)
        {
            SetStapActive(stapName);
            List<DragPos> posList = endParent.GetInstalledPosList();
            startParent.UnInstallPosListObjects(posList);
        }

        public void QuickUnInstall(string stapName)
        {
            SetStapActive(stapName);
            List<DragPos> posList = endParent.GetInstalledPosList();
            startParent.QuickUnInstallPosListObjects(posList);
        }

        private void OnInstallErr(string err)
        {
            if (InstallErr != null)
            {
                InstallErr.Invoke(currStepName, err);
            }
        }
        private void PlayAnim()
        {
            if (CurrStapComplete())
            {
                List<DragPos> posList = endParent.GetInstalledPosList();
                startParent.TryHidePosListObjects(posList);
                animParent.PlayAnim(currStepName);
            }
        }
        public void UnDoAnim(string currStepName)
        {
            animParent.UnDoAnim(currStepName);
        }
        public void EndPlayAnim(string currStepName)
        {
            animParent.EndPlayAnim(currStepName);
        }
    }
}
