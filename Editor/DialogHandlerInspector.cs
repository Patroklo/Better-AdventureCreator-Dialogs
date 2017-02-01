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

        SerializedProperty SelectedFile;

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
            SelectedFile = serializedObject.FindProperty("SelectedFile");

        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            GUILayout.Label("Dialogs Menu:");

            bool guiState = GUI.enabled;

            GUI.enabled = (LoadedDialogue.boolValue == true) ? false : true;

            SelectedDialogIndex.intValue = EditorGUILayout.Popup("Select dialog: ", Handler.SelectedDialogIndex, Handler.FileList.ToArray());

            if (Handler.FileList.Count == 0)
            {
                EditorGUILayout.HelpBox("All dialogs are currently assigned.", MessageType.Info);
            }

            if (Handler.FileList.Count != 0)
            {
                SelectedFile.stringValue = Handler.FileList[Handler.SelectedDialogIndex];
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply dialog"))
            {
                db.Load(Handler.SelectedFile);

                // Delete old dialogue data if it exists.
                DeleteDialogueData();

                // Add new dialogue data
                CreateDialogueData();

                LoadedDialogue.boolValue = true;
            }

            GUI.enabled = !GUI.enabled;

            if (GUILayout.Button("Remove dialog"))
            {
                db.Load(Handler.SelectedFile);

                // Delete old dialogue data if it exists.
                DeleteDialogueData();

                LoadedDialogue.boolValue = false;
                SelectedDialogIndex.intValue = 0;
                SelectedFile.stringValue = "";
            }
            
            GUILayout.EndHorizontal();
            GUI.enabled = guiState;

            serializedObject.ApplyModifiedProperties();
        }


        protected virtual Transform FindParentTransform()
        {
            return ((DialogHandler)target).gameObject.transform.FindChild("_dialogData");
        }



        protected virtual void DeleteDialogueData()
        {
            Transform destroyTransform = FindParentTransform();

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

            // Creates the game objects with their respective scripts for dialogues
            ProcessNode(firstNode);

            // Connects all the newly created objects
            ConnectNodeObjects();
        }

        protected virtual void ProcessNode(AbstractNode node)
        {
            Transform parentTransform = FindParentTransform();

            GameObject newObject = MakeGameObject(node.UniqueID, parentTransform);

            node.InstallNode(newObject);

            foreach (string childUniqueID in node.GetChildNodes())
            {
                ProcessNode(db.GetNodeByUniqueID(childUniqueID));
            }
        }

        protected virtual void ConnectNodeObjects()
        {
            List<AbstractNode> seedList = db.GetNodesByType(typeof(DialogSeed));

            Transform parentTransform = FindParentTransform();

            foreach (AbstractNode seed in seedList)
            {
                seed.MakeConnections(parentTransform.FindChild(seed.UniqueID).gameObject);
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