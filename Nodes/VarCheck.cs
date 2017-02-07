using System;
using UnityEngine;
using AC;
using UnityEditor;
using System.Collections.Generic;

namespace Dialogs
{

    [Serializable]
    public class VarCheck : AbstractChecker, IChecker
    {

        public int parameterID = -1;
        public int variableID;
        public int variableNumber;

        public int checkParameterID = -1;

        public GetVarMethod getVarMethod = GetVarMethod.EnteredValue;
        public int compareVariableID;

        public int intValue;
        public float floatValue;
        public IntCondition intCondition;
        public bool isAdditive = false;

        public BoolValue boolValue = BoolValue.True;
        public BoolCondition boolCondition;

        public string stringValue;
        public bool checkCase = true;

        public VariableLocation location = VariableLocation.Global;
        private LocalVariables localVariables;

        List<ActionParameter> parameters = new List<ActionParameter>();


        public VarCheck()
        {
            Init();
        }

        protected void Init()
        {
            Title = "Check a variable";

            isPublic = true;
        }


        // Will check the assigned values and enable / disable the option buttons
        // asigned to this Node
        public void Check()
        {
            if (variableID == -1)
            {
                return;
            }

            GVar compareVar = null;

            if (getVarMethod == GetVarMethod.GlobalVariable || getVarMethod == GetVarMethod.LocalVariable)
            {
                if (compareVariableID == -1)
                {
                    return;
                }

                if (getVarMethod == GetVarMethod.GlobalVariable)
                {
                    compareVar = GlobalVariables.GetVariable(compareVariableID);
                    compareVar.Download();
                }
                else if (getVarMethod == GetVarMethod.LocalVariable)
                {
                    compareVar = LocalVariables.GetVariable(compareVariableID, localVariables);
                }
            }

            if (location == VariableLocation.Local)
            {
                GVar localVar = LocalVariables.GetVariable(variableID, localVariables);
                if (localVar != null)
                {
                    ProcessResult(CheckCondition(localVar, compareVar));
                    return;
                }

                ACDebug.LogWarning("The 'Variable: Check' Action halted the ActionList because it cannot find the Local Variable with an ID of " + variableID);
                return;
            }
            else
            {
                GVar var = GlobalVariables.GetVariable(variableID);
                if (var != null)
                {
                    var.Download();
                    ProcessResult(CheckCondition(var, compareVar));
                    return;
                }

                ACDebug.LogWarning("The 'Variable: Check' Action halted the ActionList because it cannot find the Global Variable with an ID of " + variableID);
                return;
            }
        }

