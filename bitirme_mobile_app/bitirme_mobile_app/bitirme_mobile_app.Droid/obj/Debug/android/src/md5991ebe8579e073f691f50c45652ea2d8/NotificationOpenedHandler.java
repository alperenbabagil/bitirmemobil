package md5991ebe8579e073f691f50c45652ea2d8;


public class NotificationOpenedHandler
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.NotificationOpenedHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_notificationOpened:(Lcom/onesignal/OSNotificationOpenResult;)V:GetNotificationOpened_Lcom_onesignal_OSNotificationOpenResult_Handler:Com.OneSignal.Android.OneSignal/INotificationOpenedHandlerInvoker, OneSignal.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("Com.OneSignal.NotificationOpenedHandler, Com.OneSignal, Version=3.0.1.0, Culture=neutral, PublicKeyToken=null", NotificationOpenedHandler.class, __md_methods);
	}


	public NotificationOpenedHandler () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NotificationOpenedHandler.class)
			mono.android.TypeManager.Activate ("Com.OneSignal.NotificationOpenedHandler, Com.OneSignal, Version=3.0.1.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void notificationOpened (com.onesignal.OSNotificationOpenResult p0)
	{
		n_notificationOpened (p0);
	}

	private native void n_notificationOpened (com.onesignal.OSNotificationOpenResult p0);

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
