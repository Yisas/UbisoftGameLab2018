using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTrigger : AkTriggerBase {

    public void PlayPortalIdle()
    {
        if (triggerDelegate != null)
        {
            triggerDelegate(null);
        }
    }
}
