using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTransparent : MonoBehaviour
{
    private Shader m_OldShader = null;
    private Color m_OldColor = Color.black;
    private float m_Transparency = 0.3f;
    private const float m_TargetTransparancy = 0.3f;
    private const float m_FallOff = 0.1f; // returns to 100% in 0.1 sec
    private bool shouldBeTransparent = true;
    public float TargetTransparency { get; set; }
    public float FadeInTimeout = 0.6f;
    public float FadeOutTimeout = 0.2f;

    public void BeTransparent()
    {
        // reset the transparency;
        m_Transparency = m_TargetTransparancy;
        shouldBeTransparent = true;

        if (m_OldShader == null)
        {
            // Save the current shader
            m_OldShader = GetComponent<Renderer>().material.shader;
            m_OldColor = GetComponent<Renderer>().material.color;
            GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        }
    }

    void Update()
    {
        if (m_Transparency < 1.0f)
        {
            Color C = GetComponent<Renderer>().material.color;
            C.a = m_Transparency;
            GetComponent<Renderer>().material.color = C;
        }
        else
        {
            // Reset the shader
            GetComponent<Renderer>().material.shader = m_OldShader;
            GetComponent<Renderer>().material.color = m_OldColor;
            // And remove this script
            Destroy(this);
        }

        //Are we fading in our out?
        if (shouldBeTransparent)
        {
            //Fading out
            if (m_Transparency > TargetTransparency)
            {
                m_Transparency -= ((1.0f - TargetTransparency) * Time.deltaTime) / FadeOutTimeout;
            }
        }
        else
        {
            //Fading in
            m_Transparency += ((1.0f - TargetTransparency) * Time.deltaTime) / FadeInTimeout;
        }

        shouldBeTransparent = false;
    }
}
