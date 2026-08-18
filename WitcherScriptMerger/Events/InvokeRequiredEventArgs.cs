using System;
using System.Windows.Forms;

namespace WitcherScriptMerger.Events;
internal class InvokeRequiredEventArgs : EventArgs
{
	internal MethodInvoker Action { get; }

	internal InvokeRequiredEventArgs(MethodInvoker action) => Action = action;
}
