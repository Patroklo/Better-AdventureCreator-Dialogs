using System;
using UnityEngine;
using AC;
using UnityEditor;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Dialogs
{

    [Serializable]
    public class SerializableCharacter
    {
        [NonSerialized]
        protected AC.Char character = null;

        [SerializeField]
        protected int? GameObjectID = null;

        public AC.Char Character
        {
            get
            {
                return character;
            }

            set
            {
                character = value;

                if (value != null)
                {
                    GameObjectID = character.gameObject.GetInstanceID();
                }
            }
        }

        public SerializableCharacter()
        {
            character = null;
        }

        /// <summary>
        /// Called when deserializing, loads the Speaker object
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (GameObjectID != null)
            {
                GameObject characterObject = (GameObject)EditorUtility.InstanceIDToObject((int)GameObjectID);

                if (characterObject == null)
                {
                    Debug.LogError("Character game object doesn't exist in the loaded scene.");
                }
                else
                {
                    this.Character = characterObject.GetComponent<NPC>();
                }
            }
        }
    }


    [Serializable]
    public class Speech : AbstractNode
    {

        [SerializeField]
        public bool PlayerLine;

        // [NonSerialized]
        // public AC.Char Speaker;

        [SerializeField]
        public SerializableCharacter Speaker = new SerializableCharacter();

        [SerializeField]
        public string SpeechText;

        [SerializeField]
        public bool NotAnimateSpeaker;

        [SerializeField]
        public bool PlayBackground;

        [SerializeField]
        public float Offset;

        [SerializeField]
        public int parameterID = -1;

        public Speech()
        {
            Init();
        }

        protected void Init()
        {
            Title = "Speech text";

            isPublic = true;
        }
        protected List<string> ChildNodes = new List<string>();

        public override void InstallNode(GameObject workingNode)
        {
            List<AbstractNode> childNodeList = RecursiveCheckChildren(this, workingNode);
            
            ChildNodes = new List<string>();
            
            foreach (AbstractNode child in childNodeList)
            {
                ChildNodes.Add(child.UniqueID);
            }
        }

        protected List<AbstractNode> RecursiveCheckChildren(Speech node, GameObject workingNode)
        {
            List<AbstractNode> returnList = new List<AbstractNode>();

            AC.DialogueOption dialogueOption = CheckDialogueOption(workingNode);

            AddSpeechToDialog(node, dialogueOption);

            foreach (int childKey in node.GetActiveConnections().Keys)
            {
                AbstractNode childNode = db.GetNodeByUniqueID(ActiveConnections[childKey]);

                if (childNode.GetType() == typeof(Speech))
                {
                    returnList.AddRange(RecursiveCheckChildren((Speech)childNode, workingNode));
                }
                else if (childNode.GetType() == typeof(DialogSeed))
                {
                    returnList.Add(childNode);
                }
            }
            return returnList;
        }



        protected AC.DialogueOption CheckDialogueOption(GameObject workingNode)
        {
            AC.DialogueOption dialogueOption = workingNode.GetComponent<AC.DialogueOption>();

            if (dialogueOption == null)
            {
                workingNode.AddComponent<AC.DialogueOption>();
                dialogueOption = workingNode.GetComponent<AC.DialogueOption>();
                dialogueOption.actions = new List<AC.Action>();
            }

            return dialogueOption;
        }

        public override List<string> GetChildNodes()
        {
            return ChildNodes;
        }


        private void AddSpeechToDialog(Speech node, AC.DialogueOption dialogueOption)
        {
            AC.ActionSpeech newSpeech = ScriptableObject.CreateInstance<AC.ActionSpeech>();

            newSpeech.isPlayer = node.PlayerLine;
            newSpeech.speaker = node.Speaker.Character;
            newSpeech.messageText = node.SpeechText;
            newSpeech.noAnimation = node.NotAnimateSpeaker;
            newSpeech.isBackground = node.PlayBackground;
            newSpeech.waitTimeOffset = node.Offset;

            dialogueOption.actions.Add(newSpeech);
        }

#if UNITY_EDITOR

        public new int widthSize = 250;
        public new int heightSize = 200;

        public Speech(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
        }

        public override void OnGUI(int WindowID)
        {

            PlayerLine = EditorGUILayout.Toggle("Player line?", PlayerLine);

            if (!PlayerLine)
            {
                if (Application.isPlaying)
                {
                    if (Speaker.Character)
                    {
                        EditorGUILayout.LabelField("Speaker: " + Speaker.Character.name);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("The speaker cannot be assigned while the game is running.", MessageType.Info);
                    }
                }
                else
                {
                    Speaker.Character = (AC.Char)EditorGUILayout.ObjectField("Speaker:", Speaker.Character, typeof(AC.Char), true);
                }
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Line text:", GUILayout.Width(65f));
            EditorStyles.textField.wordWrap = true;
            SpeechText = EditorGUILayout.TextArea(SpeechText, GUILayout.MaxWidth(400f));
            EditorGUILayout.EndHorizontal();

            NotAnimateSpeaker = EditorGUILayout.Toggle("Don't animate speaker?", NotAnimateSpeaker);

            PlayBackground = EditorGUILayout.Toggle("Play in background?", PlayBackground);

            Offset = EditorGUILayout.Slider("Wait time offset (s):", Offset, -1f, 4f);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            AddConnectionButton(0);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }


        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            if (NodeTarget.GetType() == typeof(DialogSeed)
                || NodeTarget.GetType() == typeof(Speech))
            {
                return true;
            }

            return false;
        }


        public override void CreateAutomaticNode()
        {
            NewAutomaticNode(typeof(Speech));
        }

#endif

    }
}
