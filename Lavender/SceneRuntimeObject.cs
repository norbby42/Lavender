using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Lavender
{
    public class SceneRuntimeObject : MonoBehaviour
    {
        private float _updateWaitTime = 5f;
        private bool _trigger = true;

        private void Update()
        {
            if (_trigger)
            {
                _trigger = false;
                StartCoroutine(updateWaitRoutine());
            }
        }

        IEnumerator updateWaitRoutine()
        {
            yield return new WaitForSeconds(_updateWaitTime);

            Lavender.instance.firstUpdateFinished = true;
            LavenderLog.Log("SceneRuntimeObject finished first Update!");

            if(BepinexPlugin.Settings.SceneRuntimeObjectNotification.Value) 
            { 
                Notifications.instance.CreateNotification("Lavender", "SceneRuntimeObject finished first Update!", false); 
            }
        }

        void OnDisable()
        {
            Lavender.instance.firstUpdateFinished = false;
            LavenderLog.Log("SceneRuntimeObject disabled!");
        }
    }
}
