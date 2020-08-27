using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegatePool {
	public delegate void VoidNoneParam();
	public delegate void VoidOneParam(object obj);
	public delegate void VoidArrayParam(object[] objs);
	public delegate bool BoolOneParam(object obj);
	public delegate bool BoolArrayParam(object[] objs);
}
