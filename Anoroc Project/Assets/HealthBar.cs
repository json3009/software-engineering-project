using System;
using System.Collections;
using System.Collections.Generic;
using CharacterSystem;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    public Character character;
    public Slider slider;
    public TypeOfSlide type = TypeOfSlide.Health;

    public enum TypeOfSlide
    {
        Health,
        Mana
    }

    private void Start()
    {
        switch (type)
        {
            case TypeOfSlide.Health:
                slider.maxValue = character.Health.Max;                
                slider.value = character.Health.Value;
        
                character.Health.OnChange += () =>
                {
                    slider.maxValue = character.Health.Max;
                    slider.value = character.Health.Value;
                };
                break;
            case TypeOfSlide.Mana:
                slider.maxValue = character.Mana.Max;
                slider.value = character.Mana.Value;
        
                character.Mana.OnChange += () =>
                {
                    slider.maxValue = character.Mana.Max;
                    slider.value = character.Mana.Value;
                };
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
