/*
 * Copyright (c) 2021 AoiKamishiro
 * 
 * This code is provided under the MIT license.
 *
 */

//LastUpdate:2020/11/27(JST) 

using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Kamishiro.UnityEditor.CopyDynamicBones
{
    public class Editor : EditorWindow
    {
        [MenuItem("Tools/Kamishiro/CopyDynamicBones", priority = 150)]
        private static void OnEnable()
        {
            Editor window = GetWindow<Editor>("CopyDynamicBones");
            window.minSize = new Vector2(320, 400);
        }
        private int page = 1;

        private bool foldDBC = false;
        private bool foldDB = false;
        private bool ofNotNull = false;
        private Transform source = null;
        private Transform target = null;
        private string sourcePrefix = "";
        private string sourceSuffix = "";
        private string targetPrefix = "";
        private string targetSuffix = "";

        private DynamicBone[] sourceDB = null;
        private DynamicBoneCollider[] sourceDBC = null;
        private Transform[] targetTR_DB_GO = null;
        private Transform[] targetTR_DB_RB = null;
        private Transform[] targetTR_DBC_GO = null;
        private DynamicBone[] targetDB = null;
        private DynamicBoneCollider[] targetDBC = null;
        private DynamicBoneCollider[][] sourceColliders = null;
        private Transform[][] sourceExclusion = null;
        private DynamicBoneCollider[][] targetColliders = null;
        private Transform[][] targetExclusion = null;
        private Vector2 scrollPosition1 = Vector2.zero;
        private Vector2 scrollPosition2 = Vector2.zero;
        private int count = 0;
        private bool fold1 = false;
        private float width4 = 0f;
        private float width2 = 0f;

        private void OnGUI()
        {
            width4 = (position.width - 36) / 4;
            width2 = (position.width - 28) / 2;
            UIHelper.ShurikenHeader("CopyDynamicBones");
            EditorGUILayout.Space();
            SourceAndTarget(); if (page == 1)
            {
                AttachComponent();
            }
            else if (page == 2)
            {
                ColliderAndExclusion();
            }
            EditorGUILayout.Space();
            UIHelper.ShurikenHeader("About");
            Version.DisplayVersion();
            GUILayout.Space(30);
        }

        private void SourceAndTarget()
        {
            EditorGUI.BeginDisabledGroup(page == 2);
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.LabelField("Source & Target Setting", EditorStyles.boldLabel);
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Source", GUILayout.Width(width4));
                    source = (Transform)EditorGUILayout.ObjectField(source, typeof(Transform), true);
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Target", GUILayout.Width(width4));
                    target = (Transform)EditorGUILayout.ObjectField(target, typeof(Transform), true);
                }
                EditorGUILayout.Space();
                fold1 = EditorGUILayout.Foldout(fold1, "Optional Setting");
                if (fold1)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("SourcePrefix", GUILayout.Width(width4));
                        sourcePrefix = EditorGUILayout.TextField(sourcePrefix);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("SourceSuffix", GUILayout.Width(width4));
                        sourceSuffix = EditorGUILayout.TextField(sourceSuffix);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("TargetPrefix", GUILayout.Width(width4));
                        targetPrefix = EditorGUILayout.TextField(targetPrefix);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("TargetSuffix", GUILayout.Width(width4));
                        targetSuffix = EditorGUILayout.TextField(targetSuffix);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (source == null || target == null)
                {
                    ofNotNull = false;
                }
                else
                {
                    ofNotNull = true;
                    sourceDB = source.GetComponentsInChildren<DynamicBone>(true);
                    sourceDBC = source.GetComponentsInChildren<DynamicBoneCollider>(true);
                    targetTR_DB_GO = new Transform[sourceDB.Length];
                    targetTR_DB_RB = new Transform[sourceDB.Length];
                    targetTR_DBC_GO = new Transform[sourceDBC.Length];

                    for (int i = 0; i < sourceDB.Length; i++)
                    {
                        targetTR_DB_GO[i] = FindChild(target, sourceDB[i].name);
                        targetTR_DB_RB[i] = FindChild(target, sourceDB[i].m_Root == null ? "Null" : sourceDB[i].m_Root.name);
                    }
                    for (int i = 0; i < sourceDBC.Length; i++)
                    {
                        targetTR_DBC_GO[i] = FindChild(target, sourceDBC[i].name);
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUI.EndDisabledGroup();
        }
        private void AttachComponent()
        {
            EditorGUILayout.LabelField("Attatch Components", EditorStyles.boldLabel);
            if (!ofNotNull)
            {
                EditorGUILayout.HelpBox("Source と Target を設定してください。", MessageType.Info);
                return;
            }
            if (sourceDB.Length == 0 && sourceDBC.Length == 0)
            {
                EditorGUILayout.HelpBox("Source に DynamicBone 及び DynamicBoneCollider が存在しません。", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("target が未指定のコンポーネントはコピーされません。", MessageType.Info);
            scrollPosition1 = EditorGUILayout.BeginScrollView(scrollPosition1);
            foldDBC = EditorGUILayout.Foldout(foldDBC, "DynamicBoneCollider");
            if (foldDBC)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Source", GUILayout.Width(width2));
                    EditorGUILayout.LabelField("Target", GUILayout.Width(width2));
                }
                for (int i = 0; i < sourceDBC.Length; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(sourceDBC[i].transform, typeof(Transform), true, GUILayout.Width(width2));
                        targetTR_DBC_GO[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DBC_GO[i] == null ? null : targetTR_DBC_GO[i].transform, typeof(Transform), true, GUILayout.Width(width2));
                    }
                }
            }
            foldDB = EditorGUILayout.Foldout(foldDB, "DynamicBone");
            if (foldDB)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Source", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Source RootBone", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Target", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Target RootBone", GUILayout.Width(width4));
                }
                for (int i = 0; i < sourceDB.Length; i++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(sourceDB[i].transform, typeof(Transform), true, GUILayout.Width(width4));
                        EditorGUILayout.ObjectField(sourceDB[i].m_Root == null ? null : sourceDB[i].m_Root.transform, typeof(Transform), true, GUILayout.Width(width4));
                        targetTR_DB_GO[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DB_GO[i] == null ? null : targetTR_DB_GO[i].transform, typeof(Transform), true, GUILayout.Width(width4));
                        targetTR_DB_RB[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DB_RB[i] == null ? null : targetTR_DB_RB[i].transform, typeof(Transform), true, GUILayout.Width(width4));
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            if (GUILayout.Button("Copy DynamicBone & Goto Collider/Execlusion Settings."))
            {
                targetDB = new DynamicBone[] { };
                targetDBC = new DynamicBoneCollider[] { };
                for (int i = 0; i < sourceDB.Length; i++)
                {
                    if (targetTR_DB_GO[i] != null && targetTR_DB_RB[i] != null)
                    {
                        DynamicBone db = CopyComponent(sourceDB[i], targetTR_DB_GO[i]);
                        db.m_Colliders = new List<DynamicBoneColliderBase>();
                        db.m_Exclusions = new List<Transform>();
                        db.m_Root = targetTR_DB_RB[i];
                        targetDB = targetDB.Concat(new DynamicBone[] { db }).ToArray();
                    }
                }
                for (int i = 0; i < sourceDBC.Length; i++)
                {
                    if (targetTR_DBC_GO[i] != null)
                    {
                        DynamicBoneCollider dbc = CopyComponent(sourceDBC[i], targetTR_DBC_GO[i]);
                        targetDBC = targetDBC.Concat(new DynamicBoneCollider[] { dbc }).ToArray();
                    }
                }
                page = 2;
                count = 0;
            }
        }
        private void ColliderAndExclusion()
        {
            //Page Init
            if (count == 0)
            {
                sourceColliders = new DynamicBoneCollider[][] { };
                targetColliders = new DynamicBoneCollider[][] { };
                sourceExclusion = new Transform[][] { };
                targetExclusion = new Transform[][] { };
                count = 1;

                for (int i = 0; i < sourceDB.Length; i++)
                {
                    DynamicBoneCollider[] dbcsS = new DynamicBoneCollider[] { };
                    Transform[] trsS = new Transform[] { };
                    DynamicBoneCollider[] dbcsT = new DynamicBoneCollider[] { };
                    Transform[] trsT = new Transform[] { };
                    if (sourceDB[i].m_Colliders != null)
                    {
                        foreach (DynamicBoneCollider dbc in sourceDB[i].m_Colliders)
                        {
                            dbcsS = dbcsS.Concat(new DynamicBoneCollider[] { dbc }).ToArray();
                            DynamicBoneCollider dbcN = null;
                            if (FindChild(target, dbc.name) != null)
                            {
                                dbcN = FindChild(target, dbc.name).GetComponent<DynamicBoneCollider>();
                            }
                            dbcsT = dbcsT.Concat(new DynamicBoneCollider[] { dbcN }).ToArray();
                        }
                    }
                    if (sourceDB[i].m_Exclusions != null)
                    {
                        foreach (Transform tr in sourceDB[i].m_Exclusions)
                        {
                            trsS = trsS.Concat(new Transform[] { tr }).ToArray();
                            Transform trN = FindChild(target, tr.name);
                            trsT = trsT.Concat(new Transform[] { trN }).ToArray();
                        }
                    }
                    sourceColliders = sourceColliders.Concat(new DynamicBoneCollider[][] { dbcsS }).ToArray();
                    sourceExclusion = sourceExclusion.Concat(new Transform[][] { trsS }).ToArray();
                    targetColliders = targetColliders.Concat(new DynamicBoneCollider[][] { dbcsT }).ToArray();
                    targetExclusion = targetExclusion.Concat(new Transform[][] { trsT }).ToArray();
                }
            }

            if (targetDB != null)
            {
                //Display Category
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Source Colliders", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Target Colliders", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Source Exclusion", GUILayout.Width(width4));
                    EditorGUILayout.LabelField("Target Exclusion", GUILayout.Width(width4));
                }
                scrollPosition2 = EditorGUILayout.BeginScrollView(scrollPosition2);
                for (int i = 0; i < targetDB.Length; i++)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Object:" + targetDB[i].transform.name + " /RootBone: " + targetDB[i].m_Root.name, EditorStyles.boldLabel);
                    using (new GUILayout.HorizontalScope())
                    {
                        using (new GUILayout.VerticalScope())
                        {
                            if (sourceColliders[i].Length == 0)
                            {
                                EditorGUILayout.LabelField("None", GUILayout.Width(width4));
                            }
                            else
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                {
                                    for (int j = 0; j < sourceColliders[i].Length; j++)
                                    {
                                        EditorGUILayout.ObjectField(sourceColliders[i][j], typeof(DynamicBoneCollider), true, GUILayout.Width(width4));
                                    }
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        using (new GUILayout.VerticalScope())
                        {
                            if (sourceColliders[i].Length == 0)
                            {
                                EditorGUILayout.LabelField("None", GUILayout.Width(width4));
                            }
                            else
                            {
                                for (int j = 0; j < sourceColliders[i].Length; j++)
                                {
                                    targetColliders[i][j] = (DynamicBoneCollider)EditorGUILayout.ObjectField(targetColliders[i][j], typeof(DynamicBoneCollider), true, GUILayout.Width(width4));
                                }
                            }
                        }
                        using (new GUILayout.VerticalScope())
                        {
                            if (sourceExclusion[i].Length == 0)
                            {
                                EditorGUILayout.LabelField("None", GUILayout.Width(width4));
                            }
                            else
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                {
                                    for (int j = 0; j < sourceExclusion[i].Length; j++)
                                    {
                                        EditorGUILayout.ObjectField(sourceExclusion[i][j], typeof(Transform), true, GUILayout.Width(width4));
                                    }
                                }
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        using (new GUILayout.VerticalScope())
                        {
                            if (sourceExclusion[i].Length == 0)
                            {
                                EditorGUILayout.LabelField("None", GUILayout.Width(width4));
                            }
                            else
                            {
                                for (int j = 0; j < sourceExclusion[i].Length; j++)
                                {
                                    targetExclusion[i][j] = (Transform)EditorGUILayout.ObjectField(targetExclusion[i][j], typeof(Transform), true, GUILayout.Width(width4));
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("←Back to Previous"))
                {
                    if (EditorUtility.DisplayDialog("CopyDynamicBones", "変更を破棄して戻りますか？", "OK", "Cancel"))
                    {
                        foreach (DynamicBone db in targetDB)
                        {
                            DestroyImmediate(db);
                        }
                        foreach (DynamicBoneCollider dbc in targetDBC)
                        {
                            DestroyImmediate(dbc);
                        }
                        page = 1;
                        ofNotNull = false;
                        source = null;
                        target = null;
                        sourceDB = null;
                        sourceDBC = null;
                        targetTR_DB_GO = null;
                        targetTR_DB_RB = null;
                        targetTR_DBC_GO = null;
                    }
                }
                if (GUILayout.Button("Set Collider&Execlusions."))
                {
                    for (int i = 0; i < targetDB.Length; i++)
                    {
                        if (targetColliders[i].Length > 0)
                        {
                            targetDB[i].m_Colliders = new List<DynamicBoneColliderBase>();
                            for (int j = 0; j < targetColliders[i].Length; j++)
                            {
                                targetDB[i].m_Colliders.Add(targetColliders[i][j]);
                            }
                        }
                        if (targetExclusion[i].Length > 0)
                        {
                            targetDB[i].m_Exclusions = new List<Transform>();
                            for (int j = 0; j < targetExclusion[i].Length; j++)
                            {
                                targetDB[i].m_Exclusions.Add(targetExclusion[i][j]);
                            }
                        }
                    }
                    page = 1;
                    ofNotNull = false;
                    source = null;
                    target = null;
                    sourceDB = null;
                    sourceDBC = null;
                    targetTR_DB_GO = null;
                    targetTR_DB_RB = null;
                    targetTR_DBC_GO = null;

                    _ = EditorUtility.DisplayDialog("Complete", "設定が完了しました。", "閉じる");
                }
            }
        }
        private Transform FindChild(Transform targetRoot, string name)
        {
            Transform[] temp = new Transform[] { targetRoot };
            Transform[] list = new Transform[] { };
            while (temp.Length > 0)
            {
                Transform[] childs = temp[0].GetComponentsInChildren<Transform>(true);
                temp = temp.Concat(childs).ToArray();
                list = list.Concat(childs).ToArray();
                temp = temp.Except(new Transform[] { temp[0] }).ToArray();
            }
            foreach (Transform tr in list)
            {
                string name1 = tr.name.StartsWith(targetPrefix) ? tr.name.Substring(targetPrefix.Length) : tr.name;
                string name2 = name1.EndsWith(targetSuffix) ? name1.Substring(0, name1.Length - targetSuffix.Length) : name1;

                string name3 = name.StartsWith(sourcePrefix) ? name.Substring(sourcePrefix.Length) : name;
                string name4 = name3.EndsWith(sourceSuffix) ? name3.Substring(0, name3.Length - sourceSuffix.Length) : name3;

                if (name2 == name4)
                {
                    return tr;
                }
                if (tr.name == name)
                {
                    return tr;
                }
            }
            return null;
        }
        private static T CopyComponent<T>(T original, Transform destination) where T : Component
        {
            System.Type type = original.GetType();
            T dst = destination.gameObject.AddComponent<T>();
            IEnumerable<FieldInfo> fields = GetAllFields(type);
            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic || field.Name == "m_CachedPtr" || field.Name == "m_InstanceID" || field.Name == "m_UnityRuntimeErrorString") continue;
                field.SetValue(dst, field.GetValue(original));
            }

            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite || !prop.CanRead || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }

            return dst as T;
        }
        private static IEnumerable<FieldInfo> GetAllFields(System.Type t)
        {
            if (t == null)
            {
                return Enumerable.Empty<FieldInfo>();
            }

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
        }
    }
}