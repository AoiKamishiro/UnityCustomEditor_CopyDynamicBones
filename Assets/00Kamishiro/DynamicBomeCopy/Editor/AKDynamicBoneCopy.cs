/*
 *  MIT License

Copyright (c) 2020 AoiKamishiro

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

 *     
 */

//LastUpdate:2020/08/25(JST) 

using System.Linq;
using UnityEditor;
using UnityEngine;

public class AKDynamicBoneCopy : EditorWindow
{
    [MenuItem("Tools/Kamishiro/DynamicBoneCopy", priority = 150)]
    private static void OnEnable()
    {
        AKDynamicBoneCopy window = GetWindow<AKDynamicBoneCopy>("DynamicBoneCopy");
        window.minSize = new Vector2(640, 400);
        window.Show();
    }

    //GUI Component
    private const string version = " DynamicBoneCopy V1.0 by 神城葵";
    private const string linktext = "操作説明等はこちら";
    private const string link = "https://github.com/AoiKamishiro/UnityCustomEditor_DynamicBoneCopy";

    private int page = 1;

    private bool foldDBC = false;
    private bool foldDB = false;
    private bool ofNotNull = false;
    private Transform source = null;
    private Transform target = null;
    private DynamicBone[] sourceDB = null;
    private DynamicBoneCollider[] sourceDBC = null;
    private Transform[] targetTR_DB_GO = null;
    private Transform[] targetTR_DB_RB = null;
    private Transform[] targetTR_DBC_GO = null;
    private DynamicBone[] targetDB = null;
    private DynamicBoneCollider[][] sourceColliders = null;
    private Transform[][] sourceExclusion = null;
    private DynamicBoneCollider[][] targetColliders = null;
    private Transform[][] targetExclusion = null;
    private Vector2 scrollPosition1 = Vector2.zero;
    private Vector2 scrollPosition2 = Vector2.zero;
    private int count = 0;
    private bool fold = false;

