using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Dialogs
{
    [Serializable]
    class NewDialog : AbstractNode
    {
        [SerializeField]
        protected String FileDialogName = "";

        public NewDialog()
        {
            Init();
        }

        protected void Init()
        {
            Title = "New Dialog name";
            isPublic = false;
        }


#if UNITY_EDITOR


        public NewDialog(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
        }

        public override void OnGUI(int WindowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Insert the Dialog file name without extension.");
            FileDialogName = GUILayout.TextArea(FileDialogName);

            List<string> fileList = FileManager.LoadFiles();

            if (GUILayout.Button("Save"))
            {
                if (FileDialogName.Trim() == "")
                {
                    EditorUtility.DisplayDialog("Empty filename", "The filename must not be empty.", "Ok");
                }
                else if (fileList.FindIndex(a => a == FileDialogName) != -1)
                {
                    EditorUtility.DisplayDialog("Filename already exists", "The filename already exists in the dialog list.", "Ok");
                }
                else
                {
                    db.New(FileDialogName);
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        /// <summary>
        /// Basic menu that appears when right clicking. In this case, a delete menu option
        /// </summary>
        public override void RightClickMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Close"), false, DeleteCallback);
            menu.ShowAsContext();
        }

        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            return false;
        }
        
#endif

    }
}
