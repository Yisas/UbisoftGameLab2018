using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

namespace cakeslice
{
    public class OutlineAnimation : MonoBehaviour
    {
        bool pingPong = true;
        private static float smoothFactor = 0.2f;
        private bool first = true;

        // Use this for initialization
        void Start()
        {
            Color c = GetComponent<OutlineEffect>().lineColor0;
            c.a = 0;
            GetComponent<OutlineEffect>().lineColor0 = c;
        }

        // Update is called once per frame
        void Update()
        {
            if (first)
            {
                Color cInit = GetComponent<OutlineEffect>().lineColor0;
                cInit.a += Time.deltaTime * 0.1f;

                if (cInit.a >= 1)
                    first = false;

                GetComponent<OutlineEffect>().lineColor0 = cInit;

                return;
            }

            Color c = GetComponent<OutlineEffect>().lineColor0;

            if(pingPong)
            {
                c.a += Time.deltaTime * smoothFactor;

                if(c.a >= 1)
                    pingPong = false;
            }
            else
            {
                c.a -= Time.deltaTime * smoothFactor;

                if (c.a <= 0.7)
                    pingPong = true;
            }

            c.a = Mathf.Clamp01(c.a);
            GetComponent<OutlineEffect>().lineColor0 = c;
            GetComponent<OutlineEffect>().UpdateMaterialsPublicProperties();
        }
    }
}