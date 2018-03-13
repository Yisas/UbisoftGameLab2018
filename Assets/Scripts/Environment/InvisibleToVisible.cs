﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleToVisible : MonoBehaviour
{
    private Shader m_OldShader = null;
    private Color m_OldColor = Color.black;
    private float m_Transparency = 0.3f;
    private const float startingTransparency = 0.0f;

    public float TargetTransparency { get; set; }
    public float FadeInTimeout = 12f; //Set with Adaptative Level Transparency To Visible Time
    private bool isStandard;

    public float delayToFadeInTime = 10; //how long until this element starts fading in
    private float currentWaitedTime = 0;

    public void Start()
    {
        // reset the transparency;
        m_Transparency = startingTransparency;


        if (m_OldShader == null)
        {
            // Save the current shader
            m_OldShader = GetComponent<Renderer>().material.shader;
            m_OldColor = GetComponent<Renderer>().material.color;

            if (GetComponent<Renderer>().material.shader.name.Contains("Standard"))
            {
                isStandard = true;
                //GetComponent<Renderer>().material = new Material(GetComponent<Renderer>().material);
            }
            else
            {
                GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
            }
        }

        FadeIn();
    }

    void Update()
    {
        if (currentWaitedTime < delayToFadeInTime)
        {
            currentWaitedTime += Time.deltaTime;
            return;
        }

        FadeIn();
    }

    void FadeIn()
    {
        if (m_Transparency < 1.0f)
        {
            if (isStandard)
            {
                StandardShaderUtils.ChangeRenderMode(GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Fade);
            }
            Color C = GetComponent<Renderer>().material.color;
            C.a = m_Transparency;
            GetComponent<Renderer>().material.color = C;
        }
        else
        {
            if (isStandard)
            {
                StandardShaderUtils.ChangeRenderMode(GetComponent<Renderer>().material, StandardShaderUtils.BlendMode.Opaque);
            }
            // Reset the shader
            GetComponent<Renderer>().material.shader = m_OldShader;
            GetComponent<Renderer>().material.color = m_OldColor;
            // And remove this script

            Destroy(this);
        }

        //Fading in
        m_Transparency += ((1.0f - TargetTransparency) * Time.deltaTime) / FadeInTimeout;
    }
}
