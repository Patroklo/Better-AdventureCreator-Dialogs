using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Dialogs
{
    [Serializable]
    public class PropertiesNode : AbstractNode
    {

        protected delegate void newNodeDelegate(int NodeTypeID);
        newNodeDelegate newNode;

        public string firstNode;

        public PropertiesNode()
        {
            Init();
        }


        protected void Init()
        {
            Title = "Properties";

            isPublic = false;
        }


#if UNITY_EDITOR

        public new int widthSize = 250;
        public new int heightSize = 200;

        public string LoadedFile;


        int SelectedNodeType = 0;
        public int SelectedDialogIndex = 0;
        public bool HasLoadedDialog = false;

        public PropertiesNode(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
            newNode = addNewNode;
            HasLoadedDialog = false;
        }

        protected void addNewNode(int NodeTypeId)
        {
            db.AddNode(NodeTypeId);
        }

        public override void OnGUI(int WindowID)
        {
            GUILayout.BeginVertical();

            List<string> fileList = FileManager.LoadFiles();

            string selectedFile = "";

            GUILayout.Label("Dialogs Menu:");

            SelectedDialogIndex = EditorGUILayout.Popup("Select dialog: ", SelectedDialogIndex, fileList.ToArray());

            if (fileList.Count != 0)
            {
                selectedFile = fileList[SelectedDialogIndex];
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New"))
            {
                db.AddNode(db.GetLoadedNodeKey(typeof(NewDialog)));
            }

            if (String.IsNullOrEmpty(firstNode) || HasLoadedDialog == false)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Save"))
            {
                db.Save(selectedFile);
            }

            GUI.enabled = true;

            if (String.IsNullOrEmpty(selectedFile))
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Load"))
            {
                db.Load(selectedFile);
            }

            GUI.enabled = true;

            if (selectedFile == "")
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Delete"))
            {
                bool unloadDialog = false;

                if (selectedFile == this.LoadedFile)
                {
                    unloadDialog = true;
                }

                db.Delete(selectedFile, unloadDialog);
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();

            if (HasLoadedDialog == false)
            {
                EditorGUILayout.HelpBox("There's no loaded dialog to be saved. Create a new one or load a dialog.", MessageType.Error);
            }
            else if (String.IsNullOrEmpty(firstNode))
            {
                EditorGUILayout.HelpBox("You must declare an initial node before saving the dialog.", MessageType.Error);
            }

            GUILayout.Label("Add new nodes:");

            Dictionary<int, string> publicNodeList = db.GetPublicNodeList();
            int[] publicNodeKeyList = publicNodeList.Keys.ToArray();

            SelectedNodeType = EditorGUILayout.IntPopup("Select node type: ", SelectedNodeType, publicNodeList.Values.ToArray(), publicNodeKeyList);

            if (GUILayout.Button("Add Node"))
            {
                newNode(SelectedNodeType);
            }

            GUILayout.EndVertical();

            GUI.DragWindow();
        }

        public override void RightClickMenu()
        { }


        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            return false;
        }

#endif

    }
}
