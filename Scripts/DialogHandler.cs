using System;
using System.Collections.Generic;
using UnityEngine;


namespace Dialogs
{
    [ExecuteInEditMode]
    public class DialogHandler : MonoBehaviour
    {
        [HideInInspector]
        public string SelectedFile = "";

        [HideInInspector]
        public int SelectedDialogIndex = 0;

        [HideInInspector]
        public List<string> FileList;


        [HideInInspector]
        public bool LoadedDialogue = false;


        protected Dialog_EditorDB db;


        void Start()
        {
            InitializeDB();
        }

        /// <summary>
        /// Creates the nodes and gameobjects neccessary to add a new dialog in the game
        /// </summary>
        public void ApplyDialog()
        {
            db.Load(SelectedFile);

            // Delete old dialogue data if it exists.
            DeleteDialogueData();

            // Add new dialogue data
            CreateDialogueData();

            LoadedDialogue = true;
        }

        
        /// <summary>
        /// Removes the loaded dialog and all its nodes and gameObjects
        /// </summary>
        public void RemoveDialog()
        {
            // Delete old dialogue data if it exists.
            DeleteDialogueData();

            LoadedDialogue = false;
            SelectedDialogIndex = 0;
            SelectedFile = "";
        }


        public AbstractNode GetFirstNode()
        {
            PropertiesNode baseProperty = (PropertiesNode)db.GetNodeByType(typeof(PropertiesNode));

            String firstNodeUniqueID = baseProperty.firstNode;

            return db.GetNodeByUniqueID(firstNodeUniqueID);
        }

        public List<AbstractNode> GetNodesByType(System.Type NodeType)
        {
            return db.GetNodesByType(NodeType);
        }


        protected virtual Transform FindParentTransform()
        {
            return gameObject.transform.FindChild("_dialogData");
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
            GameObject parentObject = gameObject;

            // Add the dialogs parent folder
            if (!parentObject.transform.FindChild("_dialogData"))
            {
                MakeGameObject("_dialogData", parentObject.transform);
            }

            AbstractNode firstNode = GetFirstNode();

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
                Dialog_EditorDB database = gameObject.GetComponent<Dialog_EditorDB>();

                if (database == null)
                {
                    gameObject.AddComponent<Dialog_EditorDB>();
                    database = gameObject.GetComponent<Dialog_EditorDB>();
                }

                db = database;
            }
        }


    }

}