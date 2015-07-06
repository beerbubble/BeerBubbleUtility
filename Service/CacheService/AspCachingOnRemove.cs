using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace BeerBubbleUtility
{
	public class AspCachingOnRemove
	{
		public delegate void RemoveHandler( object sender, RemoveEventArgs e );
		public event RemoveHandler RemoveEvent;
		
		public void RemovedCallback( String key, Object expiredValue, CacheItemRemovedReason reason )
		{
			if ( RemoveEvent != null )
			{
				RemoveEvent ( this, new RemoveEventArgs ( key, expiredValue ) );
			}

			IDisposable disposableObject = expiredValue as IDisposable;
			if ( disposableObject != null )
			{
				disposableObject.Dispose ();
			}

		}
	}
}
