﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WorldActionSystem
{
    public interface IInstallEnd
    {
        void GetInstallDicAsync(UnityAction<Dictionary<string, List<InstallPos>>> onDicCrateed);
        bool SetStapActive(string stap);
        bool IsInstallStep(InstallPos installObj);
        bool HaveInstallPosInstalled(InstallPos installObj);
        List<InstallPos> GetNotInstalledPosList();
        List<InstallPos> GetNeedAutoInstallPosList(bool fauseAuto);
        bool AllElementInstalled();
        List<InstallPos> GetInstalledPosList();
    }

}