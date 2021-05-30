using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace CharacterSystem.Editor
{
    public static class CharacterStatDrawer
    {
        public static Frame CreatePropertyGUI(SerializedProperty property, string title, Color? color = null)
        {
            ICharacterStat stat = property.GetValue<ICharacterStat>();
            ProgressBar currentValueBar;

            void UpdateStatusBar()
            {
                if (currentValueBar == null) return;

                currentValueBar.title = $"{title}: {stat.Value}";
                currentValueBar.value = (float)Normalize(
                    Convert.ToDouble(stat.Value),
                    Convert.ToDouble(stat.Max),
                    Convert.ToDouble(stat.Min)
                ) * 100;
            }


            Frame frame = new Frame() { IsCollapsed = true };


            if (stat != null)
            {
                currentValueBar = new ProgressBar();
                currentValueBar.style.flexGrow = 1;

                UpdateStatusBar();

                if (color.HasValue)
                {
                    var progress = currentValueBar.Q<VisualElement>(className: "unity-progress-bar__progress");
                    progress.style.backgroundColor = new Color(color.Value.r, color.Value.g, color.Value.b, 0.35f);
                }

                PropertyField minfield = new PropertyField(property.FindPropertyRelative("_min"));
                minfield.RegisterValueChangeCallback((e) => { stat.Clamp(); UpdateStatusBar(); });

                PropertyField maxfield = new PropertyField(property.FindPropertyRelative("_max"));
                maxfield.RegisterValueChangeCallback((e) => { stat.Clamp(); UpdateStatusBar(); });

                PropertyField currentfield = new PropertyField(property.FindPropertyRelative("_value"));
                currentfield.RegisterValueChangeCallback((e) => { stat.Clamp(); UpdateStatusBar(); });

                minfield.Bind(property.serializedObject);
                maxfield.Bind(property.serializedObject);
                currentfield.Bind(property.serializedObject);

                frame.Header.Add(currentValueBar);

                frame.Add(minfield);
                frame.Add(maxfield);
                frame.Add(currentfield);
            }

            return frame;
        }

        private static double Normalize(double current, double max, double min)
        {
            return (current - min) / (max - min);
        }

    }
}
