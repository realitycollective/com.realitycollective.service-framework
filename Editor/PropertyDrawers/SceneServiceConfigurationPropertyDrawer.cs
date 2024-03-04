using RealityCollective.ServiceFramework.Definitions;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SceneServiceConfiguration))]
    public class SceneServiceConfigurationPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Calculate half of the width for each field
            float nameWidth = position.width * 0.2f;

            var profileField = property.FindPropertyRelative("profile");

            Rect profileRect = new Rect(position.x + nameWidth, position.y, position.width - nameWidth, position.height);
            EditorGUI.PropertyField(profileRect, profileField, GUIContent.none);
            if (profileField.objectReferenceValue != null)
            {
                var objectRef = profileField.objectReferenceValue as SceneServiceProvidersProfile;
                Rect nameRect = new Rect(position.x, position.y, nameWidth, position.height);
                EditorGUI.LabelField(nameRect, objectRef.SceneName);
            }
            else
            {
                var nameRect = new Rect(position.x, position.y, nameWidth, position.height);
                EditorGUI.LabelField(nameRect, "None");
            }
            EditorGUI.EndProperty();
        }
    }
}