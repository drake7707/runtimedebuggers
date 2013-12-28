using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RunTimeDebuggers.Helpers;
using System.Reflection;
using System.Runtime.CompilerServices;
using RunTimeDebuggers.AssemblyExplorer;

namespace RunTimeDebuggers.LocalsDebugger
{
    internal partial class ExpressionBox : UserControl
    {

        private Stack<string> history = new Stack<string>();
        private Stack<string> forwardHistory = new Stack<string>();


        private IntellisensePopup intellisense;

        private object thisObject;

        public ExpressionBox(object thisObject)
        {
            this.thisObject = thisObject;

            InitializeComponent();

            intellisense = new IntellisensePopup();
            intellisense.Show();
            intellisense.Hide();
            intellisense.Deactivate += new EventHandler(intellisense_Deactivate);
            intellisense.MemberChosen += new EventHandler(intellisense_MemberChosen);
        }

        internal delegate void ExpressionEvaluatedHandler(object sender, EvaluateArgs args);



        public class EvaluateArgs
        {
            public CSharpExpressionEvaluation.EvaluateResult Result { get; set; }
            public string Statement { get; set; }
        }

        internal event ExpressionEvaluatedHandler ExpressionEvaluated;
        protected virtual void OnExpressionEvaluated(EvaluateArgs args)
        {
            ExpressionEvaluatedHandler temp = ExpressionEvaluated;
            if (temp != null)
                temp(this, args);
        }

        private void txtEvaluate_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Enter)
                {

                    if (intellisense.Visible)
                    {
                        ChooseMemberFromIntellisense(true);
                    }
                    else
                    {
                        try
                        {
                            this.Enabled = false;

                            string statement = txtEvaluate.Text;
                            var value = StatementParser.EvaluateStatement(statement, thisObject, true, true, useDebugger: ModifierKeys == Keys.Control);

                            OnExpressionEvaluated(new EvaluateArgs() { Result = value, Statement = statement });

                            SetInput("");
                        }
                        catch (Exception ex)
                        {
                            ILDebugManager.Instance.Debugger = null;
                            MessageBox.Show("Unable to evaluate: " + ex.ToExceptionString());
                        }
                        finally
                        {
                            this.Enabled = true;
                        }
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.OemPeriod)
                {
                    if (intellisense.Visible)
                    {
                        ChooseMemberFromIntellisense(false);
                    }
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    if (intellisense.Visible)
                    {
                        if (e.KeyCode == Keys.Up)
                            intellisense.Up();
                        else
                            intellisense.Down();

                        e.Handled = true;
                    }
                    else
                    {
                        if (e.KeyCode == Keys.Up)
                            GoBack();
                        else
                            GoForward();

                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown)
                {
                    if (intellisense.Visible)
                    {
                        if (e.KeyCode == Keys.PageUp)
                            intellisense.PageUp();
                        else
                            intellisense.PageDown();

                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    if (intellisense.Visible)
                        intellisense.Hide();
                }


            }
            catch (Exception)
            {
                // don't go crashing the app
            }

        }

        public void SetInput(string statement)
        {

            if (!string.IsNullOrEmpty(txtEvaluate.Text))
                history.Push(txtEvaluate.Text);
            forwardHistory.Clear();
            txtEvaluate.Text = statement;
        }


        private void txtEvaluate_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control && e.KeyChar == ' ')
                {
                    this.ShowIntellisense();
                    e.Handled = true;
                }

