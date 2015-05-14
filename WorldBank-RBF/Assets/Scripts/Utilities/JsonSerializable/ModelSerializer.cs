﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using JsonFx.Json;

public class ModelSerializer {

	static string PATH {
		get { return Application.dataPath + "/Scripts/Utilities/JsonSerializable/Data/"; }
	}

	public static void Save (object obj, string fileName = "") {
        fileName = (fileName == "") ? obj.GetType ().ToString () : fileName;
        WriteJsonData (CreateModelFromObject (obj), fileName);
    }

    public static void Load (object obj, string fileName = "") {
    	fileName = (fileName == "") ? obj.GetType ().ToString () : fileName;
    	ApplyModelPropertiesToObject (obj, ReadJsonData (obj, fileName));
    }

    // Saving
	static object ApplyObjectPropertiesToModel (object obj, object applyTo, List<PropertyInfo> modelProperties) {

        for (int i = 0; i < modelProperties.Count; i ++) {
           
            PropertyInfo p = modelProperties[i];
            object value = null;
            System.Type memberType = null;

            PropertyInfo targetProperty = obj.GetType ().GetProperty (p.Name);
            if (targetProperty != null) {
                value = targetProperty.GetValue (obj, null);
                memberType = targetProperty.PropertyType;
            } else {
                FieldInfo targetField = obj.GetType ().GetField (p.Name);
                if (targetField != null) {
                    value = targetField.GetValue (obj);
                    memberType = targetField.FieldType;
                }
            }

            if (typeof (IEnumerable).IsAssignableFrom (memberType) && memberType != typeof (string)) {
                
                IList ilist = (IList)value;
                IEnumerable<object> list = ilist.Cast<object> ().Select (x => CreateModelFromObject (x));

                // List
                if (value is IList 
                    && value.GetType ().IsGenericType 
                    && !IsFundamental (memberType.GetGenericArguments ()[0])) {
                    value = list.ToList ();

                // Array
                } else if (memberType.IsArray && !IsFundamental (memberType.GetElementType ())) {
                    value = list.ToArray ();
                }

            } else {
                if (!IsFundamental (memberType)) {
                    value = CreateModelFromObject (value);
                } 
            }

            if (value == null) {
                continue;
            } else {
                memberType = value.GetType ();
            }

            if (value != null && memberType != null) {
                p.SetValue (
                    applyTo,
                    Convert.ChangeType (value, memberType),
                    null);
            } else {
                Debug.LogError (obj.GetType () + " does not include a field or property named " + p.Name);
            }
        }

        return applyTo;
    }

    // Loading
    static void ApplyModelPropertiesToObject (object obj, object model) {
        
        if (obj == null)
            return;

        System.Type modelType = GetModelType (obj);
        System.Type objType = obj.GetType ();
        Dictionary<string, object> dict = (Dictionary<string, object>)model;

        foreach (var val in dict) {

            string propName = val.Key;
            PropertyInfo property = modelType.GetProperty (propName);
            if (property == null) {
                continue;
            }

            object value = val.Value;
            System.Type memberType = property.PropertyType;

            if (typeof (IEnumerable).IsAssignableFrom (memberType)) {
                
                IList ilist = (IList)value;
                List<object> list = ilist.Cast<object> ().ToList ();

                if (memberType.IsGenericType) {

                    FieldInfo field = objType.GetField (propName);
                    IList objIlist = (IList)field.GetValue (obj);
                    List<object> objList = objIlist.Cast<object> ().ToList ();

                	// List
                    System.Type oType = memberType.GetGenericArguments ()[0];
                    if (IsFundamental (oType)) {
                        if (objList.Count < list.Count) {
                            objList.AddRange (new object[list.Count-objList.Count]);
                        }
                        value = ConvertObjectListToTypeList (
                            list, objType.GetField (propName).FieldType, oType);
                        memberType = value.GetType ();
                    } else {
                        SetObjectsFromModels (
                            objType.GetField (propName).FieldType.GetGenericArguments ()[0],
                            objType.GetField (propName).GetValue (obj), list);
                        continue;
                    }
                } else {

                	// Array
                    System.Type oType = memberType.GetElementType ();
                    // TODO: this does not pass the correct type into SetObjectsFromModels (must grab it from the array)
                    Debug.Log ("array " + oType);
                    if (!IsFundamental (oType)) {
                        SetObjectsFromModels (
                            oType,
                            objType.GetField (propName).GetValue (obj), list);
                        continue;
                    }
                }
            } else {
                if (!IsFundamental (memberType)) {
                    object o = objType.GetField (propName).GetValue (obj);
                    ApplyModelPropertiesToObject (o, value);
                    continue;
                }
            }

            SetMemberValue (obj, propName, value);
        }
    }

    static object CreateModelFromObject (System.Object obj) {

        if (obj == null) 
            return null;

        System.Type modelType = GetModelType (obj);
        object model = Activator.CreateInstance (modelType);
        List<PropertyInfo> modelProperties  = new List<PropertyInfo> (modelType.GetProperties ());
        model = ApplyObjectPropertiesToModel (obj, model, modelProperties);

        return model;
    }

    static void SetObjectsFromModels (System.Type itemType, object group, List<object> models) {
        IList ilist = (IList)group;
        List<object> list = ilist.Cast<object> ().ToList ();
        for (int i = 0; i < models.Count; i ++) {
            if (list.Count < i+1) {
                if (typeof (IEditorPoolable).IsAssignableFrom (itemType)) {
                    list.Add (EditorObjectPool.Create (itemType.ToString ()));
                } else {
                    list.Add (Activator.CreateInstance (itemType));
                }
            }
            ApplyModelPropertiesToObject (list[i], models[i]);
        }
    }

    static void SetMemberValue (object obj, string memberName, object value) {
        
        System.Type objType = obj.GetType ();
        System.Type memberType = value.GetType ();
        
        PropertyInfo targetProperty = objType.GetProperty (memberName);
        if (targetProperty != null) {
            targetProperty.SetValue (
                obj, Convert.ChangeType (value, memberType), null);
        }

        FieldInfo targetField = objType.GetField (memberName);
        if (targetField != null) {
            targetField.SetValue (
                obj, Convert.ChangeType (value, memberType));
        }
    }

    static object ConvertObjectListToTypeList (List<object> fromList, System.Type toListType, System.Type toItemType) {
        object newList = Activator.CreateInstance (toListType);
        IList l = (IList)newList;
        foreach (object ob in fromList) {
            l.Add (Convert.ChangeType (ob, toItemType));
        }
        return l;
    }

    static System.Type GetModelType (object obj) {
        JsonSerializableAttribute j = (JsonSerializableAttribute) Attribute.GetCustomAttribute (
            obj.GetType (), 
            typeof (JsonSerializableAttribute));
        return j.modelType;
    }

    static bool IsFundamental (System.Type type) {
        return type.IsPrimitive || type.Equals (typeof (string)) || type.Equals (typeof (DateTime));
    }

    static public void WriteJsonData (object obj, string fileName) {
		var streamWriter = new StreamWriter (PATH + "" + fileName + ".json");
        streamWriter.Write (JsonWriter.Serialize (obj));
        streamWriter.Close ();
    }

    static public object ReadJsonData (object obj, string fileName) {
        StreamReader streamReader = new StreamReader (PATH + "" + fileName + ".json");
        string data = streamReader.ReadToEnd ();
        streamReader.Close ();
        return JsonReader.Deserialize<object> (data);
    }
}
