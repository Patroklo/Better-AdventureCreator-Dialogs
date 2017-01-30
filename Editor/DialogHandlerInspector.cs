using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Dialogs
{

    [CustomEditor(typeof(DialogHandler))]
    public class DialogHandlerInspector : Editor
    {
        protected DialogHandler Handler;

        protected Dialog_EditorDB db;

        SerializedProperty LoadedDialogue;
        SerializedProperty SelectedDialogIndex;


        void OnEnable()
        {

            Handler = (DialogHandler)target;

            InitializeDB();

            Handler.FileList = FileManager.LoadFiles().Clone<string>();

            DialogHandler[] handlerList = FindObjectsOfType<DialogHandler>();

            foreach (var _handler in handlerList)
            {
                if (_handler.gameObject.GetInstanceID() != ((DialogHandler)target).gameObject.GetInstanceID() && _handler.LoadedDialogue == true)
                {
                    int fileIndex = _handler.FileList.FindIndex(a => a == _handler.SelectedFile);
                    if (fileIndex != -1)
                    {
                        Handler.FileList.RemoveAt(fileIndex);
                    }
                }
            }


            LoadedDialogue = serializedObject.FindProperty("LoadedDialogue");
            SelectedDialogIndex = serializedObject.FindProperty("SelectedDialogIndex");
        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            GUILayout.Label("Dialogs Menu:");

            SelectedDialogIndex.intValue = EditorGUILayout.Popup("Select dialog: ", Handler.SelectedDialogIndex, Handler.FileList.ToArray());

            if (Handler.FileList.Count == 0)
            {
                EditorGUILayout.HelpBox("All dialogs are currently assigned.", MessageType.Info);
            }

            if (Handler.FileList.Count != 0)
            {
                Handler.SelectedFile = Handler.FileList[Handler.SelectedDialogIndex];
            }

            if (GUILayout.Button("Apply dialog into scenario"))
            {
                db.Load(Handler.SelectedFile);

                // Delete old dialogue data if it exists.
                DeleteDialogueData();

                // Add new dialogue data
                CreateDialogueData();

                LoadedDialogue.boolValue = true;
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DeleteDialogueData()
        {
            Transform destroyTransform = ((DialogHandler)target).gameObject.transform.FindChild("_dialogData");

            if (destroyTransform != null)
            {
                UnityEngine.GameObject.DestroyImmediate(destroyTransform.gameObject);
            }
        }


        /// <summary>
        /// Will insert dialogues, speeches, etc... as childs of the current object.
        /// </summary>
        protected virtual void CreateDialogueData()
        {
            GameObject parentObject = ((DialogHandler)target).gameObject;

            // Add the dialogs parent folder
            if (!parentObject.transform.FindChild("_dialogData"))
            {
                MakeGameObject("_dialogData", parentObject.transform);
            }

            PropertiesNode baseProperty = (PropertiesNode)db.GetNodeByType(typeof(PropertiesNode));

            String firstNodeUniqueID = baseProperty.firstNode;

            AbstractNode firstNode = db.GetNodeByUniqueID(firstNodeUniqueID);

            ProcessNode(firstNode);
        }

        protected virtual void ProcessNode(AbstractNode node)
        {
            Transform parentTransform = ((DialogHandler)target).gameObject.transform.FindChild("_dialogData");

            GameObject newObject = MakeGameObject(node.UniqueID, parentTransform);

            node.InstallNode(newObject);

            foreach (string childUniqueID in node.ChildNodes())
            {
                ProcessNode(db.GetNodeByUniqueID(childUniqueID));
            }
        }

        protected virtual GameObject MakeGameObject(string Name, Transform parentTransform)
        {
            GameObject newGameObject = new GameObject();

            newGameObject.name = Name;

            newGameObject.transform.parent = parentTransform.transform;

            return newGameObject;
        }


        protected virtual void InitializeDB()
        {
            if (db == null)
            {
                Dialog_EditorDB database = ((DialogHandler)target).gameObject.GetComponent<Dialog_EditorDB>();

                if (database == null)
                {
                    ((DialogHandler)target).gameObject.AddComponent<Dialog_EditorDB>();
                    database = ((DialogHandler)target).gameObject.GetComponent<Dialog_EditorDB>();
                }

                db = database;
            }
        }

    }
}