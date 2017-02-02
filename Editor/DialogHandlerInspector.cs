using UnityEditor;
using UnityEngine;

namespace Dialogs
{

    [CustomEditor(typeof(DialogHandler))]
    public class DialogHandlerInspector : Editor
    {
        protected DialogHandler Handler;

        SerializedProperty LoadedDialogue;
        SerializedProperty SelectedDialogIndex;

        SerializedProperty SelectedFile;

        void OnEnable()
        {

            Handler = (DialogHandler)target;

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
                Handler.ApplyDialog();
            }

            GUI.enabled = !GUI.enabled;

            if (GUILayout.Button("Remove dialog"))
            {
               Handler.RemoveDialog();
            }

            GUILayout.EndHorizontal();
            GUI.enabled = guiState;

            serializedObject.ApplyModifiedProperties();
        }

    }
}