        private bool CheckCondition(GVar _var, GVar _compareVar)
        {
            if (_var == null)
            {
                ACDebug.LogWarning("Cannot check state of variable since it cannot be found!");
                return false;
            }

            if (_compareVar != null && _var != null && _compareVar.type != _var.type)
            {
                ACDebug.LogWarning("Cannot compare " + _var.label + " and " + _compareVar.label + " as they are not the same type!");
                return false;
            }

            if (_var.type == VariableType.Boolean)
            {
                int fieldValue = _var.val;
                int compareValue = (int)boolValue;
                if (_compareVar != null)
                {
                    compareValue = _compareVar.val;
                }

                if (boolCondition == BoolCondition.EqualTo)
                {
                    if (fieldValue == compareValue)
                    {
                        return true;
                    }
                }
                else
                {
                    if (fieldValue != compareValue)
                    {
                        return true;
                    }
                }
            }

            else if (_var.type == VariableType.Integer || _var.type == VariableType.PopUp)
            {
                int fieldValue = _var.val;
                int compareValue = intValue;
                if (_compareVar != null)
                {
                    compareValue = _compareVar.val;
                }

                if (intCondition == IntCondition.EqualTo)
                {
                    if (fieldValue == compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.NotEqualTo)
                {
                    if (fieldValue != compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.LessThan)
                {
                    if (fieldValue < compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.MoreThan)
                {
                    if (fieldValue > compareValue)
                    {
                        return true;
                    }
                }
            }

            else if (_var.type == VariableType.Float)
            {
                float fieldValue = _var.floatVal;
                float compareValue = floatValue;
                if (_compareVar != null)
                {
                    compareValue = _compareVar.floatVal;
                }

                if (intCondition == IntCondition.EqualTo)
                {
                    if (fieldValue == compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.NotEqualTo)
                {
                    if (fieldValue != compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.LessThan)
                {
                    if (fieldValue < compareValue)
                    {
                        return true;
                    }
                }
                else if (intCondition == IntCondition.MoreThan)
                {
                    if (fieldValue > compareValue)
                    {
                        return true;
                    }
                }
            }

            else if (_var.type == VariableType.String)
            {
                string fieldValue = _var.textVal;
                string compareValue = AdvGame.ConvertTokens(stringValue);
                if (_compareVar != null)
                {
                    compareValue = _compareVar.textVal;
                }

                if (!checkCase)
                {
                    fieldValue = fieldValue.ToLower();
                    compareValue = compareValue.ToLower();
                }

                if (boolCondition == BoolCondition.EqualTo)
                {
                    if (fieldValue == compareValue)
                    {
                        return true;
                    }
                }
                else
                {
                    if (fieldValue != compareValue)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


#if UNITY_EDITOR

        public new int widthSize = 300;
        public new int heightSize = 150;


        public VarCheck(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
        }

        public override void OnGUI(int WindowID)
        {

            location = (VariableLocation)EditorGUILayout.EnumPopup("Source:", location);


            if (location == VariableLocation.Global)
            {
                if (AdvGame.GetReferences().variablesManager)
                {
                    VariablesManager variablesManager = AdvGame.GetReferences().variablesManager;

                    parameterID = AC.Action.ChooseParameterGUI("Variable:", parameters, parameterID, ParameterType.GlobalVariable);
                    if (parameterID >= 0)
                    {
                        variableID = ShowVarGUI(parameters, variablesManager.vars, variableID, false);
                    }
                    else
                    {
                        variableID = ShowVarGUI(parameters, variablesManager.vars, variableID, true);
                    }
                }
            }

            else if (location == VariableLocation.Local)
            {
                if (localVariables)
                {
                    parameterID = AC.Action.ChooseParameterGUI("Variable:", parameters, parameterID, ParameterType.LocalVariable);
                    if (parameterID >= 0)
                    {
                        variableID = ShowVarGUI(parameters, localVariables.localVars, variableID, false);
                    }
                    else
                    {
                        variableID = ShowVarGUI(parameters, localVariables.localVars, variableID, true);
                    }
                }
            }


            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("If condition is met:");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            AddConnectionButton(0);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("If condition is not met:");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            AddConnectionButton(1);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }


        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            if (NodeTarget.GetType() == typeof(DialogOption))
            {
                return true;
            }

            return false;
        }

        public override void CreateAutomaticNode()
        {
            NewAutomaticNode(typeof(DialogOption));
        }

        private int ShowVarSelectorGUI(List<GVar> vars, int ID)
        {
            variableNumber = -1;

            List<string> labelList = new List<string>();
            foreach (GVar _var in vars)
            {
                labelList.Add(_var.label);
            }

            variableNumber = GetVarNumber(vars, ID);

            if (variableNumber == -1)
            {
                // Wasn't found (variable was deleted?), so revert to zero
                ACDebug.LogWarning("Previously chosen variable no longer exists!");
                variableNumber = 0;
                ID = 0;
            }

            variableNumber = EditorGUILayout.Popup("Variable:", variableNumber, labelList.ToArray());
            ID = vars[variableNumber].id;

            return ID;
        }


        public string SetLabel()
        {
            if (location == VariableLocation.Local)
            {
                if (localVariables)
                {
                    return GetLabelString(localVariables.localVars);
                }
            }
            else
            {
                if (AdvGame.GetReferences().variablesManager)
                {
                    return GetLabelString(AdvGame.GetReferences().variablesManager.vars);
                }
            }

            return "";
        }


        private string GetLabelString(List<GVar> vars)
        {
            string labelAdd = "";

            if (vars.Count > 0 && vars.Count > variableNumber && variableNumber > -1)
            {
                labelAdd = " (" + vars[variableNumber].label;

                if (vars[variableNumber].type == VariableType.Boolean)
                {
                    labelAdd += " " + boolCondition.ToString() + " " + boolValue.ToString();
                }
                else if (vars[variableNumber].type == VariableType.Integer)
                {
                    labelAdd += " " + intCondition.ToString() + " " + intValue.ToString();
                }
                else if (vars[variableNumber].type == VariableType.Float)
                {
                    labelAdd += " " + intCondition.ToString() + " " + floatValue.ToString();
                }
                else if (vars[variableNumber].type == VariableType.String)
                {
                    labelAdd += " " + boolCondition.ToString() + " " + stringValue;
                }
                else if (vars[variableNumber].type == VariableType.PopUp)
                {
                    labelAdd += " " + intCondition.ToString() + " " + vars[variableNumber].popUps[intValue];
                }

                labelAdd += ")";
            }

            return labelAdd;
        }


        private int ShowVarGUI(List<ActionParameter> parameters, List<GVar> vars, int ID, bool changeID)
        {
            if (vars.Count > 0)
            {
                if (changeID)
                {
                    ID = ShowVarSelectorGUI(vars, ID);
                }
                variableNumber = Mathf.Min(variableNumber, vars.Count - 1);
                getVarMethod = (GetVarMethod)EditorGUILayout.EnumPopup("Compare with:", getVarMethod);

                if (parameters == null || parameters.Count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }

                if (vars[variableNumber].type == VariableType.Boolean)
                {
                    boolCondition = (BoolCondition)EditorGUILayout.EnumPopup(boolCondition);
                    if (getVarMethod == GetVarMethod.EnteredValue)
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Boolean:", parameters, checkParameterID, ParameterType.Boolean);
                        if (checkParameterID < 0)
                        {
                            EditorGUILayout.LabelField("Boolean:", GUILayout.MaxWidth(60f));
                            boolValue = (BoolValue)EditorGUILayout.EnumPopup(boolValue);
                        }
                    }
                }
                else if (vars[variableNumber].type == VariableType.Integer)
                {
                    intCondition = (IntCondition)EditorGUILayout.EnumPopup(intCondition);
                    if (getVarMethod == GetVarMethod.EnteredValue)
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Integer:", parameters, checkParameterID, ParameterType.Integer);
                        if (checkParameterID < 0)
                        {
                            EditorGUILayout.LabelField("Integer:", GUILayout.MaxWidth(60f));
                            intValue = EditorGUILayout.IntField(intValue);
                        }
                    }
                }
                else if (vars[variableNumber].type == VariableType.PopUp)
                {
                    intCondition = (IntCondition)EditorGUILayout.EnumPopup(intCondition);
                    if (getVarMethod == GetVarMethod.EnteredValue)
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Value:", parameters, checkParameterID, ParameterType.Integer);
                        if (checkParameterID < 0)
                        {
                            EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(60f));
                            intValue = EditorGUILayout.Popup(intValue, vars[variableNumber].popUps);
                        }
                    }
                }
                else if (vars[variableNumber].type == VariableType.Float)
                {
                    intCondition = (IntCondition)EditorGUILayout.EnumPopup(intCondition);
                    if (getVarMethod == GetVarMethod.EnteredValue)
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Float:", parameters, checkParameterID, ParameterType.Float);
                        if (checkParameterID < 0)
                        {
                            EditorGUILayout.LabelField("Float:", GUILayout.MaxWidth(60f));
                            floatValue = EditorGUILayout.FloatField(floatValue);
                        }
                    }
                }
                else if (vars[variableNumber].type == VariableType.String)
                {
                    boolCondition = (BoolCondition)EditorGUILayout.EnumPopup(boolCondition);
                    if (getVarMethod == GetVarMethod.EnteredValue)
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("String:", parameters, checkParameterID, ParameterType.String);
                        if (checkParameterID < 0)
                        {
                            EditorGUILayout.LabelField("String:", GUILayout.MaxWidth(60f));
                            stringValue = EditorGUILayout.TextField(stringValue);
                        }
                    }
                }

                if (getVarMethod == GetVarMethod.GlobalVariable)
                {
                    if (AdvGame.GetReferences().variablesManager == null || AdvGame.GetReferences().variablesManager.vars == null || AdvGame.GetReferences().variablesManager.vars.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No Global variables exist!", MessageType.Info);
                    }
                    else
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Global variable:", parameters, checkParameterID, ParameterType.GlobalVariable);
                        if (checkParameterID < 0)
                        {
                            compareVariableID = ShowVarSelectorGUI(AdvGame.GetReferences().variablesManager.vars, compareVariableID);
                        }
                    }
                }
                else if (getVarMethod == GetVarMethod.LocalVariable)
                {
                    if (localVariables == null || localVariables.localVars == null || localVariables.localVars.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No Local variables exist!", MessageType.Info);
                    }
                    else
                    {
                        checkParameterID = AC.Action.ChooseParameterGUI("Local variable:", parameters, checkParameterID, ParameterType.LocalVariable);
                        if (checkParameterID < 0)
                        {
                            compareVariableID = ShowVarSelectorGUI(localVariables.localVars, compareVariableID);
                        }
                    }
                }

                if (parameters == null || parameters.Count == 0)
                {
                    EditorGUILayout.EndHorizontal();
                }

                if (vars[variableNumber].type == VariableType.String)
                {
                    checkCase = EditorGUILayout.Toggle("Case-senstive?", checkCase);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No variables exist!", MessageType.Info);
                ID = -1;
                variableNumber = -1;
            }

            return ID;
        }



#endif


        private int GetVarNumber(List<GVar> vars, int ID)
        {
            int i = 0;
            foreach (GVar _var in vars)
            {
                if (_var.id == ID)
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }

}
