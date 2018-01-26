package md501d8f5dfb54d2c08bda2bb10b3ed891a;


public class Demo15GridLayout
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("CommonBasicControls.SrcActivity.Demo15GridLayout, CommonBasicControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Demo15GridLayout.class, __md_methods);
	}


	public Demo15GridLayout () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Demo15GridLayout.class)
			mono.android.TypeManager.Activate ("CommonBasicControls.SrcActivity.Demo15GridLayout, CommonBasicControls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