    private void OnGUI()
    {
        //Editor Window
        using (new GUILayout.VerticalScope())
        {

            EditorGUILayout.LabelField(version);
            if (GUILayout.Button(linktext)) { OpenLink(link); }
            EditorGUILayout.Space();
            if (page == 1)
            {
                EditorGUI.BeginChangeCheck();
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Source");
                        source = (Transform)EditorGUILayout.ObjectField(source, typeof(Transform), true);
                        EditorGUI.indentLevel--;
                    }
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Target");
                        target = (Transform)EditorGUILayout.ObjectField(target, typeof(Transform), true);
                        EditorGUI.indentLevel--;
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
                            targetTR_DB_RB[i] = FindChild(target, sourceDB[i].m_Root.name);
                        }
                        for (int i = 0; i < sourceDBC.Length; i++)
                        {
                            targetTR_DBC_GO[i] = FindChild(target, sourceDBC[i].name);
                        }
                    }
                }
                EditorGUILayout.Space();
                if (!ofNotNull)
                {
                    GUILayout.Label("Set Target & Source.");
                }
                else
                {
                    if (sourceDB.Length == 0 && sourceDBC.Length == 0)
                    {
                        GUILayout.Label("There is no Dynamic Bone Components");
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("target が未指定のコンポーネントはコピーされません。", MessageType.Info, false);
                        scrollPosition1 = EditorGUILayout.BeginScrollView(scrollPosition1);
                        EditorGUI.indentLevel++;
                        foldDBC = EditorGUILayout.Foldout(foldDBC, "DynamicBoneCollider");
                        if (foldDBC)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField("Source");
                                EditorGUI.indentLevel--;
                                EditorGUILayout.LabelField("Target");
                            }
                            for (int i = 0; i < sourceDBC.Length; i++)
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.ObjectField(sourceDBC[i].transform, typeof(Transform), true);
                                    EditorGUI.indentLevel--;
                                    targetTR_DBC_GO[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DBC_GO[i] == null ? null : targetTR_DBC_GO[i].transform, typeof(Transform), true);
                                }
                            }
                        }
                        foldDB = EditorGUILayout.Foldout(foldDB, "DynamicBone");
                        if (foldDB)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField("Source");
                                EditorGUI.indentLevel--;
                                EditorGUI.indentLevel--;
                                EditorGUILayout.LabelField("Source RootBone");
                                EditorGUI.indentLevel++;
                                EditorGUILayout.LabelField("Target");
                                EditorGUI.indentLevel--;
                                EditorGUILayout.LabelField("Target RootBone");
                                EditorGUI.indentLevel++;
                            }
                            for (int i = 0; i < sourceDB.Length; i++)
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.ObjectField(sourceDB[i].transform, typeof(Transform), true);
                                    EditorGUI.indentLevel--;
                                    EditorGUI.indentLevel--;
                                    EditorGUILayout.ObjectField(sourceDB[i].m_Root.transform, typeof(Transform), true);
                                    EditorGUI.indentLevel++;
                                    targetTR_DB_GO[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DB_GO[i] == null ? null : targetTR_DB_GO[i].transform, typeof(Transform), true);
                                    EditorGUI.indentLevel--;
                                    targetTR_DB_RB[i] = (Transform)EditorGUILayout.ObjectField(targetTR_DB_RB[i] == null ? null : targetTR_DB_RB[i].transform, typeof(Transform), true);
                                    EditorGUI.indentLevel++;
                                }
                            }
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.EndScrollView();
                        if (GUILayout.Button("Copy DynamicBone & Goto Collider/Execlusion Settings."))
                        {
                            targetDB = new DynamicBone[] { };
                            for (int i = 0; i < sourceDB.Length; i++)
                            {
                                if (targetTR_DB_GO[i] != null && targetTR_DB_RB[i] != null)
                                {
                                    DynamicBone db = targetTR_DB_GO[i].gameObject.AddComponent<DynamicBone>();
                                    //db.m_Colliders = sourceDB[i].m_Colliders;
                                    db.m_Damping = sourceDB[i].m_Damping;
                                    db.m_DampingDistrib = sourceDB[i].m_DampingDistrib;
                                    db.m_DistanceToObject = sourceDB[i].m_DistanceToObject;
                                    db.m_DistantDisable = sourceDB[i].m_DistantDisable;
                                    db.m_Elasticity = sourceDB[i].m_Elasticity;
                                    db.m_ElasticityDistrib = sourceDB[i].m_ElasticityDistrib;
                                    db.m_EndLength = sourceDB[i].m_EndLength;
                                    db.m_EndOffset = sourceDB[i].m_EndOffset;
                                    //db.m_Exclusions = sourceDB[i].m_Exclusions;
                                    db.m_Force = sourceDB[i].m_Force;
                                    db.m_FreezeAxis = sourceDB[i].m_FreezeAxis;
                                    db.m_Friction = sourceDB[i].m_Friction;
                                    db.m_FrictionDistrib = sourceDB[i].m_FrictionDistrib;
                                    db.m_Gravity = sourceDB[i].m_Gravity;
                                    db.m_Inert = sourceDB[i].m_Inert;
                                    db.m_InertDistrib = sourceDB[i].m_InertDistrib;
                                    db.m_Radius = sourceDB[i].m_Radius;
                                    db.m_RadiusDistrib = sourceDB[i].m_RadiusDistrib;
                                    db.m_ReferenceObject = sourceDB[i].m_ReferenceObject;
                                    db.m_Stiffness = sourceDB[i].m_Stiffness;
                                    db.m_StiffnessDistrib = sourceDB[i].m_StiffnessDistrib;
                                    db.m_UpdateMode = sourceDB[i].m_UpdateMode;
                                    db.m_UpdateRate = sourceDB[i].m_UpdateRate;
                                    db.m_Root = targetTR_DB_RB[i];
                                    targetDB = targetDB.Concat(new DynamicBone[] { db }).ToArray();
                                }
                            }
                            for (int i = 0; i < sourceDBC.Length; i++)
                            {
                                if (targetTR_DBC_GO[i] != null)
                                {
                                    DynamicBoneCollider dbc = targetTR_DBC_GO[i].gameObject.AddComponent<DynamicBoneCollider>();
                                    dbc.m_Bound = sourceDBC[i].m_Bound;
                                    dbc.m_Center = sourceDBC[i].m_Center;
                                    dbc.m_Direction = sourceDBC[i].m_Direction;
                                    dbc.m_Height = sourceDBC[i].m_Height;
                                    dbc.m_Radius = sourceDBC[i].m_Radius;
                                }
                            }
                            page = 2;
                            count = 0;
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
            }
            else if (page == 2)
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
                        foreach (DynamicBoneCollider dbc in sourceDB[i].m_Colliders)
                        {
                            dbcsS = dbcsS.Concat(new DynamicBoneCollider[] { dbc }).ToArray();
                            DynamicBoneCollider dbcN = FindChild(target, dbc.name).GetComponent<DynamicBoneCollider>();
                            dbcsT = dbcsT.Concat(new DynamicBoneCollider[] { dbcN }).ToArray();
                        }
                        foreach (Transform tr in sourceDB[i].m_Exclusions)
                        {
                            trsS = trsS.Concat(new Transform[] { tr }).ToArray();
                            Transform trN = FindChild(target, tr.name);
                            trsT = trsT.Concat(new Transform[] { trN }).ToArray();
                        }
                        sourceColliders = sourceColliders.Concat(new DynamicBoneCollider[][] { dbcsS }).ToArray();
                        sourceExclusion = sourceExclusion.Concat(new Transform[][] { trsS }).ToArray();
                        targetColliders = targetColliders.Concat(new DynamicBoneCollider[][] { dbcsT }).ToArray();
                        targetExclusion = targetExclusion.Concat(new Transform[][] { trsT }).ToArray();
                    }
                }

                //DIsolay Source & Target
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Source");
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        source = (Transform)EditorGUILayout.ObjectField(source, typeof(Transform), true);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Target");
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        target = (Transform)EditorGUILayout.ObjectField(target, typeof(Transform), true);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();

                if (targetDB != null)
                {
                    //Display Category
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Source Colliders");
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                        EditorGUILayout.LabelField("Target Colliders");
                        EditorGUI.indentLevel++;
                        EditorGUILayout.LabelField("Source Exclusion");
                        EditorGUI.indentLevel--;
                        EditorGUILayout.LabelField("Target Exclusion");
                        EditorGUI.indentLevel++;
                    }
                    scrollPosition2 = EditorGUILayout.BeginScrollView(scrollPosition2);
                    {
                        for (int i = 0; i < targetDB.Length; i++)
                        {
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.LabelField("GameObject:" + targetDB[i].transform.name + " /RootBone: " + targetDB[i].m_Root.name, EditorStyles.boldLabel);
                            EditorGUI.indentLevel--;
                            using (new GUILayout.HorizontalScope())
                            {
                                //Source Colliders
                                EditorGUI.indentLevel++;
                                EditorGUI.indentLevel++;
                                using (new GUILayout.VerticalScope())
                                {
                                    if (sourceColliders[i].Length == 0)
                                    {
                                        EditorGUILayout.LabelField("None");
                                    }
                                    else
                                    {
                                        EditorGUI.BeginDisabledGroup(true);
                                        {
                                            for (int j = 0; j < sourceColliders[i].Length; j++)
                                            {
                                                EditorGUILayout.ObjectField(sourceColliders[i][j], typeof(DynamicBoneCollider), true);
                                            }
                                        }
                                        EditorGUI.EndDisabledGroup();
                                    }
                                }
                                EditorGUI.indentLevel--;
                                EditorGUI.indentLevel--;
                                using (new GUILayout.VerticalScope())
                                {
                                    if (sourceColliders[i].Length == 0)
                                    {
                                        EditorGUILayout.LabelField("None");
                                    }
                                    else
                                    {
                                        for (int j = 0; j < sourceColliders[i].Length; j++)
                                        {
                                            targetColliders[i][j] = (DynamicBoneCollider)EditorGUILayout.ObjectField(targetColliders[i][j], typeof(DynamicBoneCollider), true);
                                        }
                                    }
                                }
                                EditorGUI.indentLevel++;
                                using (new GUILayout.VerticalScope())
                                {
                                    if (sourceExclusion[i].Length == 0)
                                    {
                                        EditorGUILayout.LabelField("None");
                                    }
                                    else
                                    {
                                        EditorGUI.BeginDisabledGroup(true);
                                        {
                                            for (int j = 0; j < sourceExclusion[i].Length; j++)
                                            {
                                                EditorGUILayout.ObjectField(sourceExclusion[i][j], typeof(Transform), true);
                                            }
                                        }
                                        EditorGUI.EndDisabledGroup();
                                    }
                                }
                                EditorGUI.indentLevel--;
                                using (new GUILayout.VerticalScope())
                                {
                                    if (sourceExclusion[i].Length == 0)
                                    {
                                        EditorGUILayout.LabelField("None");
                                    }
                                    else
                                    {
                                        for (int j = 0; j < sourceExclusion[i].Length; j++)
                                        {
                                            targetExclusion[i][j] = (Transform)EditorGUILayout.ObjectField(targetExclusion[i][j], typeof(Transform), true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                if (GUILayout.Button("Set Collider&Execlusions."))
                {
                    for (int i = 0; i < targetDB.Length; i++)
                    {
                        if (targetColliders[i].Length > 0)
                        {
                            targetDB[i].m_Colliders = new System.Collections.Generic.List<DynamicBoneColliderBase>();
                            for (int j = 0; j < targetColliders[i].Length; j++)
                            {
                                targetDB[i].m_Colliders.Add(targetColliders[i][j]);
                            }
                        }
                        if (targetExclusion[i].Length > 0)
                        {
                            targetDB[i].m_Exclusions = new System.Collections.Generic.List<Transform>();
                            for (int j = 0; j < targetExclusion[i].Length; j++)
                            {
                                targetDB[i].m_Exclusions.Add(targetExclusion[i][j]);
                            }
                        }
                    }
                    page = 1;
                    source = null;
                    target = null;
                    sourceDB = null;
                    sourceDBC = null;
                    _ = EditorUtility.DisplayDialog("Complete", "設定が完了しました。", "閉じる");
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
        }
    }
    private Transform FindChild(Transform transform, string name)
    {
        Transform[] temp = new Transform[] { transform };
        Transform[] list = new Transform[] { };
        while (temp.Length > 0)
        {
            Transform[] childs = temp[0].GetComponentsInChildren<Transform>();
            temp = temp.Concat(childs).ToArray();
            list = list.Concat(childs).ToArray();
            temp = temp.Except(new Transform[] { temp[0] }).ToArray();
        }
        foreach (Transform tr in list)
        {
            if (tr.name == name)
            {
                return tr;
            }
        }
        return null;
    }
    public string GetPath(Transform self)
    {
        string path = self.gameObject.name;

        Transform parent = self.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
    private void OpenLink(string link)
    {
        Application.OpenURL(link);
    }
}
