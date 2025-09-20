using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(InterfaceTypeAttribute))]
public class InterfaceTypeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		// 1. 이 Drawer는 ObjectReference 타입의 프로퍼티에만 작동합니다.
		if (property.propertyType != SerializedPropertyType.ObjectReference)
		{
			EditorGUI.LabelField(position, label.text, "InterfaceTypeAttribute는 Object 필드에만 사용할 수 있습니다.");
			return;
		}

		// 2. 속성에서 지정한 인터페이스 타입을 가져옵니다.
		var attribute = this.attribute as InterfaceTypeAttribute;

		EditorGUI.BeginProperty(position, label, property);

		// 3. 인스펙터에 Object 필드를 그립니다.
		var assignedObject = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(ScriptableObject), false);

		// 4. 사용자가 새로운 오브젝트를 할당했는지 검사합니다.
		if (assignedObject != null)
		{
			// 5. 할당된 오브젝트의 타입이 지정된 인터페이스를 구현하는지 확인합니다.
			Type objectType = assignedObject.GetType();
			if (!attribute.InterfaceType.IsAssignableFrom(objectType))
			{
				// 구현하지 않으면, 할당을 취소하고 경고를 출력합니다.
				Debug.LogWarning($"'{assignedObject.name}'({objectType.Name})는 '{attribute.InterfaceType.Name}' 인터페이스를 구현하지 않습니다.");
				property.objectReferenceValue = null;
			}
			else
			{
				property.objectReferenceValue = assignedObject;
			}
		}
		else
		{
			property.objectReferenceValue = null;
		}

		EditorGUI.EndProperty();
	}
}