﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace InteractSystem.Actions
{
    public delegate void ChargeEvent(Vector3 center, ChargeData data, UnityAction onComplete);
}