                else if (e.KeyChar == '(' || e.KeyChar == '<' || e.KeyChar == '[')
                {
                    var member = intellisense.GetSelectedMember();
                    if (member is MethodInfo)
                    {
                        ChooseMemberFromIntellisense(false);
                    }
                    else
                        intellisense.Hide();
                }
                else if (e.KeyChar == ' ')
                {
                    ChooseMemberFromIntellisense(false);
                }
            }
            catch (Exception)
            {
                // don't go crashing the app
            }
        }

        private void txtEvaluate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtEvaluate.SelectionStart - 1 >= txtEvaluate.Text.Length)
                    return;

                var typeAndAfter = StatementParser.GetTypeOfCurrentStatement(txtEvaluate.Text, txtEvaluate.SelectionStart - 1, thisObject, false);


                if (typeAndAfter.Key != null)
                {
                    if (intellisense.CurrentType != typeAndAfter.Key)
                    {
                        Type fromType = typeAndAfter.Key;
                        UpdateIntellisense(fromType);
                    }

                    if (txtEvaluate.Text.EndsWith("."))
                    {
                        ShowIntellisense();
                    }

                    intellisense.SelectCurrentPart(typeAndAfter.Value);
                }
            }
            catch (Exception)
            {
                if (intellisense.Visible)
                {
                    intellisense.SetItems(new List<IntellisensePopup.ListItem>());
                    intellisense.CurrentType = null;
                    intellisense.Hide();
                }
            }

            try
            {
                if (txtEvaluate.SelectionStart - 1 >= txtEvaluate.Text.Length)
                    return;

                var methods = StatementParser.GetCurrentMethodOverloads(txtEvaluate.Text, txtEvaluate.SelectionStart - 1, thisObject);
                if (methods.Count > 0)
                {
                    string methodSignatures = string.Join(Environment.NewLine, methods.Select(m => m.ToSignatureString()).ToArray());

                    Point p = txtEvaluate.PointToScreen(txtEvaluate.GetPositionFromCharIndex(txtEvaluate.SelectionStart - 1));
                    p.Offset(16, -16 + -methods.Count * 10);
                    p.Offset(-this.Location.X, -this.Location.Y);

                    methodsToolTip.Show(methodSignatures, this, p);
                }
                else
                    methodsToolTip.Hide(this);
            }
            catch (Exception)
            {
                methodsToolTip.Hide(this);

            }
        }
        private void UpdateIntellisense(Type fromType)
        {

            var items = TypeHelper.GetAllMembers(fromType)
                                  .Concat(StatementParser.Store.GetExtensionMethodsOf(fromType).ToArray())
                                  .Where(m => !(m.Name.Contains('<') || m.Name.Contains('>'))) // compiler generated
                                  .GroupBy(m =>
                                  {
                                      if (m is FieldInfo)
                                          return new KeyValuePair<string, string>("field", m.Name);
                                      else if (m is PropertyInfo)
                                          return new KeyValuePair<string, string>("property", m.Name);
                                      else if (m is MethodInfo)
                                      {
                                          if (m.IsDefined(typeof(ExtensionAttribute), false))
                                              return new KeyValuePair<string, string>("extmethod", m.Name);
                                          else
                                              return new KeyValuePair<string, string>("method", m.Name);
                                      }

                                      return default(KeyValuePair<string, string>);
                                  })
                                  .SelectMany(m =>
                                  {
                                      List<IntellisensePopup.ListItem> members = new List<IntellisensePopup.ListItem>();
                                      members.Add(new IntellisensePopup.ListItem(m.First()));
                                      foreach (var member in m)
                                      {
                                          string alias = AliasManager.Instance.GetAlias(member);
                                          if (!string.IsNullOrEmpty(alias))
                                              members.Add(new IntellisensePopup.ListItem(member, alias));
                                      }
                                      return members;
                                  })
                                  .OrderBy(itm => itm.ToString())
                                  .ToArray();

            intellisense.SetItems(items);

            intellisense.CurrentType = fromType;
        }

        public void UpdateIntellisense()
        {
            Type fromType = intellisense.CurrentType;
            
            if (fromType != null)
                UpdateIntellisense(fromType);
        }

        private void ChooseMemberFromIntellisense(bool canChooseWhenEmpty)
        {
            if (!intellisense.Visible)
                return;

            var selectedMember = intellisense.GetSelectedMember();
            if (selectedMember != null)
            {
                // replace current part with selected member
                var typeAndAfter = StatementParser.GetTypeOfCurrentStatement(txtEvaluate.Text, txtEvaluate.SelectionStart - 1, thisObject, false);


                if ((!canChooseWhenEmpty && string.IsNullOrEmpty(typeAndAfter.Value)) || StatementParser.GetStringsNotToAutocompleteOn().Contains(typeAndAfter.Value))
                    return;

                string name;
                string alias = AliasManager.Instance.GetAlias(selectedMember);
                if (!string.IsNullOrEmpty(alias))
                    name = alias;
                else
                    name = selectedMember.Name;

                if (name.ToLower().StartsWith(txtEvaluate.Text.Substring(txtEvaluate.Text.Length - typeAndAfter.Value.Length).ToLower()))
                {
                    txtEvaluate.Text = txtEvaluate.Text.Substring(0, txtEvaluate.Text.Length - typeAndAfter.Value.Length) + name;
                    txtEvaluate.SelectionStart = txtEvaluate.TextLength;
                }

                intellisense.Hide();
            }
        }

        private void ShowIntellisense()
        {
            ignoreDeactivate = true;
            try
            {
                if (intellisense.ItemCount > 0)
                {
                    intellisense.Show();
                    intellisense.BringToFront();
                    txtEvaluate.Focus();
                    Point p = txtEvaluate.PointToScreen(txtEvaluate.GetPositionFromCharIndex(txtEvaluate.SelectionStart - 1));
                    p.Offset(16, 16);
                    intellisense.Location = p;
                }
            }
            finally
            {
                ignoreDeactivate = false;
            }
        }

        void intellisense_MemberChosen(object sender, EventArgs e)
        {
            try
            {
                ChooseMemberFromIntellisense(true);
            }
            catch (Exception)
            {

            }
        }

        void intellisense_Deactivate(object sender, EventArgs e)
        {
            if (!ignoreDeactivate)
            {
                if (intellisense.Visible)
                    intellisense.Hide();
            }
        }


        private bool ignoreDeactivate;


        public void GoBack()
        {
            if (HasHistory)
            {
                var n = history.Pop();
                if (forwardHistory.Count <= 0 || forwardHistory.Peek() != n)
                    forwardHistory.Push(txtEvaluate.Text);

                txtEvaluate.Text = n;
                txtEvaluate.SelectionStart = txtEvaluate.Text.Length;
            }
        }

        public void GoForward()
        {
            if (HasForwardHistory)
            {
                var n = forwardHistory.Pop();

                if (history.Count <= 0 || history.Peek() != n)
                    history.Push(txtEvaluate.Text);

                txtEvaluate.Text = n;
                txtEvaluate.SelectionStart = txtEvaluate.Text.Length;
            }
        }

        public bool HasHistory { get { return history.Count > 0; } }
        public bool HasForwardHistory { get { return forwardHistory.Count > 0; } }


        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing && intellisense != null)
            {
                intellisense.Close();
                intellisense.Dispose();
                intellisense = null;
            }
            base.Dispose(disposing);
        }
    }
}
