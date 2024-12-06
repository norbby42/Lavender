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
            Debug.Log("[Lavender] SceneRuntimeObject finished first Update!");

            Notifications.instance.CreateNotification("OSML", "SceneRuntimeObject finished first Update!", false);
        }

        void OnDisable()
        {
            Lavender.instance.firstUpdateFinished = false;
            Debug.Log("[Lavender] SceneRuntimeObject disabled!");
        }
    }
}
