using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RotaryHeart.Lib.SerializableDictionary
{
    [CustomPropertyDrawer(typeof(DrawableDictionary), true)]
    public class DictionaryPropertyDrawer : PropertyDrawer
    {
        #region Fields
        SerializedProperty reqReferences;
        SerializedProperty isExpanded;
        SerializedProperty KeysValues;
        SerializedProperty KeysProp;
        SerializedProperty ValuesProp;

        readonly GUIContent idContent = new GUIContent("Id");
        readonly GUIContent valueContent = new GUIContent("Value");
        readonly GUIStyle tooTipStyle = new GUIStyle("Tooltip");

        ReorderableList list;

        string title;

        System.Type[] typesNative =
        {
                typeof(bool),
                typeof(byte),
                typeof(float),
                typeof(int),
                typeof(string),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Matrix4x4),
                typeof(Color),
                typeof(Rect),
                typeof(LayerMask)
        };
        #endregion

        private SerializedProperty GetReferences(SerializedProperty property)
        {
            SerializedProperty listProp = property.FindPropertyRelative("reorderableList");
            reqReferences = property.FindPropertyRelative("reqReferences");
            isExpanded = property.FindPropertyRelative("isExpanded");

            list = GetTargetObjectOfProperty(listProp) as ReorderableList;

            KeysValues = property.FindPropertyRelative("_keyValues");
            KeysProp = property.FindPropertyRelative("_keys");
            ValuesProp = property.FindPropertyRelative("_values");

            return listProp;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GetReferences(property);

            float height = EditorGUIUtility.singleLineHeight;
            float verticalSpace = list.verticalSpacing * 2;

            if (KeysProp.arrayElementType.Contains("$"))
                height += EditorGUIUtility.singleLineHeight;

            if (isExpanded.boolValue)
            {
                height += EditorGUIUtility.singleLineHeight;

                int keysSize = KeysProp.arraySize;

                if (Constants.ShowPages)
                {
                    height += EditorGUIUtility.singleLineHeight;
                    keysSize = Mathf.Min(keysSize, Constants.PageCount);
                }

                if (keysSize > 0)
                {
                    for (int keyIndex = 0; keyIndex < keysSize; keyIndex++)
                    {
                        if (keyIndex >= KeysProp.arraySize)
                            break;

                        var keyProp = KeysProp.GetArrayElementAtIndex(keyIndex);

                        height += List_getElementHeightCallback(keyProp, keyIndex) + verticalSpace;
                    }
                }
                else
                    height += EditorGUIUtility.singleLineHeight + verticalSpace * 3;
            }

            return height + verticalSpace;
        }

        #region Helpers

        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private void SetTargetObjectOfProperty(SerializedProperty prop, object value, bool custom = false)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            if (Object.ReferenceEquals(obj, null)) return;

            try
            {
                var element = elements.Last();
                var tp = obj.GetType();

                if (custom)
                    tp = tp.BaseType;

                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var arr = field.GetValue(obj) as System.Collections.IList;
                    arr[index] = value;
                }
                else
                {
                    var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                    }
                }

            }
            catch
            {
                return;
            }
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        private bool IsUnitySerialized(FieldInfo fieldInfo)
        {
            object[] customAttributes = fieldInfo.GetCustomAttributes(true);
            if (customAttributes.Any(x => x is System.NonSerializedAttribute))
            {
                return false;
            }
            if (fieldInfo.IsPrivate && !customAttributes.Any(x => x is SerializeField))
            {
                return false;
            }
            return IsUnitySerialized(fieldInfo.FieldType);
        }

        private bool IsUnitySerialized(System.Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    return IsUnitySerialized(type.GetGenericArguments()[0]);
                }
                return false;
            }
            if (type.IsEnum)
            {
                return true;
            }
            if (type.IsValueType)
            {
                return true;
            }
            if (type.IsAssignableFrom(typeof(Object)))
            {
                return true;
            }
            if (typesNative.Contains(type) || (type.IsArray && typesNative.Contains(type.GetElementType())))
            {
                return true;
            }
            return false;
        }

        private Quaternion ConvertToQuaternion(Vector4 v4)
        {
            return new Quaternion(v4.x, v4.y, v4.z, v4.w);
        }

        private Vector4 QuaternionToVector4(Quaternion q)
        {
            return new Vector4(q.x, q.y, q.z, q.w);
        }

        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProp = GetReferences(property);

            reqReferences.isExpanded = false;
            property.isExpanded = false;

            title = label.text;

            Rect nextRect;

            string keyType = KeysProp.arrayElementType;
            int offset = 0;

            if (keyType.Contains("$"))
            {
                nextRect = GetNextRect(ref position);
                EditorGUI.PropertyField(nextRect, reqReferences);
                offset = 20;
            }

            nextRect = GetNextRect(ref position);

            if (ValuesProp.arraySize != KeysProp.arraySize)
                ValuesProp.arraySize = KeysProp.arraySize;
            if (KeysValues.arraySize != KeysProp.arraySize)
                KeysValues.arraySize = KeysProp.arraySize;

            if (list != null)
            {
                if (!list.HasList)
                {
                    list = new ReorderableList(KeysProp, true, true, true);

                    list.onRemoveCallback += List_onRemoveCallback;
                    list.onAddCallback += List_onAddCallback;
                    list.drawElementCallback += List_drawElementCallback;
                    list.drawHeaderCallback += List_drawHeaderCallback;
                    list.getElementHeightCallback += List_getElementHeightCallback;
                    list.onElementsReorder += List_onElementsReorder;
                    list.headerExpand += List_headerExpand;

                    SetTargetObjectOfProperty(listProp, list);
                }

                list.List = KeysProp;
                list.isExpanded = isExpanded.boolValue;

                list.DoList(new Rect(nextRect.x, nextRect.y, nextRect.width, GetPropertyHeight(property, label) - offset), label, Constants.ShowPages, Constants.PageCount);
            }
        }

        private void List_headerExpand(bool expand)
        {
            isExpanded.boolValue = expand;

            for (int i = 0; i < KeysValues.arraySize; i++)
            {
                KeysProp.GetArrayElementAtIndex(i).isExpanded = expand;
                ValuesProp.GetArrayElementAtIndex(i).isExpanded = expand;
            }
        }

        private void List_onElementsReorder(int startIndex, int newIndex)
        {
            KeysValues.MoveArrayElement(startIndex, newIndex);
            ValuesProp.MoveArrayElement(startIndex, newIndex);
        }

        private void List_drawHeaderCallback(Rect rect, GUIContent label)
        {
            rect.x += 6;

            isExpanded.boolValue = EditorGUI.Foldout(rect, isExpanded.boolValue, "", true);
            isExpanded.serializedObject.ApplyModifiedProperties();
            EditorGUI.LabelField(rect, title + (Constants.ShowSize ? " [" + KeysValues.arraySize + "]" : ""));
        }

        private float List_getElementHeightCallback(SerializedProperty element, int index)
        {
            float height;

            bool containsAttribute = fieldInfo.GetCustomAttributes(typeof(DrawKeyAsPropertyAttribute), true).Any();

            height = EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + list.verticalSpacing * 4;

            if (element.isExpanded)
            {
                var valueProp = ValuesProp.GetArrayElementAtIndex(index);

                height += EditorGUI.GetPropertyHeight(valueProp, GUIContent.none, true) + list.verticalSpacing - (containsAttribute ? EditorGUIUtility.singleLineHeight : 0);
            }

            return height;
        }

        private void List_drawElementCallback(Rect rect, SerializedProperty element, GUIContent label, int index, bool selected, bool focused)
        {
            var keyValueProp = KeysValues.GetArrayElementAtIndex(index);
            var keyProp = KeysProp.GetArrayElementAtIndex(index);
            var valueProp = ValuesProp.GetArrayElementAtIndex(index);

            SerializedProperty keyToUse = keyProp.propertyType == SerializedPropertyType.Generic ? keyProp : keyValueProp;

            if (keyToUse.propertyType == SerializedPropertyType.Generic)
            {
                rect.x -= 10;
                rect.width += 10;
            }

            if (!selected)
            {
                if (Event.current.type == EventType.Repaint)
                    tooTipStyle.Draw(rect, false, false, false, false);
            }

            rect.height = EditorGUIUtility.singleLineHeight;

            bool containsAttribute = fieldInfo.GetCustomAttributes(typeof(DrawKeyAsPropertyAttribute), true).Any();

            Rect keyRect = new Rect(rect.x + 50, rect.y + 4, rect.width - 52, rect.height);
            Rect valueRect = new Rect(keyRect);

            #region Key Field

            string propName = "";
            if (containsAttribute)
            {
                var field = fieldInfo.FieldType.BaseType.GetField("_keys", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                var fieldType = field.FieldType;
                var elementType = fieldType.GetGenericArguments()[0];
                foreach (var fi in elementType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (IsUnitySerialized(fi))
                    {
                        propName = fi.Name;
                        break;
                    }
                }
            }

            if ((containsAttribute && !string.IsNullOrEmpty(propName)) || keyToUse.propertyType != SerializedPropertyType.Generic)
            {
                if (containsAttribute)
                    keyRect.height = EditorGUI.GetPropertyHeight(keyProp, GUIContent.none, true) - (keyProp.isExpanded ? EditorGUIUtility.singleLineHeight : 0);

                keyProp.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 15, keyRect.y, 20, rect.height), keyProp.isExpanded, idContent, true);
            }

            GUI.SetNextControlName("CheckGenericFocus" + index);

            switch (keyToUse.propertyType)
            {
                case SerializedPropertyType.Quaternion:
                    EditorGUI.BeginChangeCheck();
                    var newV4 = EditorGUI.Vector4Field(keyRect, GUIContent.none, QuaternionToVector4(keyToUse.quaternionValue));

                    if (EditorGUI.EndChangeCheck())
                    {
                        keyToUse.quaternionValue = ConvertToQuaternion(newV4);
                    }
                    break;

                case SerializedPropertyType.Enum:
                    string[] names = keyToUse.enumDisplayNames;
                    if (names.Length <= keyToUse.enumValueIndex || keyToUse.enumValueIndex < 0)
                    {
                        list.Selected = new[] { index };
                        List_onRemoveCallback(list);
                        return;
                    }
                    var selectedVal = names[keyToUse.enumValueIndex];

                    if (GUI.Button(keyRect, selectedVal, EditorStyles.layerMaskField))
                    {
                        List<string> usedNames = new List<string>();
                        GenericMenu menu = new GenericMenu();

                        for (int i = 0; i < KeysValues.arraySize; i++)
                        {
                            usedNames.Add(names[KeysValues.GetArrayElementAtIndex(i).enumValueIndex]);
                        }

                        for (int i = 0; i < names.Length; i++)
                        {
                            int nameIndex = i;

                            if (usedNames.Contains(names[nameIndex]) && !names[nameIndex].Equals(selectedVal))
                            {
                                menu.AddDisabledItem(new GUIContent(names[nameIndex]));
                            }
                            else
                            {
                                menu.AddItem(new GUIContent(names[nameIndex]), selectedVal == names[nameIndex], () =>
                                {
                                    keyValueProp.enumValueIndex = nameIndex;
                                    keyProp.enumValueIndex = nameIndex;
                                    keyToUse.serializedObject.ApplyModifiedProperties();
                                });
                            }
                        }

                        menu.ShowAsContext();

                        Event.current.Use();
                    }
                    break;

                case SerializedPropertyType.Generic:
                    if (containsAttribute && !string.IsNullOrEmpty(propName))
                    {
                        EditorGUI.PropertyField(keyRect, keyToUse.FindPropertyRelative(propName), GUIContent.none, false);
                    }
                    else
                    {
                        keyRect.height = EditorGUI.GetPropertyHeight(keyToUse, idContent);
                        EditorGUI.PropertyField(new Rect(rect.x + 15, keyRect.y, keyRect.width + 35, keyRect.height), keyToUse, idContent, true);
                    }
                    break;

                default:
                    EditorGUI.PropertyField(keyRect, keyToUse, GUIContent.none, false);
                    break;
            }

            if (keyToUse.propertyType != SerializedPropertyType.Generic)
            {
                var oldId = GetKeyValue(keyProp);
                var newId = GetKeyValue(keyValueProp);

                if ((keyToUse.propertyType == SerializedPropertyType.String && string.IsNullOrEmpty(newId.ToString())) || newId == null)
                {
                    GUIContent content = EditorGUIUtility.IconContent("console.warnicon.sml");
                    content.tooltip = "ID cannot be left empty";

                    GUI.Button(new Rect(keyRect.x - 15, keyRect.y, 30, 30), content, GUIStyle.none);
                }
                else if ((oldId == null && newId != null) || !oldId.Equals(newId))
                {
                    if (ContainsId(newId, index))
                    {
                        if (GUI.GetNameOfFocusedControl().Equals("CheckGenericFocus" + index))
                        {
                            GUIContent content = EditorGUIUtility.IconContent("console.erroricon.sml");
                            content.tooltip = "Dictionary already has this id, this id cannot be used";

                            GUI.Button(new Rect(keyRect.x - 15, keyRect.y, 30, 30), content, GUIStyle.none);
                        }
                        else
                        {
                            SetValue(keyValueProp, oldId);
                        }
                    }
                    else
                    {
                        SetGenericValue(keyProp, valueProp, newId);
                    }
                }
            }
            #endregion Fey Field

            valueRect.y = keyRect.yMax + 3 - (containsAttribute ? 2 : 0);
            valueRect.x -= 20;
            valueRect.width += 20;

            #region Value Field

            if (valueProp.propertyType == SerializedPropertyType.Generic)
            {
                if (keyToUse.propertyType != SerializedPropertyType.Generic)
                    valueRect.y -= 3;

                EditorGUI.BeginChangeCheck();

                if (keyProp.isExpanded)
                {
                    EditorGUI.BeginProperty(valueRect, GUIContent.none, valueProp);

                    if (valueProp.propertyType == SerializedPropertyType.Quaternion)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newV4 = EditorGUI.Vector4Field(valueRect, GUIContent.none, QuaternionToVector4(valueProp.quaternionValue));

                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.quaternionValue = ConvertToQuaternion(newV4);
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(valueRect, valueProp, valueContent, true);
                    }
                    EditorGUI.EndProperty();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    ValuesProp.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (keyProp.isExpanded)
                {
                    valueRect.x -= 10;
                    valueRect.width += 10;

                    EditorGUI.BeginProperty(valueRect, GUIContent.none, valueProp);

                    EditorGUI.PrefixLabel(valueRect, valueContent);
                    if (valueProp.propertyType == SerializedPropertyType.Quaternion)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newV4 = EditorGUI.Vector4Field(new Rect(valueRect.x + 45, valueRect.y, valueRect.width - 45, valueRect.height), GUIContent.none, QuaternionToVector4(valueProp.quaternionValue));

                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.quaternionValue = ConvertToQuaternion(newV4);
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(new Rect(valueRect.x + 45, valueRect.y, valueRect.width - 45, valueRect.height), valueProp, GUIContent.none, true);
                    }
                    EditorGUI.EndProperty();
                }
            }

            #endregion Value Field
        }

        private void List_onAddCallback(ReorderableList list)
        {
            KeysValues.arraySize = ValuesProp.arraySize = ++KeysProp.arraySize;

            SetPropertyDefault(KeysValues.GetArrayElementAtIndex(KeysValues.arraySize - 1), KeysValues);
            SetPropertyDefault(KeysProp.GetArrayElementAtIndex(KeysProp.arraySize - 1), KeysProp);

            KeysValues.serializedObject.ApplyModifiedProperties();
            ValuesProp.serializedObject.ApplyModifiedProperties();
            KeysProp.serializedObject.ApplyModifiedProperties();

        }

        private void List_onRemoveCallback(ReorderableList list)
        {
            for (int i = list.Selected.Length - 1; i >= 0; i--)
            {
                int index = list.Selected[i];

                int last = KeysProp.arraySize - 1;

                KeysValues.MoveArrayElement(index, last);
                KeysProp.MoveArrayElement(index, last);
                ValuesProp.MoveArrayElement(index, last);

                KeysValues.arraySize--;
                KeysProp.arraySize--;
                ValuesProp.arraySize--;
            }

            ValuesProp.serializedObject.ApplyModifiedProperties();
            ValuesProp.serializedObject.Update();
        }

        private bool ContainsId(object obj, int index)
        {
            for (int i = 0; i < KeysProp.arraySize; i++)
            {
                if (index == i)
                {
                    continue;
                }

                object val = GetKeyValue(KeysProp.GetArrayElementAtIndex(i));

                if (val.Equals(obj))
                    return true;
            }
            return false;
        }

        private object GetKeyValue(SerializedProperty prop)
        {
            object obj = null;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    obj = prop.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    obj = prop.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    obj = prop.floatValue;
                    break;
                case SerializedPropertyType.String:
                    obj = prop.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    obj = prop.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    obj = prop.objectReferenceValue;
                    break;
                case SerializedPropertyType.Enum:
                    obj = prop.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    obj = prop.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    obj = prop.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    obj = prop.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    obj = prop.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    obj = prop.arraySize;
                    break;
                case SerializedPropertyType.Character:
                    obj = (char)prop.intValue;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    obj = prop.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    obj = prop.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    obj = GetGradientValue(prop);
                    break;
                case SerializedPropertyType.Quaternion:
                    obj = prop.quaternionValue;
                    break;
                case SerializedPropertyType.Generic:
                    obj = GetTargetObjectOfProperty(prop);
                    break;
                default:
                    Debug.LogError("Key Type not implemented: " + prop.propertyType);
                    break;
            }

            return obj;
        }

        private void SetValue(SerializedProperty prop, object obj)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (int)obj;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)obj;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)obj;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = (string)obj;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)obj;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = (Object)obj;
                    break;
                case SerializedPropertyType.Enum:
                    prop.enumValueIndex = (int)obj;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)obj;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)obj;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = (Vector4)obj;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)obj;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int)obj;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (char)obj;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = (AnimationCurve)obj;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)obj;
                    break;
                case SerializedPropertyType.Gradient:
                    SetGradientValue(prop, (Gradient)obj);
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = (Quaternion)obj;
                    break;
                case SerializedPropertyType.Generic:
                    SetTargetObjectOfProperty(prop, null);
                    break;
                default:
                    Debug.Log("Type not implemented: " + prop.propertyType);
                    break;
            }
        }

        private Gradient GetGradientValue(SerializedProperty prop)
        {
            PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(prop, null) as Gradient;
        }

        private void SetGradientValue(SerializedProperty prop, Gradient gradient)
        {
            PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty("gradientValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                return;

            propertyInfo.SetValue(prop, gradient, null);
        }

        private void SetGenericValue(SerializedProperty keyProp, SerializedProperty valueProp, object obj)
        {
            SetValue(keyProp, obj);

            IDAttribute attribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(IDAttribute)) as IDAttribute;

            if (attribute == null)
            {
                return;
            }

            SerializedProperty id = valueProp.FindPropertyRelative(attribute.Id);

            if (id == null)
            {
                Debug.LogError("Couldn't find any id field with name '" + attribute.Id + "' on field: " + fieldInfo.Name);
                return;
            }

            SetValue(id, obj);
        }

        private Rect GetNextRect(ref Rect position)
        {
            var h = EditorGUIUtility.singleLineHeight;
            var r = new Rect(position.x, position.y, position.width, h);
            position = new Rect(position.x, position.y + h, position.width, h);
            return r;
        }

        private void SetPropertyDefault(SerializedProperty prop, SerializedProperty parentProperty)
        {
            if (prop == null)
                throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = int.MaxValue;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = false;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = Mathf.Infinity;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = string.Empty;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = Color.black;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = -1;
                    break;
                case SerializedPropertyType.Enum:
                    int index = 0;

                    if (parentProperty != null)
                    {
                        List<int> numbersUsed = new List<int>();

                        for (int i = 0; i < parentProperty.arraySize; i++)
                            numbersUsed.Add(parentProperty.GetArrayElementAtIndex(i).enumValueIndex);

                        while (true)
                        {
                            if (!numbersUsed.Contains(index))
                            {
                                break;
                            }
                            index++;
                        }

                        if (index >= prop.enumNames.Length)
                            index = 0;
                    }

                    prop.enumValueIndex = index;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = Vector4.zero;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = Rect.zero;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = 0;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = 0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = null;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = default(Bounds);
                    break;
                case SerializedPropertyType.Gradient:
                    SetGradientValue(prop, new Gradient());
                    break;
                case SerializedPropertyType.Generic:
                    var t = prop.GetEnumerator();
                    while (t.MoveNext())
                    {
                        var val = t.Current;
                        SetPropertyDefault((val as SerializedProperty), null);
                    }
                    break;
                case SerializedPropertyType.Quaternion:
                    prop.quaternionValue = Quaternion.identity;
                    break;
                default:
                    Debug.Log("Type not implemented: " + prop.propertyType);
                    break;
            }
        }
    }
}
