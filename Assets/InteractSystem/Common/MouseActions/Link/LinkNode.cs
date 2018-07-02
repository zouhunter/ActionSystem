﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InteractSystem.Actions
{
    /// <summary>
    /// 指定个数的元素
    /// 按指定的方式完成连接
    /// </summary>
    [NodeGraph.CustomNode("Operate/Link", 17, "InteractSystem")]
    public class LinkNode : Graph.OperaterNode
    {
        [SerializeField]
        private bool quickLink;
        [SerializeField]
        private float autoLinkTime = 1f;
        [SerializeField]
        public List<LinkGroup> defultLink;
        protected ElementController elementCtrl { get { return ElementController.Instence; } }

        [SerializeField]
        protected CollectNodeFeature completeableFeature = new CollectNodeFeature(typeof(LinkItem));

        protected override List<OperateNodeFeature> RegistFeatures()
        {
            var features = base.RegistFeatures();

            completeableFeature.SetTarget(this);
            completeableFeature.onAddToPool = OnAddedToPool;
            completeableFeature.onRemoveFromPool = OnRemovedFromPool;

            features.Add(completeableFeature);
            return features;
        }
        protected void OnAddedToPool(ISupportElement arg0)
        {
            Debug.Log(arg0);
            (arg0 as LinkItem).RegistOnConnected(TryComplete);
        }
        protected void OnRemovedFromPool(ISupportElement arg0)
        {
            (arg0 as LinkItem).RemoveOnConnected(TryComplete);
        }

        public override void OnStartExecute(bool auto = false)
        {
            base.OnStartExecute(auto);
            OnStepActive();
            if (auto)
            {
                CoroutineController.Instence.StartCoroutine(AutoLinkItems());
            }
        }

        protected override void OnBeforeEnd(bool force)
        {
            base.OnBeforeEnd(force);

            CoroutineController.Instence.StopCoroutine(AutoLinkItems());

            if (completeableFeature.finalGroup == null)
            {
                QuickLinkItems();
            }
        }

        public override void OnUnDoExecute()
        {
            base.OnUnDoExecute();

            CoroutineController.Instence.StopCoroutine(AutoLinkItems());

            if (completeableFeature.finalGroup != null)
            {
                Array.ForEach(completeableFeature.finalGroup, linkItem =>
                {
                    //解除本脚本对linkItem的锁定
                    elementCtrl.UnLockElement(linkItem, this);
                });
                completeableFeature.finalGroup = null;
            }

        }
        /// <summary>
        /// 提示连接元素
        /// </summary>
        private void ActiveOneLinkItem()
        {
            //安装操作时动态提示
        }

        /// <summary>
        /// 获取需要数量的linkItem
        /// </summary>
        private void OnStepActive()
        {
            completeableFeature.elementPool.ForEach(linkItem =>
            {
                linkItem.StepActive();
                (linkItem as LinkItem).RegistOnConnected(TryComplete);
            });
        }

        /// <summary>
        /// 找到满足条件的连接体,
        /// 并将其锁定
        /// </summary>
        private void TryComplete()
        {
            if (log) Debug.Log("TryComplete");
            //所有可能的元素组合
            var combinations = CreateCombinations();
            var count = completeableFeature.itemList.Count - 1;//连接数
            //对每一个组合进行判断
            foreach (var combination in combinations)
            {
                //bool combinationOK = true;
                var counter = 0;
                //记录每个点可能的LinkItem
                foreach (var group in defultLink)
                {
                    var itemA = combination[group.ItemA];
                    var itemB = combination[group.ItemB];
                    var portA = itemA.ChildNodes.Find(x => x.NodeID == group.portA);
                    var portB = itemB.ChildNodes.Find(x => x.NodeID == group.portB);

                    var connected =
                        portA != null &&
                        portB != null &&
                        portA.ConnectedNode == portB
                        && portB.ConnectedNode == portA;

                    if (connected)
                    {
                        counter++;
                    }
                }
                ///连接数足够就当作已经连接
                if (counter >= count)
                {
                    OnCombinationOK(combination);
                    OnEndExecute(false);
                    return;
                }
            }
            //高亮提示下一个安装
            ActiveOneLinkItem();
        }

        /// <summary>
        /// 完成装配
        /// </summary>
        /// <param name="combination"></param>
        private void OnCombinationOK(List<LinkItem> combination)
        {
            //Debug.Log("OnCombinationOK");
            completeableFeature.finalGroup = combination.ToArray();
            foreach (var item in combination)
            {
                elementCtrl.LockElement(item, this);
            }
        }
        /// <summary>
        /// 计算出所有可能的组合
        /// </summary>
        /// <returns></returns>
        private List<List<LinkItem>> CreateCombinations()
        {
            var result = new List<List<LinkItem>>();
            var dic = new Dictionary<int, List<LinkItem>>();
            for (int i = 0; i < completeableFeature.itemList.Count; i++)
            {
                var id = i;
                var items = completeableFeature.elementPool.FindAll(x => x.Name == completeableFeature.itemList[i]).Select(x => x as LinkItem).ToList();
                if (items != null)
                {
                    dic.Add(id, items);
                }
                else
                {
                    Debug.LogError("缺少：" + completeableFeature.itemList[i]);
                    return null;
                }
            }

            foreach (var listTemp in dic)
            {
                var resultTemp = result.ToArray();
                result.Clear();

                for (int i = 0; i < listTemp.Value.Count; i++)
                {
                    if (resultTemp.Length == 0)
                    {
                        result.Add(new List<LinkItem> { listTemp.Value[i] });
                    }
                    else
                    {
                        for (int j = 0; j < resultTemp.Length; j++)
                        {
                            var list = new List<LinkItem>();

                            if (!resultTemp[j].Contains(listTemp.Value[i]))
                            {
                                list.AddRange(resultTemp[j]);
                                list.Add(listTemp.Value[i]);
                                result.Add(list);
                            }
                            else
                            {
                                //不需要重复的元素
                                continue;
                            }
                        }
                    }
                }
            }
            result.RemoveAll(x => x.Count < completeableFeature.itemList.Count);
            return result;
        }

        /// <summary>
        /// 快速连接元素
        /// </summary>
        private void QuickLinkItems()
        {
            Debug.Log("QuickLinkItems");
            var links = SurchLinkItems();
            for (int i = 0; i < defultLink.Count; i++)
            {
                var linkGroup = defultLink[i];

                var itemA = links[linkGroup.ItemA];
                var itemB = links[linkGroup.ItemB];

                var portA = itemA.ChildNodes[linkGroup.portA];
                var portB = itemB.ChildNodes[linkGroup.portB];

                if (portA.ConnectedNode != null || portB.ConnectedNode != null)
                {
                    continue;
                }

                //anglePos = portA.transform;
                LinkUtil.AttachNodes(portB, portA);
                portB.ResetTransform();
            }
        }

        /// <summary>
        /// 自动连接
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoLinkItems()
        {
            var links = SurchLinkItems();
            for (int i = 0; i < defultLink.Count; i++)
            {
                var linkGroup = defultLink[i];

                var itemA = links[linkGroup.ItemA];
                var itemB = links[linkGroup.ItemB];

                var portA = itemA.ChildNodes[linkGroup.portA];
                var portB = itemB.ChildNodes[linkGroup.portB];

                if (portA.ConnectedNode != null || portB.ConnectedNode != null)
                {
                    continue;
                }

                //anglePos = portA.transform;
                yield return MoveBToA(portA, portB);
                LinkUtil.AttachNodes(portB, portA);
            }
            TryComplete();
        }

        private LinkItem[] SurchLinkItems()
        {
            List<LinkItem> linkItemGroup = new List<LinkItem>();
            foreach (var itemName in completeableFeature.itemList)
            {
                var item = completeableFeature.elementPool.Find(x => x.Name == itemName && !linkItemGroup.Contains(x as LinkItem) && (x as LinkItem).CanUse);
                Debug.Assert(item != null, "缺少：" + itemName);
                linkItemGroup.Add(item as LinkItem);
            }
            return linkItemGroup.ToArray();
        }

        private IEnumerator MoveBToA(LinkPort portA, LinkPort portB)
        {
            //var linkInfoA = portA.connectAble.Find(x => x.itemName == portB.Body.Name);
            var linkInfoB = portB.connectAble.Find(x => x.itemName == portA.Body.Name);

            Vector3 pos;
            Vector3 eular;

            LinkUtil.GetWorldPosFromTarget(portA.Body, linkInfoB.relativePos, linkInfoB.relativeDir, out pos, out eular);

            var startPos = portB.Body.transform.position;
            var startforward = portB.Body.transform.forward;

            if (quickLink || autoLinkTime < 0.1f)
            {
                yield return new WaitForSeconds(autoLinkTime);
                portB.Body.transform.eulerAngles = eular;
                portB.Body.transform.position = pos;
                LinkUtil.UpdateBrotherPos(portB.Body, new List<LinkItem>());
            }
            else
            {
                var context = new List<LinkItem>();
                for (float j = 0; j < autoLinkTime; j += Time.deltaTime)
                {
                    yield return null;
                    portB.Body.transform.position = Vector3.Lerp(startPos, pos, j / autoLinkTime);
                    portB.Body.transform.eulerAngles = Vector3.Lerp(startforward, eular, j / autoLinkTime);
                    context.Clear();
                    LinkUtil.UpdateBrotherPos(portB.Body, context);
                }
            }

        }
    }
}