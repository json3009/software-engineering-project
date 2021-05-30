using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CombatSystem.SpellSystem;
using CombatSystem.SpellSystem.Attributes;
using CombatSystem.Stats.Modifiers;
using Scripts.CombatSystem;
using StatSystem;
using StatSystem.StatAttributes;
using StatSystem.StatModifiers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering.Universal;

public class testExplosion : SpellVisual
{
    // Start is called before the first frame update
    [SerializeField] public new Light2D light;
    [SerializeField] public ParticleSystem system;
    [SerializeField] public ParticleSystem subSystem;

    [SerializeField] public float baseRange = 10f;

    [SerializeField] public float magnitude = 0.5f;
    
    
    [SpellVisualInput] public FloatModifier duration;   //DEFAULT: 0.4f;

    [SpellVisualInput] public FloatModifier fadein;     //DEFAULT: 3f
    [SpellVisualInput] public FloatModifier fadeoff;    //DEFAULT: 0.4f
    
    

    [SerializeField, ColorUsage(true, true)] public Color color = Color.red;
    [SerializeField, ColorUsage(true, true)] public Color ringColor = Color.red;
    
    private float baseIntensity;
    private bool hasStarted = false;

    private CameraRestrict cam;

    public override void StartCall()
    {
        Assert.IsNotNull(light);
        Assert.IsNotNull(system);
        Renderer r = system.GetComponent<Renderer>();
        Renderer rsub = subSystem.GetComponent<Renderer>();

        if (!Data.TryGetAttribute("_attackDamageType", out IStatAttribute attackDamageAttr))
            throw new NullReferenceException($"Attack Damage missing on explosion visuals");
        
        if (!Data.TryGetAttribute("_radius", out FloatAttribute radiusAttr))
            throw new NullReferenceException($"Radius missing on explosion visuals");

        var d = (DamageTypeModifier) attackDamageAttr.Modifiers.LastOrDefault();
        if (d is {Value: { }})
        {
            var def = d.Definition.GetType(d.Value.Value);
            if (def != null)
            {
                color = def.Color;
                ringColor = def.Color;
            }
        }

        light.color = color;
        r.material.SetColor("Color_particle", ringColor);
        rsub.material.SetColor("Color_particle", ringColor);
        
        

        light.intensity = light.intensity * Behaviour.NormalizedMana; 
        
        baseIntensity = light.intensity;
        light.intensity = 0;
        light.enabled = true;

        var test = system.main;
        test.startSpeed = new ParticleSystem.MinMaxCurve( (radiusAttr.Value * Behaviour.NormalizedMana) * (1 + magnitude));

        Behaviour.RequestDelayedDestroy(this);

        cam = Camera.main.transform.parent.GetComponent<CameraRestrict>();
    }

    private void Update()
    {
        if (hasStarted)
        {
            light.intensity -= fadeoff.Value * Time.deltaTime;
            if (light.intensity < 0)
                light.enabled = false;

            if (light.intensity < 0 && !system.isPlaying)
                VisualsHaveFinished();

            return;
        }

        if (!hasStarted)
        {
            if (light.intensity <= baseIntensity)
            {
                light.intensity += fadein.Value * Time.deltaTime;
            }
            else
            {
                hasStarted = true;
                system.Play();
                cam.Shake(duration.Value, magnitude);
            }
        }
    }

}
